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

        public ReportRiunioneDocument(List<ReportVisitaRiunione> visite, DateTime inizio, DateTime fine)
        {
            _visite = visite ?? new List<ReportVisitaRiunione>();
            _inizio = inizio;
            _fine = fine;
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
                    row.RelativeItem().Element(c => CreateKpiCard(c, "NUOVI CLIENTI", $"+{nuovi}", Colors.Green.Medium));
                });

                column.Item().PaddingTop(50).Background(Colors.Grey.Lighten4).Padding(20).Column(c =>
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
                    c.Item().PaddingTop(15).AlignCenter().Text("Ogni nuova anagrafica e ogni cliente 'caldo' rappresentano le basi del fatturato futuro.").FontSize(11).Italic().FontColor(Colors.Grey.Darken2);
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
