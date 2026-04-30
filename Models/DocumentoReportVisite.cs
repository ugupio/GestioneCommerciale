using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestioneCommerciale.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace GestioneCommerciale.Models
{
    public class DocumentoReportVisite : IDocument
    {
        public DateTime Data { get; }
        public List<Visita> Visite { get; }

        public DocumentoReportVisite(DateTime data, List<Visita> visite)
        {
            Data = data;
            Visite = visite;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        // --- 1. HEADER: Loghi e Titolo Centrale ---
        void ComposeHeader(IContainer container)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Percorso standard (quello che hai scritto tu)
            string pathAlq = Path.Combine(baseDir, "wwwroot", "images", "logo-alq.jpg");
            string pathTwin = Path.Combine(baseDir, "wwwroot", "images", "logo-twin.png");

            // Se non lo trova in bin (perché VS non ha ancora copiato), 
            // risaliamo alla cartella principale del progetto
            if (!File.Exists(pathAlq))
            {
                // Questo risale di 3 livelli (da bin/Debug/netX a cartella Progetto)
                string projectRoot = Directory.GetParent(baseDir).Parent.Parent.FullName;
                pathAlq = Path.Combine(projectRoot, "wwwroot", "images", "logo-alq.jpg");
                pathTwin = Path.Combine(projectRoot, "wwwroot", "images", "logo-twin.png");
            }


            container.Row(row =>
            {
                row.ConstantItem(80).Element(e => {
                    if (File.Exists(pathAlq)) e.Image(pathAlq);
                    else e.Placeholder(); // Se vedi ancora il placeholder, il file non è fisicamente lì
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignCenter().Text("REPORT VISITE GIORNALIERO").FontSize(20).ExtraBold().FontColor(Colors.Blue.Medium);
                    col.Item().AlignCenter().Text($"Data: {Data:dd MMMM yyyy}").FontSize(11).Italic();
                });

                row.ConstantItem(80).Element(e => {
                    if (File.Exists(pathTwin)) e.Image(pathTwin);
                    else e.Placeholder();
                });
            });
        }


        // --- 2. BODY: Visite con Sfondo Grigio, Angoli Tondi e Icone ---
        void ComposeContent(IContainer container)
{
    container.PaddingTop(20).Column(column =>
    {
        column.Spacing(12);

        foreach (var v in Visite)
        {
            // Design Flat con bordo laterale colorato per un tocco moderno
            column.Item()
                .PaddingBottom(5)
                .Background(Colors.Grey.Lighten2)
                .BorderLeft(4) // Aggiunge una barra colorata a sinistra per stile
                .BorderColor(Colors.Blue.Medium)
                .BorderTop(0.5f)
                .BorderRight(0.5f)
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Medium)
                .Padding(12)
                .Column(col =>
                {
                    // RIGA 1: CLIENTE | MOTIVO | REF
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Row(inner => {
                            inner.ConstantItem(18).Text("👤").FontSize(10);
                            inner.RelativeItem().Column(c => {
                                c.Item().Text("CLIENTE").FontSize(8).Bold();
                                c.Item().Text(v.RagSociale ?? "N/D").FontSize(9);
                            });
                        });

                        row.RelativeItem(2).Row(inner => {
                            inner.ConstantItem(18).Text("🎯").FontSize(10);
                            inner.RelativeItem().Column(c => {
                                c.Item().Text("MOTIVO").FontSize(8).Bold();
                                c.Item().Text(v.Motivazione ?? "-").FontSize(9);
                            });
                        });

                        row.RelativeItem(2).Row(inner => {
                            inner.ConstantItem(18).Text("🤝").FontSize(10);
                            inner.RelativeItem().Column(c => {
                                c.Item().Text("REF").FontSize(8).Bold();
                                c.Item().Text(v.ReferenteIncontrato ?? "-").FontSize(9);
                            });
                        });
                    });

                    col.Item().PaddingVertical(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);

                    // RIGA 2: NOTE ESITO | SEGUITO
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(5).Row(inner => {
                            inner.ConstantItem(18).Text("📝").FontSize(10);
                            inner.RelativeItem().Column(c => {
                                c.Item().Text("NOTE ESITO").FontSize(8).Bold();
                                c.Item().Text(v.NoteEsito ?? "Nessuna nota.").FontSize(9).Italic();
                            });
                        });
                        
                        row.RelativeItem(2).AlignRight().Column(c => {
                            c.Item().Text("SEGUITO").FontSize(8).Bold();
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



        // --- 3. FOOTER: Numerazione e Data Generazione ---
        void ComposeFooter(IContainer container)
        {
            container.PaddingTop(10).BorderTop(1).Row(row =>
            {
                row.RelativeItem().Text(x =>
                {
                    x.Span("Documento generato il ");
                    x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").Italic();
                });

                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.Span("Pagina ");
                    x.CurrentPageNumber();
                    x.Span(" di ");
                    x.TotalPages();
                });
            });
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }
}
