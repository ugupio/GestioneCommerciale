using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SerramentiConfigurator.Models



{


    // ==========================================
    // 1. ANAGRAFICA CLIENTE
    // ==========================================
    public class Cliente
    {
        public int Id { get; set; }
        public string RagioneSociale { get; set; }
        public string PartitaIva { get; set; }
        public string Email { get; set; }
        public List<Cantiere> Cantieri { get; set; } = new();

        // NUOVI CAMPI COMMERCIALI: Sconti di listino predefiniti (es. 40.00 = 40%)
        public decimal ScontoStandardProfili { get; set; } = 0;
        public decimal ScontoStandardAccessori { get; set; } = 0;
    }


    // ==========================================
    // 2. CONTENITORE PROGETTO (CANTIERE)
    // ==========================================
    public class Cantiere
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Localita { get; set; }
        public string Stato { get; set; } = "In Corso"; // In Corso, Completato, In Attesa
        public bool IsPinned { get; set; }
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        // Relazione verso le commesse interne (es. Piano 1, Lotto A)
        public List<Commessa> Commesse { get; set; } = new();
    }

    // ==========================================
    // 3. IL LOTTO DI LAVORO (COMMESSA)
    // ==========================================
    public class Commessa
    {
        public int Id { get; set; }
        public int CantiereId { get; set; }
        public string TitoloCommessa { get; set; } // es. "Infissi Piano Terra" o "Blocco A"
        public string Note { get; set; }

        // NUOVI CAMPI COMMERCIALI: Sconti reali applicati a questa specifica trattativa
        public decimal ScontoProfiliApplicato { get; set; } = 0;
        public decimal ScontoAccessoriApplicato { get; set; } = 0;

        // Relazione verso i singoli infissi inseriti
        public List<ElementoInfisso> Elementi { get; set; } = new();
    }


    // ==========================================
    // 4. IL SINGOLO SERRAMENTO (CONFIGURATO)
    // ==========================================
    public class ElementoInfisso
    {
        public int Id { get; set; }
        public int CommessaId { get; set; }
        public string EtichettaVano { get; set; } // es. "Aula 10", "Ufficio", "Cucina"
        public string TipologiaApertura { get; set; } // es. "1 Anta", "2 Ante", "Fisso", "Vasistas"
        public string SerieProfilo { get; set; } // es. "CX650", "RX450"
        public string Colore { get; set; } // Codice RAL o Finitura Legno

        // Dimensioni millimetriche inserite dall'utente
        public int Larghezza { get; set; }
        public int Altezza { get; set; }
        public int Quantita { get; set; } = 1;

        // Scelte vincolate inserite dall'utente nel configuratore
        public string TipoSoglia { get; set; }
        public string TipoManiglia { get; set; }
        public string TipoVetro { get; set; }

        // --- 🔲 NUOVI PARAMETRI STRUTTURALI B2B (Aggiungere qui per sanare CS0117) ---
        public bool TelaioSuperiore { get; set; } = true;
        public bool TelaioInferiore { get; set; } = true;
        public bool TelaioSinistro { get; set; } = true;
        public bool TelaioDestro { get; set; } = true;

        public int NumeroAnte { get; set; } = 1;
        public bool AntaHaZoccoloInferiore { get; set; } = false;
        public int AltezzaZoccolo { get; set; } = 150;

        public bool HaSopraluce { get; set; } = false;
        public int AltezzaSopraluce { get; set; } = 400;
        public bool HaFiancoluce { get; set; } = false;
        public int LarghezzaFiancoluce { get; set; } = 500;

        public string TipoAperturaFerramenta { get; set; } = "Cremonese";
        public string VersoApertura { get; set; } = "Destra";
        public int NumeroCerniere { get; set; } = 3;
    }




    // 1. Catalogo Generale dei Profili in Alluminio
    public class ProfiloCatalogo
    {
        public int Id { get; set; }

        // Mappatura "SerieId" di FP Pro (Chiave Esterna)
        public int SerieSerramentoId { get; set; }

        // Relazione virtuale per navigare verso la serie padre nel motore di calcolo
        public SerieSerramento SerieSerramento { get; set; }

        // Mappatura "Codice" di FP Pro
        public string CodiceArticolo { get; set; }

        // Mappatura "Descr" di FP Pro
        public string Descrizione { get; set; }

        // Identifica l'uso nel configuratore (Telaio, Anta, Fermavetro...)
        public string TipoProfilo { get; set; }

        // Mappatura "Costo_ml" di FP Pro
        public decimal PrezzoAlMetro { get; set; }

        // Mappatura "Kg_ml" di FP Pro
        public double PesoAlMetro { get; set; }

        // --- CAMPO AGGIUNTIVO PER L'ORDINE COMMERCIALE B2B ---
        // Lunghezza commerciale fissa memorizzata (es. 6.8 o 6.5 metri)
        public double LunghezzaCommercialeBarra { get; set; } = 6.5;

        // --- NUOVO CAMPO: Attiva la maggiorazione di +0.50€/Kg ereditando la base RX450 ---
        public bool IsProfiloCommerciale { get; set; } = false;
    }


    // 2. Catalogo Generale degli Accessori e Ferramenta
    public class AccessorioCatalogo
    {
        public int Id { get; set; }
        public string CodiceArticolo { get; set; }
        public string Descrizione { get; set; }
        public string UnitaMisura { get; set; }
        public decimal PrezzoUnitario { get; set; }
    }

    // 3. Matrice di Compatibilità / Regole di Associazione Accessori
    // Definisce quali accessori servono e quanti pezzi calcolare in base alle scelte tecniche
    public class RegolaFerramenta
    {
        public int Id { get; set; }
        public string SerieProfilo { get; set; }
        public string TipoAperturaFerramenta { get; set; }
        public string CodiceAccessorioCorrelato { get; set; }
        public string LogicaCalcolo { get; set; }
        public int QuantitaMoltiplicatore { get; set; }
    }

    public class SerieSerramento
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string CodiceSerie { get; set; }
        public string Descrizione { get; set; }
        public string TipoSistema { get; set; }
        public double LunghezzaBarraDefault { get; set; } = 6.8;

        // NUOVO CAMPO: Il valore puro del metallo non lavorato (es. 7.62 per RX450)
        public decimal PrezzoGrezzoAlKg { get; set; } = 0.00m;

        public List<ProfiloCatalogo> Profili { get; set; } = new();
    }

    public class ImpiantoVerniciatura
    {
        public int Id { get; set; }
        public string Nome { get; set; } // Es. "Impianto Interno Vert", "Impianto Subfornitura"
        public string Descrizione { get; set; }

        // Relazione uno-a-molti verso le tariffe delle finiture lavorate da questo impianto
        public List<FinituraPrezzo> FinitureGestite { get; set; } = new();
    }

    public class FinituraPrezzo
    {
        public int Id { get; set; }
        public string CodiceFinitura { get; set; }
        public string Descrizione { get; set; }// Es. "RAL5014"
        public string Sottogruppo { get; set; }           // Es. "Colori Costa Azzurra"
        public string FasciaPrezzo { get; set; }          // Destinato al listino al KG futuro
        public string CodiceEsadecimale { get; set; }

        // Tariffa base di lavorazione al KG
        public decimal SovrapprezzoVerniciaturaAlKg { get; set; }

        // GESTIONE CONTRIBUZIONE FISSA ALLA COMMESSA (ITALVER)
        public double SogliaPesoKg { get; set; }          // Es. 100.00 o 500.00
        public decimal ContributoSottoSoglia { get; set; } // Es. 300.00
        public decimal ContributoSopraSoglia { get; set; } // Es. 150.00 o 0.00

        public int ImpiantoVerniciaturaId { get; set; }
        public ImpiantoVerniciatura Impianto { get; set; }
    }




}









