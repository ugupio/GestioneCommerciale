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

                row.RelativeItem().Border(0.25f).Padding(5).Column(c => {
                    c.Item().Text($"RAGIONE SOCIALE: {Cliente?.RagSociale}").FontSize(9).SemiBold();
                    c.Item().Text($"COD. CLIENTE: {Ordine.IDCliente}").FontSize(8);
                });
            });

            col.Item().PaddingTop(5).Table(table =>
            {
                // Portiamo le colonne a 4 per far stare tutto su una riga
                table.ColumnsDefinition(columns => {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Element(CellStyle).Text($"DATA: {Ordine.DataOrdine?.ToShortDateString()}").FontSize(8);
                table.Cell().Element(CellStyle).Text($"RIF: {Ordine.NumeroDocumento}").FontSize(8);
                table.Cell().Element(CellStyle).Text($"CONSEGNA: {Ordine.DataConsegnaPrevista.ToShortDateString()}").FontSize(8);

                // AGGIUNTA DELLO STATO (CAUSALE)
                table.Cell().Element(CellStyle).Text($"STATO: {Ordine.StatoOrdine?.ToUpper()}").FontSize(8).SemiBold();
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        // Usiamo una colonna che occupa tutto lo spazio verticale
        container.Column(col =>
        {
            // La tabella occupa il suo spazio naturale
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).AlignCenter().Text("Pos.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).Text("DESCRIZIONE").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("INT.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("EST.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Q.TÀ").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).Text("NOTE").FontSize(7).SemiBold();
                });

                // Generiamo sempre 14 righe per mantenere la struttura del modulo
                for (int i = 1; i <= 14; i++)
                {
                    var r = Ordine.Righe.ElementAtOrDefault(i - 1);

                    // Applichiamo .MinHeight(38) a ogni cella per "stirare" la tabella verso il basso
                    table.Cell().Element(CellStyle).MinHeight(38).AlignCenter().Text(i.ToString()).FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(38).Text(r?.Descrizione ?? "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(38).AlignCenter().Text(r?.ColoreInt ?? "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(38).AlignCenter().Text(r?.ColoreEst ?? "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(38).AlignCenter().Text(r != null ? r.Quantita.ToString() : "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(38).Text(r?.IsAccessorio == true ? r.FinituraAccessorio : "").FontSize(7);
                }

            });

            // Spingiamo il blocco Note e Condizioni in fondo alla pagina
            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem(2).Border(0.25f).Padding(5).Column(c => {
                    row.RelativeItem(2).Border(0.25f).Padding(5).Column(c => {
                        c.Item().Text("CONDIZIONI DI VENDITA").FontSize(8).SemiBold();
                        c.Item().PaddingTop(2).Text($"Totale Imponibile: {Ordine.TotaleImponibile:N2} €").FontSize(9).SemiBold();

                        // Mostra il nome se presente, altrimenti lascia i puntini
                        c.Item().PaddingTop(10).Text(txt => {
                            txt.Span("Concordate con: ").FontSize(8);
                            txt.Span(string.IsNullOrEmpty(Ordine.ConcordatoCon) ? "____________________" : Ordine.ConcordatoCon).FontSize(8).SemiBold();
                        });
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
