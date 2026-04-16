using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestioneCommerciale.Models;

public class DocumentoOrdine : IDocument
{
    public Ordine Ordine { get; }
    public Cliente Cliente { get; }

    public DocumentoOrdine(Ordine ordine, Cliente cliente)
    {
        Ordine = ordine;
        Cliente = cliente;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Verdana));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c => {
                    c.Item().Text("ALQ GENOVA").FontSize(20).ExtraBold().FontColor(Colors.Blue.Medium);
                });

                // BOX DESTINATARIO (Sempre visibile)
                row.RelativeItem().Border(0.25f).Padding(5).Column(c =>
                {
                    c.Item().Text("SPETT.LE").FontSize(7).FontColor(Colors.Grey.Medium);
                    c.Item().Text($"{Cliente?.RagSociale}").FontSize(10).SemiBold();

                    // Indirizzo
                    c.Item().Text($"{Cliente?.Ind_legale}").FontSize(9);

                    // CAP e Città
                    c.Item().Text($"{Cliente?.CAP_Legale} {Cliente?.Citta_Legale}").FontSize(9);

                    // Cellulare/Telefono (solo se presente)
                    if (!string.IsNullOrWhiteSpace(Cliente?.Cel1))
                    {
                        c.Item().Text($"Tel/Cell: {Cliente.Cel1}").FontSize(9);
                    }

                    // Spazio per separare e Riferimento Cliente (solo se presente)
                    if (!string.IsNullOrWhiteSpace(Ordine.RiferimentoCliente))
                    {
                        c.Item().PaddingTop(4);
                        c.Item().Text(text =>
                        {
                            text.Span("RIFERIMENTO: ").FontSize(8).SemiBold();
                            text.Span(Ordine.RiferimentoCliente).FontSize(8);
                        });
                    }
                });
            });

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns => {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Element(CellStyle).Text($"DATA: {Ordine.DataOrdine?.ToShortDateString()}").FontSize(8);
                table.Cell().Element(CellStyle).Text($"RIF: {Ordine.NumeroDocumento}").FontSize(8);
                table.Cell().Element(CellStyle).Text($"CONSEGNA: {Ordine.DataConsegnaPrevista.ToShortDateString()}").FontSize(8);
                table.Cell().Element(CellStyle).Text($"STATO: {Ordine.StatoOrdine?.ToUpper()}").FontSize(8).SemiBold();
            });
        });
    }



    void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25); // Pos.
                    columns.RelativeColumn(3);  // CODICE e DESCRIZIONE
                    columns.ConstantColumn(50); // INT.
                    columns.ConstantColumn(50); // EST.
                    columns.ConstantColumn(35); // Q.TÀ
                    columns.RelativeColumn(1);  // NOTE (Finitura)
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).AlignCenter().Text("Pos.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).Text("ARTICOLO / DESCRIZIONE").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("INT.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("EST.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Q.TÀ").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).Text("NOTE").FontSize(7).SemiBold();
                });

                for (int i = 1; i <= 14; i++)
                {
                    var r = Ordine.Righe.ElementAtOrDefault(i - 1);

                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(i.ToString()).FontSize(8);

                    // CELLA ARTICOLO: Codice + Descrizione
                    table.Cell().Element(CellStyle).MinHeight(30).Text(txt =>
                    {
                        if (r != null)
                        {
                            // Codice Prodotto in grassetto e blu per risaltare
                            txt.Span(r.CodiceProdotto).SemiBold().FontSize(8).FontColor(Colors.Blue.Medium);
                            txt.EmptyLine();
                            // Descrizione sotto
                            txt.Span(r.Descrizione).FontSize(8);
                        }
                    });

                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r?.ColoreInt ?? "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r?.ColoreEst ?? "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r != null ? r.Quantita.ToString() : "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(30).Text(r?.IsAccessorio == true ? r.FinituraAccessorio : "").FontSize(7);
                }
            });

            // Blocco finale (Condizioni e Note)
            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem(2).Border(0.25f).Padding(5).Column(c => {
                    c.Item().Text("CONDIZIONI DI VENDITA").FontSize(8).SemiBold();
                    c.Item().PaddingTop(2).Text($"Totale Imponibile: {Ordine.TotaleImponibile:N2} €").FontSize(9).SemiBold();

                    // AGGIUNTA: Peso Totale Ordine (Opzionale, se vuoi mostrarlo qui)
                    // c.Item().Text($"Peso Totale: {Ordine.Righe.Sum(x => x.Quantita * x.PesoAlMetro * (x.LunghezzaVerga > 0 ? x.LunghezzaVerga : 1)):N2} Kg").FontSize(8);

                    c.Item().PaddingTop(10).Text(txt => {
                        txt.Span("Concordate con: ").FontSize(8);
                        txt.Span(string.IsNullOrEmpty(Ordine.ConcordatoCon) ? "____________________" : Ordine.ConcordatoCon).FontSize(8).SemiBold();
                    });
                });

                row.RelativeItem(3).PaddingLeft(10).Border(0.25f).Padding(5).Column(c => {
                    c.Item().Text("NOTE").FontSize(8).SemiBold();
                    c.Item().PaddingTop(2).Text(" ").FontSize(7);
                });
            });
        });
    }


    void ComposeFooter(IContainer container)
    {
        container.PaddingTop(10).Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
            col.Item().AlignCenter().Text(t => {
                t.Span("ALQ GENOVA S.r.l. - Sede Legale: Via E. De Amicis, Milano | ").FontSize(7);
                t.Span("Sede Operativa: Genova | ").FontSize(7);
                t.Span("Sede Amm.: Firenze").FontSize(7);
            });
            col.Item().AlignCenter().Text("P.IVA 01234567890 - Tel: 010.XXXXXX").FontSize(7);
            col.Item().AlignRight().Text(x => {
                x.Span("Pagina ");
                x.CurrentPageNumber();
            });
        });
    }

    static IContainer CellStyle(IContainer container) =>
        container.Border(0.25f).BorderColor(Colors.Grey.Medium).PaddingVertical(2).PaddingHorizontal(4).AlignMiddle();
}
