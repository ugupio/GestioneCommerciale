using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using SerramentiConfigurator.Data;
using SerramentiConfigurator.Models;
using SerramentiConfigurator.Services;


namespace SerramentiConfigurator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfiliController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfiliController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("importare-profili")]
        public async Task<IActionResult> ImportaProfiliDaExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File di esportazione profili non valido.");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Carica tutte le serie in un dizionario in memoria per massimizzare le prestazioni
            var serieDizionario = await _context.SerieSerramenti
                .ToDictionaryAsync(s => s.Id, s => s.LunghezzaBarraDefault);

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int inseriti = 0;
                    int aggiornati = 0;
                    int saltatiFormato = 0;
                    int saltatiSerieMancante = 0;

                    var idSerieMancantiLog = new HashSet<int>();
                    bool primaRigaIntestazione = true;

                    // Indici posizionali delle colonne (inizializzati a -1 = non trovata)
                    int colSerieId = -1;
                    int colCodice = -1;
                    int colDescr = -1;
                    int colCosto = -1;
                    int colKg = -1;

                    while (reader.Read())
                    {
                        // 1. SCANSIONE INTESTAZIONE: Identifica dinamicamente le colonne reali di FP Pro
                        if (primaRigaIntestazione)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                // Converte tutto in minuscolo per azzerare i problemi di maiuscole/minuscole
                                string nomeCol = reader.GetValue(i)?.ToString()?.Trim()?.ToLower();
                                if (string.IsNullOrEmpty(nomeCol)) continue;

                                // CONTROLLO ID SERIE (Trova 'pkid' derivato dal tuo 'PkId')
                                if (nomeCol == "pkid" || nomeCol == "serieid" || nomeCol == "idserie" || nomeCol == "id_serie")
                                    colSerieId = i;

                                // CONTROLLO CODICE ARTICOLO (Verifica anche la colonna 'serie' se usata per il codice)
                                else if (nomeCol == "codice" || nomeCol == "codicearticolo" || nomeCol == "cod_art" || nomeCol == "serie")
                                    colCodice = i;

                                // CONTROLLO DESCRIZIONE
                                else if (nomeCol == "descr" || nomeCol == "descrizione" || nomeCol == "des_art")
                                    colDescr = i;

                                // CONTROLLO COSTO AL METRO
                                else if (nomeCol == "costo_ml" || nomeCol == "prezzofisso" || nomeCol == "costo")
                                    colCosto = i;

                                // CONTROLLO PESO AL METRO
                                else if (nomeCol == "kg_ml" || nomeCol == "pesofisso" || nomeCol == "peso")
                                    colKg = i;
                            }

                            // Se mancano i campi minimi, restituisce l'elenco esatto rilevato per debug
                            if (colSerieId == -1 || colCodice == -1)
                            {
                                var colonneRilevate = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                    colonneRilevate.Add(reader.GetValue(i)?.ToString() ?? $"Vuota({i})");

                                return BadRequest($"Struttura non riconosciuta. Colonne lette: {string.Join(", ", colonneRilevate)}. Verifica la mappatura per 'pkid' o 'serie'.");
                            }

                            primaRigaIntestazione = false;
                            continue;
                        }

                        // 2. ESTRAZIONE DINAMICA DEI DATI BASATA SUI REALI INDICI RILEVATI
                        var serieIdStr = reader.GetValue(colSerieId)?.ToString();
                        var codice = reader.GetValue(colCodice)?.ToString()?.Trim();

                        // Campi opzionali gestiti con fallback sicuro se la colonna manca nell'Excel
                        var descr = colDescr != -1 ? reader.GetValue(colDescr)?.ToString()?.Trim() : null;
                        var costoMlStr = colCosto != -1 ? reader.GetValue(colCosto)?.ToString() : "0";
                        var kgMlStr = colKg != -1 ? reader.GetValue(colKg)?.ToString() : "0";

                        // Validazione del record corrente
                        if (string.IsNullOrEmpty(codice) || !int.TryParse(serieIdStr, out int idSerie))
                        {
                            saltatiFormato++;
                            continue;
                        }

                        // Verifica la presenza della serie padre nel dizionario web caricato all'avvio
                        if (!serieDizionario.TryGetValue(idSerie, out double lunghezzaBarraCalcolata))
                        {
                            saltatiSerieMancante++;
                            idSerieMancantiLog.Add(idSerie);
                            continue;
                        }

                        double.TryParse(kgMlStr, out double kgMl);
                        decimal.TryParse(costoMlStr, out decimal costoMl);

                        // Logica di classificazione automatica per i menu del configuratore
                        string tipoNormalizzato = "Telaio";
                        if (!string.IsNullOrEmpty(descr))
                        {
                            string descrLower = descr.ToLower();
                            if (descrLower.Contains("anta")) tipoNormalizzato = "Anta";
                            else if (descrLower.Contains("ferma") || descrLower.Contains("fv")) tipoNormalizzato = "Fermavetro";
                            else if (descrLower.Contains("inv") || descrLower.Contains("riporto")) tipoNormalizzato = "Inversore";
                            else if (descrLower.Contains("traverso") || descrLower.Contains("t ")) tipoNormalizzato = "TraversoT";
                            else if (descrLower.Contains("zoccolo")) tipoNormalizzato = "Zoccolo";
                        }

                        // Cerca se la chiave accoppiata CodiceArticolo + SerieSerramentoId esiste già (Upsert Idempotente)
                        var profiloEsistente = await _context.ProfiliCatalogo
                            .FirstOrDefaultAsync(p => p.CodiceArticolo == codice && p.SerieSerramentoId == idSerie);

                        if (profiloEsistente != null)
                        {
                            profiloEsistente.Descrizione = descr ?? profiloEsistente.Descrizione;
                            profiloEsistente.PrezzoAlMetro = costoMl > 0 ? costoMl : profiloEsistente.PrezzoAlMetro;
                            profiloEsistente.PesoAlMetro = kgMl > 0 ? kgMl : profiloEsistente.PesoAlMetro;
                            profiloEsistente.TipoProfilo = tipoNormalizzato;
                            profiloEsistente.LunghezzaCommercialeBarra = lunghezzaBarraCalcolata;

                            _context.ProfiliCatalogo.Update(profiloEsistente);
                            aggiornati++;
                        }
                        else
                        {
                            var nuovoProfilo = new ProfiloCatalogo
                            {
                                SerieSerramentoId = idSerie,
                                CodiceArticolo = codice,
                                Descrizione = descr ?? $"Profilo {codice}",
                                TipoProfilo = tipoNormalizzato,
                                PrezzoAlMetro = costoMl > 0 ? costoMl : 1.00m,
                                PesoAlMetro = kgMl,
                                LunghezzaCommercialeBarra = lunghezzaBarraCalcolata
                            };

                            await _context.ProfiliCatalogo.AddAsync(nuovoProfilo);
                            inseriti++;
                        }
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        Messaggio = "Importazione dinamica completata senza alcun duplicato.",
                        RecordInseriti = inseriti,
                        RecordAggiornati = aggiornati,
                        SaltatiErroreFormato = saltatiFormato,
                        SaltatiPerSerieMancante = saltatiSerieMancante,
                        IdSerieDaImportarePrima = idSerieMancantiLog.OrderBy(x => x).ToList()
                    });
                }
            }
        }

        // =========================================================================
        // ENDPOINT DI TEST: SIMULAZIONE CALCOLO PREVENTIVO COMMERCIALE B2B
        // =========================================================================
        [HttpPost("simulare-preventivo-test")]
        public async Task<IActionResult> SimularePreventivoTest([FromBody] RichiestaSimulazioneTest richiesta)
        {
            if (richiesta == null)
                return BadRequest("Dati della simulazione non validi.");

            // 1. Recupera la finitura dal database per estrarre il costo di verniciatura e la penale Italver
            var finitura = await _context.FiniturePrezzi
                .Include(f => f.Impianto)
                .FirstOrDefaultAsync(f => f.CodiceFinitura == richiesta.CodiceFinituraScelta);

            if (finitura == null)
                return NotFound($"Finitura con codice '{richiesta.CodiceFinituraScelta}' non trovata a database.");

            // 2. Recupera il prezzo base della serie RX450 (ID 12) per i profili commerciali (+0.50€/Kg)
            var serieRx450 = await _context.SerieSerramenti.FirstOrDefaultAsync(s => s.Id == 12);
            decimal prezzoGrezzoRx450 = serieRx450?.PrezzoGrezzoAlKg ?? 7.62m;

            // 3. Inizializza il motore di calcolo commerciale isolato
            var motore = new Server.Services.MotoreCalcoloCommerciale();
            var listaProfiliInput = new List<Server.Services.VoceProfiloInput>();

            foreach (var p in richiesta.Profili)
            {
                var profiloDb = await _context.ProfiliCatalogo
                    .Include(pDb => pDb.SerieSerramento)
                    .FirstOrDefaultAsync(pDb => pDb.CodiceArticolo == p.CodiceArticolo);

                decimal prezzoGrezzoSerie = 0.00m;
                double pesoAlMl = p.PesoAlMlMock;
                bool isCommerciale = p.IsProfiloCommercialeMock;

                if (profiloDb != null)
                {
                    prezzoGrezzoSerie = profiloDb.SerieSerramento?.PrezzoGrezzoAlKg ?? 0.00m;
                    pesoAlMl = profiloDb.PesoAlMetro;
                    isCommerciale = profiloDb.IsProfiloCommerciale;
                }
                else
                {
                    // Fallback Mock se l'anagrafica profili non è ancora caricata a DB
                    var serieSelezionata = await _context.SerieSerramenti.FirstOrDefaultAsync(s => s.Id == p.SerieSerramentoIdMock);
                    prezzoGrezzoSerie = serieSelezionata?.PrezzoGrezzoAlKg ?? 0.00m;
                }

                // Mappatura sicura verso la struttura a tipi primitivi isolati del server
                listaProfiliInput.Add(new Server.Services.VoceProfiloInput
                {
                    MetriLineari = p.MetriLineariTotali,
                    PesoAlMl = pesoAlMl,
                    IsProfiloCommerciale = isCommerciale,
                    PrezzoGrezzoSeriePadre = prezzoGrezzoSerie
                });
            }

            // 4. Esegue il calcolo dell'alluminio al KG passando i parametri disaccoppiati
            var risultatoAlluminio = motore.CalcolaSezioneAlluminio(
                listaProfiliInput,
                finitura.SovrapprezzoVerniciaturaAlKg,
                finitura.SogliaPesoKg,
                finitura.ContributoSottoSoglia,
                finitura.ContributoSopraSoglia,
                richiesta.ScontoTrattativaProfili,
                prezzoGrezzoRx450
            );

            // 5. CALCOLO ACCESSORI: Logica lineare a quantità con sconto ferramenta indipendente
            decimal totaleLordoAccessori = 0;
            foreach (var acc in richiesta.Accessori)
            {
                totaleLordoAccessori += acc.Quantita * acc.PrezzoListinoUnitario;
            }
            decimal fattoreScontoAccessori = 1 - (richiesta.ScontoTrattativaAccessori / 100);
            decimal totaleNettoAccessori = Math.Round(totaleLordoAccessori * fattoreScontoAccessori, 2);

            // 6. GENERAZIONE REPORT FINANZIARIO COMMERCIALE DI RISPOSTA
            return Ok(new
            {
                DettaglioImpianto = new { ImpiantoNome = finitura.Impianto?.Nome, FinituraFascia = finitura.FasciaPrezzo },
                SezioneAlluminio = new
                {
                    PesoTotaleAlluminioKg = risultatoAlluminio.PesoTotaleAlluminioKg,
                    ValoreLordoAlluminio = risultatoAlluminio.LordoProfiliAlKg,
                    ScontoApplicatoProfili = richiesta.ScontoTrattativaProfili,
                    ValoreNettoAlluminioScontato = risultatoAlluminio.NettoProfiliAlKg,

                    // Contributo fisso d'allestimento Italver escluso dallo sconto profili
                    AddebitoFissoAllestimentoImpianto = risultatoAlluminio.ContributoAllestimentoImpianto,
                    TotaleFatturatoAlluminio = risultatoAlluminio.TotaleFatturatoProfili
                },
                SezioneAccessori = new
                {
                    TotaleLordoFerramenta = totaleLordoAccessori,
                    ScontoApplicatoAccessori = richiesta.ScontoTrattativaAccessori,
                    TotaleNettoFerramenta = totaleNettoAccessori
                },
                TotaleGeneraleNettoCommessa = risultatoAlluminio.TotaleFatturatoProfili + totaleNettoAccessori
            });
        }

    }

    // === CLASSI DI STRUTTURA PER IL PAYLOAD JSON DELLA RICHIESTA DI SIMULAZIONE ===





}
