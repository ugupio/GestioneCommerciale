using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestioneCommerciale.Models;
using System.IO;

namespace GestioneCommerciale.Models
{
    public class ReportRiunioneDocument : IDocument
    {
        private readonly List<ReportVisitaRiunione> _visite;
        private readonly DateTime _inizio;
        private readonly DateTime _fine;
        private readonly byte[] _mappaBytes;

        public ReportRiunioneDocument(List<ReportVisitaRiunione> visite, DateTime inizio, DateTime fine, byte[] mappaBytes)
        {
            _visite = visite ?? new List<ReportVisitaRiunione>();
            _inizio = inizio;
            _fine = fine;
            _mappaBytes = mappaBytes;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Element(x => ComposeHeader(x));
                page.Content().Element(x => ComposeContent(x));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Pagina ");
                    x.CurrentPageNumber();
                    x.Span(" di ");
                    x.TotalPages();
                });
            });
        }

        void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string pathAlq = Path.Combine(baseDir, "wwwroot", "images", "logo-alq.jpg");
            string pathTwin = Path.Combine(baseDir, "wwwroot", "images", "logo-twin.png");

            if (!File.Exists(pathAlq))
            {
                string projectRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName ?? baseDir;
                pathAlq = Path.Combine(projectRoot, "wwwroot", "images", "logo-alq.jpg");
                pathTwin = Path.Combine(projectRoot, "wwwroot", "images", "logo-twin.png");
            }

            container.Row(row =>
            {
                row.ConstantItem(80).Element(e => {
                    if (File.Exists(pathAlq)) e.Image(pathAlq);
                    else e.Placeholder();
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignCenter().Text("ANALISI COMMERCIALE STRATEGICA").FontSize(18).ExtraBold().FontColor(Colors.Blue.Medium);
                    col.Item().AlignCenter().Text($"Periodo: {_inizio:dd/MM/yyyy} - {_fine:dd/MM/yyyy}").FontSize(10).Italic();
                });

                row.ConstantItem(80).Element(e => {
                    if (File.Exists(pathTwin)) e.Image(pathTwin);
                    else e.Placeholder();
                });
            });
        }

        void ComposeContent(QuestPDF.Infrastructure.IContainer container)
        {
            container.Column(column =>
            {
                // --- PAGINA 1: COPERTINA ---
                column.Item().PaddingTop(40).AlignCenter().Text("SINTESI DELLE ATTIVITÀ").FontSize(24).ExtraBold().FontColor(Colors.Blue.Medium);
                column.Item().AlignCenter().Text("Analisi Territoriale e Potenziali di Vendita").FontSize(12).FontColor(Colors.Grey.Medium);
                column.Item().PaddingVertical(30).LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                column.Item().Row(row =>
                {
                    var totali = _visite.Count;
                    var nuovi = _visite.Count(x => x.IsNuovo);
                    var comuni = _visite.Select(x => x.Citta_Legale ?? "").Distinct().Count();

                    row.RelativeItem().Element(c => CreateKpiCard(c, "VISITE TOTALI", totali.ToString(), Colors.Blue.Medium));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => CreateKpiCard(c, "COMUNI COPERTI", comuni.ToString(), Colors.Grey.Medium));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => CreateKpiCard(c, "NUOVI CLIENTI PROSPECT", $"+{nuovi}", Colors.Green.Medium));
                });

                // --- GRAFICO EVOLUTO: VISITE + NUOVI CLIENTI ---
                column.Item().PaddingTop(30).Element(container =>
                {
                    container.Column(c =>
                    {
                        c.Item().PaddingBottom(10).Text("DISTRIBUZIONE TERRITORIALE E SVILUPPO NUOVI CLIENTI").FontSize(11).Bold().FontColor(Colors.Blue.Medium);

                        // 1. Raggruppamento dati con conteggio Nuovi (Prospect)
                        var statisticheProv = _visite
                            .GroupBy(v => v.Prov_Legale ?? "N.D.")
                            .Select(g => new {
                                Provincia = g.Key,
                                Totali = g.Count(),
                                Nuovi = g.Count(x => x.IsNuovo) // Conta i nuovi clienti
                            })
                            .OrderByDescending(x => x.Totali)
                            .ToList();

                        if (statisticheProv.Any())
                        {
                            var maxVisite = (float)statisticheProv.Max(x => x.Totali);

                            c.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(15).Background(Colors.Grey.Lighten5).Column(colStat =>
                            {
                                foreach (var stat in statisticheProv)
                                {
                                    colStat.Item().PaddingVertical(5).Row(row =>
                                    {
                                        // Nome Provincia
                                        row.ConstantItem(60).Text(stat.Provincia).FontSize(10).SemiBold();

                                        // Contenitore Barre (una sopra l'altra per ogni provincia)
                                        row.RelativeItem().Column(barre =>
                                        {
                                            // BARRA BLU (Visite Totali)
                                            barre.Item().Height(8).Background(Colors.Grey.Lighten3).Row(r => {
                                                r.RelativeItem(stat.Totali / maxVisite).Background(Colors.Blue.Medium);
                                                if (stat.Totali < maxVisite) r.RelativeItem(1 - (stat.Totali / maxVisite));
                                            });

                                            // BARRA VERDE (Nuovi Clienti) - Solo se > 0
                                            if (stat.Nuovi > 0)
                                            {
                                                barre.Item().PaddingTop(2).Height(8).Background(Colors.Grey.Lighten3).Row(r => {
                                                    r.RelativeItem(stat.Nuovi / maxVisite).Background(Colors.Green.Medium);
                                                    if (stat.Nuovi < maxVisite) r.RelativeItem(1 - (stat.Nuovi / maxVisite));
                                                });
                                            }
                                        });

                                        // Valori numerici a destra
                                        row.ConstantItem(70).AlignRight().Column(cVal => {
                                            cVal.Item().Text($"{stat.Totali} Tot.").FontSize(8).Bold().FontColor(Colors.Blue.Medium);
                                            if (stat.Nuovi > 0)
                                                cVal.Item().Text($"{stat.Nuovi} Nuovi").FontSize(8).Bold().FontColor(Colors.Green.Medium);
                                        });
                                    });
                                }

                                // --- LEGENDA (Aggiunta in fondo al box grigio) ---
                                colStat.Item().PaddingTop(15).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Row(legenda =>
                                {
                                    legenda.RelativeItem().Row(l1 => {
                                        l1.ConstantItem(10).Height(10).Width(10).Background(Colors.Blue.Medium);
                                        l1.ConstantItem(5);
                                        l1.RelativeItem().Text("Visite Totali").FontSize(8);
                                    });
                                    legenda.RelativeItem().Row(l2 => {
                                        l2.ConstantItem(10).Height(10).Width(10).Background(Colors.Green.Medium);
                                        l2.ConstantItem(5);
                                        l2.RelativeItem().Text("Nuovi Clienti (Prospect)").FontSize(8);
                                    });
                                });
                            });
                        }

                        c.Item().PaddingTop(10).AlignCenter().Text("Analisi della capillarità operativa e dello sviluppo commerciale")
                                .FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                    });
                });








                column.Item().PaddingTop(20).Background(Colors.Grey.Lighten4).Padding(20).Column(c =>
                {
                    c.Item().AlignCenter().Text("PIPELINE DI SVILUPPO").FontSize(10).Bold().FontColor(Colors.Blue.Medium);
                    c.Item().PaddingTop(15).Row(r =>
                    {
                        r.RelativeItem(1).Height(8).Background(Colors.Blue.Lighten4);
                        r.RelativeItem(1).Height(8).Background(Colors.Blue.Lighten2);
                        r.RelativeItem(1).Height(8).Background(Colors.Blue.Medium);
                        r.RelativeItem(1).Height(8).Background(Colors.Blue.Darken2);
                        r.RelativeItem(1).Height(8).Background(Colors.Green.Medium);
                    });
                    c.Item().PaddingTop(15).AlignCenter().Text("Analisi del Territorio, Potenziali di Vendita e Opportunità Future").FontSize(11).Italic().FontColor(Colors.Grey.Darken2);
                });

                column.Item().PageBreak();


                // --- PAGINA 2+: DETTAGLIO ---
                var gruppiProvincia = _visite.GroupBy(x => x.Prov_Legale ?? "N.D.").OrderBy(x => x.Key);

                foreach (var gruppo in gruppiProvincia)
                {
                    column.Item().PaddingTop(15).Background(Colors.Blue.Medium).Padding(5).PaddingLeft(10).Text(gruppo.Key).FontColor(Colors.White).Bold();

                    column.Item().PaddingTop(10).Column(listCol =>
                    {
                        listCol.Spacing(12);

                        foreach (var v in gruppo)
                        {
                            listCol.Item().ShowEntire().PaddingBottom(5)
                                .Background(v.IsNuovo ? "#F0FFF4" : Colors.Grey.Lighten4)
                                .BorderLeft(6)
                                .BorderColor(v.IsNuovo ? Colors.Green.Medium : Colors.Blue.Medium)
                                .BorderTop(0.5f).BorderRight(0.5f).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(12)
                                .Column(col =>
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem(3).Column(c => {
                                            c.Item().Row(r => {
                                                r.AutoItem().Text("CLIENTE").FontSize(7).Bold().FontColor(v.IsNuovo ? Colors.Green.Darken3 : Colors.Grey.Medium);
                                                if (v.IsNuovo)
                                                {
                                                    r.ConstantItem(5);
                                                    r.AutoItem().Background(Colors.Green.Medium).PaddingHorizontal(5).Text("NUOVA ANAGRAFICA").FontSize(7).Bold().FontColor(Colors.White);
                                                }
                                            });
                                            c.Item().Text(v.RagSociale ?? "N/D").FontSize(11).Bold();
                                        });

                                        row.RelativeItem(2).Column(c => {
                                            c.Item().Text("MOTIVO").FontSize(7).Bold();
                                            c.Item().Text(v.Motivazione ?? "-").FontSize(9);
                                        });

                                        row.RelativeItem(2).Column(c => {
                                            c.Item().Text("REFERENTE").FontSize(7).Bold();
                                            c.Item().Text(v.ReferenteIncontrato ?? "-").FontSize(9);
                                        });
                                    });

                                    col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem(5).Column(c => {
                                            c.Item().Text("NOTE ESITO").FontSize(7).Bold();
                                            c.Item().Text(v.NoteEsito ?? "Nessuna nota.").FontSize(9).Italic();
                                        });

                                        // --- INDICATORE TEMPERATURA AL POSTO DEL SEGUITO ---
                                        row.RelativeItem(2).AlignRight().Column(c => {
                                            c.Item().Text("POTENZIALE").FontSize(7).Bold();
                                            c.Item().Row(r => {
                                                for (int i = 1; i <= 5; i++)
                                                {
                                                    string icon = (i <= v.TemperaturaCliente) ? "●" : "○";
                                                    string color = v.TemperaturaCliente >= 4 ? Colors.Red.Medium : (v.TemperaturaCliente <= 2 ? Colors.Blue.Medium : Colors.Orange.Medium);
                                                    r.AutoItem().PaddingLeft(1).Text(icon).FontSize(12).FontColor(i <= v.TemperaturaCliente ? color : Colors.Grey.Lighten2);
                                                }
                                            });
                                            string label = v.TemperaturaCliente >= 4 ? "CALDO" : (v.TemperaturaCliente <= 2 ? "FREDDO" : "TIEPIDO");
                                            c.Item().Text(label).FontSize(7).Bold().FontColor(v.TemperaturaCliente >= 4 ? Colors.Red.Medium : Colors.Grey.Medium);
                                        });
                                    });
                                });
                        }
                    });
                }
            });
        }

        void CreateKpiCard(QuestPDF.Infrastructure.IContainer container, string title, string value, string color)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(col =>
            {
                col.Item().AlignCenter().Text(title).FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                col.Item().AlignCenter().Text(value).FontSize(26).Bold().FontColor(color);
            });
        }
    }
}
