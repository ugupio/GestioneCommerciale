using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExcelDataReader;
using SerramentiConfigurator.Data;
using SerramentiConfigurator.Models;
using Microsoft.EntityFrameworkCore;

namespace SerramentiConfigurator.Controllers // Allineato: rimosso .Server
{
    [ApiController]
    [Route("api/[controller]")]
    public class SerieController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SerieController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("importare-excel")]
        public async Task<IActionResult> ImportaSerieDaExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File Excel non valido o vuoto.");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int righeInserite = 0;
                    int righeAggiornate = 0;
                    bool primaRigaIntestazione = true;

                    while (reader.Read())
                    {
                        if (primaRigaIntestazione)
                        {
                            primaRigaIntestazione = false;
                            continue;
                        }

                        var pkIdStr = reader.GetValue(0)?.ToString();
                        var nomeSerie = reader.GetValue(1)?.ToString();

                        if (int.TryParse(pkIdStr, out int pkId) && !string.IsNullOrEmpty(nomeSerie))
                        {
                            var serieEsistente = _context.SerieSerramenti.FirstOrDefault(s => s.Id == pkId);

                            if (serieEsistente != null)
                            {
                                serieEsistente.CodiceSerie = nomeSerie.Trim();
                                righeAggiornate++;
                            }
                            else
                            {
                                double lunghezzaBase = nomeSerie.Contains("CX") ? 6.8 : 6.5;

                                var nuovaSerie = new SerieSerramento
                                {
                                    Id = pkId,
                                    CodiceSerie = nomeSerie.Trim(),
                                    Descrizione = $"Serie {nomeSerie.Trim()} importata da FP Pro",
                                    TipoSistema = nomeSerie.Contains("CX") ? "Taglio Termico" : "Freddo/Giunto Aperto",
                                    LunghezzaBarraDefault = lunghezzaBase
                                };

                                _context.SerieSerramenti.Add(nuovaSerie);
                                righeInserite++;
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        Messaggio = "Importazione completata con successo!",
                        Inseriti = righeInserite,
                        Aggiornati = righeAggiornate
                    });
                }
            }
        }
    }
}
