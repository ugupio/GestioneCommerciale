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

                // --- 1. HEADER (Ripetuto su ogni pagina) ---
                page.Header().Column(col => {
                    col.Item().Row(row => {
                        row.ConstantItem(100).Image("wwwroot/images/logo-twin.png");
                        row.RelativeItem().AlignCenter().Column(c => {
                            c.Item().Text("MODELLO RESO MERCE").FontSize(18).ExtraBold();
                        });
                        row.ConstantItem(100).Image("wwwroot/images/logo-alq.jpg");
                    });

                    // Sezione Anagrafica (Ora nell'Header per coerenza multi-pagina)
                    col.Item().PaddingVertical(10).Row(row => {
                        row.RelativeItem().Column(c => {
                            c.Item().Text("CLIENTE").FontSize(7).Bold().FontColor("#666666");
                            c.Item().Text(Modello.RagSociale).FontSize(12).ExtraBold();
                            c.Item().Text(Modello.Indirizzo ?? "________________").FontSize(9);
                            c.Item().Text($"{Modello.CAP} {Modello.Citta} ({Modello.Provincia}) - P.IVA: {Modello.PIva}").FontSize(9);
                        });

                        row.ConstantItem(180).Background("#F9F9F9").Padding(8).Border(0.5f).BorderColor("#DDD").Column(c => {
                            c.Item().Text("RIF. VENDITA").FontSize(7).Bold().AlignCenter();
                            c.Item().PaddingTop(2).Text($"Doc. n°: {Modello.RifDocumentoVendita ?? "_______"}").FontSize(10).AlignCenter();
                            c.Item().Text($"del {Modello.DataDocumentoVendita?.ToShortDateString() ?? "__/__/____"}").FontSize(10).AlignCenter();
                            c.Item().PaddingTop(4).Text($"Agente: {Modello.Agente}").FontSize(8).AlignCenter();
                        });
                    });
                    col.Item().LineHorizontal(0.5f).LineColor("#CCC");
                });

                // --- 2. CONTENT (Tabella con Griglia Fine) ---
                page.Content().Column(col => {
                    col.Item().PaddingTop(10).Table(table => {
                        table.ColumnsDefinition(c => {
                            c.RelativeColumn(4); c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(2);
                        });

                        table.Header(h => {
                            h.Cell().Element(FineHeaderStyle).Text("Descrizione del bene");
                            h.Cell().Element(FineHeaderStyle).Text("Quantità");
                            h.Cell().Element(FineHeaderStyle).Text("Colore");
                            h.Cell().Element(FineHeaderStyle).Text("Stato del bene");
                        });

                        // Righe Dati
                        foreach (var r in Modello.Righe)
                        {
                            table.Cell().Element(FineCellStyle).Text(r.Descrizione);
                            table.Cell().Element(FineCellStyle).AlignCenter().Text(r.Quantita.ToString());
                            table.Cell().Element(FineCellStyle).AlignCenter().Text(r.Colore);
                            table.Cell().Element(FineCellStyle).Text(r.StatoBene);
                        }

                        // Riempimento (Portato a 21 totali)
                        for (int i = Modello.Righe.Count; i < 21; i++)
                        {
                            table.Cell().Element(FineCellStyle).Text(" ");
                            table.Cell().Element(FineCellStyle).Text(" ");
                            table.Cell().Element(FineCellStyle).Text(" ");
                            table.Cell().Element(FineCellStyle).Text(" ");
                        }
                    });

                    col.Item().PaddingTop(10).Text(t => {
                        t.Span("Motivo del reso: ").Bold().FontSize(9);
                        t.Span(Modello.Motivazione).FontSize(9);
                    });
                });

                // --- 3. FOOTER (Sezione Aziendale Impilata) ---
                page.Footer().Column(f => {
                    f.Item().Border(0.5f).BorderColor("#CCC").Padding(10).Column(c => {
                        c.Item().Text("DA COMPILARE A CURA ACCETTAZIONE ALQ GENOVA SRL").FontSize(8).Bold().Underline();

                        c.Item().PaddingTop(8).Row(row => {
                            // ASPETTO (Verticale)
                            row.RelativeItem().Column(ca => {
                                ca.Item().Text("ASPETTO DEL MATERIALE").FontSize(8).Bold();
                                ca.Spacing(2);
                                ca.Item().Text("○ Materiale arrivato incartato ed imballato in modo adeguato").FontSize(8);
                                ca.Item().Text("○ Materiale arrivato privo di imballo, in pessimo stato").FontSize(8);
                                ca.Item().Text("○ Materiale arrivato non completo (accessori)").FontSize(8);
                                ca.Item().Text("○ Altro: ___________________________").FontSize(8);
                            });

                            // DESTINAZIONE (Verticale)
                            row.RelativeItem().Column(cd => {
                                cd.Item().Text("DESTINAZIONE MATERIALE IN ALQ GENOVA SRL").FontSize(8).Bold();
                                cd.Spacing(2);
                                cd.Item().Text("○ Materiale messo a rottame").FontSize(8);
                                cd.Item().Text("○ Materiale messo a reso a fornitore").FontSize(8);
                                cd.Item().Text("○ Materiale in casella/scafale perchè buono e vendibile)").FontSize(8);
                                cd.Item().Text("○ Altro: ___________________________").FontSize(8);
                            });
                        });

                        c.Item().PaddingTop(15).Row(row => {
                            row.RelativeItem().AlignBottom().Text("Foto allegate: SI – NO").FontSize(8);
                            row.ConstantItem(200).Column(sig => {
                                sig.Item().Text("___________________________").FontSize(10).AlignCenter();
                                sig.Item().Text("Firma responsabile magazzino").FontSize(7).AlignCenter();
                            });
                        });
                    });

                    f.Item().AlignRight().PaddingTop(5).Text(x => {
                        x.Span("Pag. ").FontSize(7).FontColor("#999");
                        x.CurrentPageNumber().FontSize(7).FontColor("#999");
                    });
                });
            });
        }

        // Metodi di supporto per la griglia fine
        IContainer FineHeaderStyle(IContainer container) => container.Border(0.5f).BorderColor("#CCC").Background("#F5F5F5").Padding(5).DefaultTextStyle(x => x.SemiBold().FontSize(9));
        IContainer FineCellStyle(IContainer container) => container.Border(0.5f).BorderColor("#CCC").Padding(5).DefaultTextStyle(x => x.FontSize(9));








        // STILI
        static IContainer HeaderStyle(IContainer container) => container.Border(1).Background("#f0f0f0").Padding(5).AlignCenter();
        static IContainer CellStyle(IContainer container) => container.Border(1).Padding(5);
    }
}
