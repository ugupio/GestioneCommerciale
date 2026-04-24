using GestioneCommerciale.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestioneCommerciale.Models
{
    public class ResoDocument : IDocument
    {
        public Reso Modello { get; }
        public ResoDocument(Reso modello) => Modello = modello;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                // --- 1. HEADER ---
                page.Header().Row(row => {
                    row.ConstantItem(100).Image("wwwroot/images/logo-twin.png");
                    row.RelativeItem().PaddingTop(10).AlignCenter().Column(c => {
                        c.Item().Text("MODELLO RESO MERCE").FontSize(20).Bold();
                    });
                    row.ConstantItem(120).Image("wwwroot/images/logo-alq.jpg");
                });

                // --- 2. CONTENT ---
                page.Content().PaddingVertical(10).Column(col => {
                    // Dati Cliente e Agente
                    col.Item().PaddingBottom(5).Row(row => {
                        row.RelativeItem().Text(t => { t.Span("CLIENTE: ").Bold(); t.Span(Modello.RagSociale); });
                        row.RelativeItem().Text(t => { t.Span("AGENTE: ").Bold(); t.Span(Modello.Agente); });
                    });

                    // --- NUOVA SEZIONE: RIFERIMENTI VENDITA (Sotto Cliente, Sopra Tabella) ---
                    col.Item().PaddingBottom(15).Row(row => {
                        row.RelativeItem().Text(t => {
                            t.Span("Rif. documento n°: ").Bold();
                            t.Span(Modello.RifDocumentoVendita ?? "___________").Underline();
                        });
                        row.RelativeItem().Text(t => {
                            t.Span("Data: ").Bold();
                            t.Span(Modello.DataDocumentoVendita?.ToShortDateString() ?? "__/__/____").Underline();
                        });
                    });

                    // 3. TABELLA ARTICOLI
                    col.Item().Table(table => {
                        table.ColumnsDefinition(c => {
                            c.RelativeColumn(4); c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(2);
                        });
                        table.Header(h => {
                            h.Cell().Element(HeaderStyle).Text("Descrizione del bene");
                            h.Cell().Element(HeaderStyle).Text("Quantità");
                            h.Cell().Element(HeaderStyle).Text("Colore");
                            h.Cell().Element(HeaderStyle).Text("Stato del bene");
                        });
                        foreach (var r in Modello.Righe)
                        {
                            table.Cell().Element(CellStyle).Text(r.Descrizione);
                            table.Cell().Element(CellStyle).AlignCenter().Text(r.Quantita.ToString());
                            table.Cell().Element(CellStyle).AlignCenter().Text(r.Colore);
                            table.Cell().Element(CellStyle).Text(r.StatoBene);
                        }
                    });

                    col.Item().PaddingTop(10).Text(t => { t.Span("Motivo del reso: ").Bold(); t.Span(Modello.Motivazione); });
                });

                // --- 4. FOOTER (Senza punti di errore) ---
                page.Footer().Column(f =>
                {
                    f.Item().Border(1).Padding(10).Column(c => {
                        c.Item().Text("DA COMPILARE A CURA ACCETTAZIONE MAGAZZINO").Bold().Underline();
                        c.Item().PaddingTop(5).Text("ASPETTO DEL MATERIALE:");
                        c.Item().Text("▢ Imballo adeguato  ▢ Privo imballo  ▢ Incompleto");
                        c.Item().PaddingTop(10).Text("DESTINAZIONE: ▢ Rottame  ▢ Reso Fornitore  ▢ Scaffale");
                        c.Item().PaddingTop(25).AlignRight().Column(sig => {
                            sig.Item().Text("______________________________________").FontSize(10);
                            sig.Item().AlignRight().PaddingRight(20).Text("Firma del responsabile magazzino").FontSize(8);
                        });
                    });

                    f.Item().AlignRight().Text(x => {
                        x.Span("Pag. ").FontSize(8).FontColor("#999");
                        x.CurrentPageNumber().FontSize(8).FontColor("#999");
                    });
                });
            });
        }




        // STILI
        static IContainer HeaderStyle(IContainer container) => container.Border(1).Background("#f0f0f0").Padding(5).AlignCenter();
        static IContainer CellStyle(IContainer container) => container.Border(1).Padding(5);
    }
}
