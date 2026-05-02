using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestioneCommerciale.Models;

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
                // --- PAGINA 1: COPERTINA ACCATTIVANTE ---
                column.Item().PaddingTop(40).AlignCenter().Text("SINTESI DELLE ATTIVITÀ").FontSize(24).ExtraBold().FontColor(Colors.Blue.Medium);
                column.Item().AlignCenter().Text("Presidio del Territorio e Sviluppo Nuovi Lead").FontSize(12).FontColor(Colors.Grey.Medium);

                column.Item().PaddingVertical(30).LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                // Box KPI Grandi
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

                // Elemento Grafico "Stimolo Fantasia" (Progress Bar di Crescita)
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
                    //c.Item().PaddingTop(15).AlignCenter().Text("Stiamo seminando oggi per raccogliere domani. Ogni nuovo cliente è un investimento per il futuro.").FontSize(11).Italic().FontColor(Colors.Grey.Darken2);
                });

                // SALTO PAGINA
                column.Item().PageBreak();

                // --- PAGINA 2: DETTAGLIO VISITE RAGGRUPPATE ---
                column.Item().PaddingTop(10).Text("DETTAGLIO ATTIVITÀ PER PROVINCIA").FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                column.Item().PaddingBottom(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                var gruppiProvincia = _visite.GroupBy(x => x.Prov_Legale ?? "N.D.").OrderBy(x => x.Key);

                foreach (var gruppo in gruppiProvincia)
                {
                    column.Item().PaddingTop(15).Background(Colors.Blue.Medium).Padding(5).PaddingLeft(10).Text(gruppo.Key).FontColor(Colors.White).Bold();

                    column.Item().PaddingTop(10).Column(listCol =>
                    {
                        listCol.Spacing(12);

                        foreach (var v in gruppo)
                        {
                            listCol.Item()
                                .Background(Colors.Grey.Lighten4)
                                .BorderLeft(4)
                                .BorderColor(v.IsNuovo ? Colors.Green.Medium : Colors.Blue.Medium)
                                .BorderTop(0.5f).BorderRight(0.5f).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(12)
                                .Column(col =>
                                {
                                    // RIGA 1: CLIENTE | MOTIVO | REF
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem(3).Row(inner => {
                                            inner.ConstantItem(18).Text("👤");
                                            inner.RelativeItem().Column(c => {
                                                c.Item().Row(r => {
                                                    r.AutoItem().Text("CLIENTE").FontSize(7).Bold();
                                                    if (v.IsNuovo)
                                                    {
                                                        r.ConstantItem(5);
                                                        r.AutoItem().Background(Colors.Green.Lighten4).PaddingHorizontal(3).Text("NUOVO").FontSize(6).Bold().FontColor(Colors.Green.Darken3);
                                                    }
                                                });
                                                c.Item().Text(v.RagSociale ?? "N/D").FontSize(9).Bold();
                                            });
                                        });

                                        row.RelativeItem(2).Row(inner => {
                                            inner.ConstantItem(18).Text("🎯");
                                            inner.RelativeItem().Column(c => {
                                                c.Item().Text("MOTIVO").FontSize(7).Bold();
                                                c.Item().Text(v.Motivazione ?? "-").FontSize(9);
                                            });
                                        });

                                        row.RelativeItem(2).Row(inner => {
                                            inner.ConstantItem(18).Text("🤝");
                                            inner.RelativeItem().Column(c => {
                                                c.Item().Text("REF").FontSize(7).Bold();
                                                c.Item().Text(v.ReferenteIncontrato ?? "-").FontSize(9);
                                            });
                                        });
                                    });

                                    col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                                    // RIGA 2: NOTE ESITO | SEGUITO
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem(5).Row(inner => {
                                            inner.ConstantItem(18).Text("📝");
                                            inner.RelativeItem().Column(c => {
                                                c.Item().Row(r => {
                                                    r.AutoItem().Text("NOTE ESITO").FontSize(7).Bold();
                                                    if (v.TemperaturaCliente >= 4)
                                                    {
                                                        r.ConstantItem(8);
                                                        r.AutoItem().Text("🔥 INTERESSE ALTO").FontSize(7).Bold().FontColor(Colors.Red.Medium);
                                                    }
                                                });
                                                c.Item().Text(v.NoteEsito ?? "Nessuna nota.").FontSize(9).Italic();
                                            });
                                        });

                                        row.RelativeItem(2).AlignRight().Column(c => {
                                            c.Item().Text("SEGUITO").FontSize(7).Bold();
                                            c.Item().Text(v.RichiedeSeguito ? "SÌ" : "NO")
                                             .FontSize(10)
                                             .FontColor(v.RichiedeSeguito ? Colors.Red.Medium : Colors.Green.Darken3)
                                             .Bold();
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
            container.Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(col =>
            {
                col.Item().AlignCenter().Text(title).FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                col.Item().AlignCenter().Text(value).FontSize(24).Bold().FontColor(color);
            });
        }
    }
}
