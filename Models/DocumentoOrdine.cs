using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestioneCommerciale.Models; // Assicurati che il namespace sia corretto

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

    // Qui costruiremo il logo e i dati Ragione Sociale
    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            // Parte sinistra: Logo/Nome Azienda
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("ALQ GENOVA").FontSize(20).ExtraBold().FontColor(Colors.Blue.Medium);
                col.Item().Text("SISTEMI PER SERRAMENTI IN ALLUMINIO").FontSize(8);
            });

            // Parte destra: Riquadro Ragione Sociale (Uguale al tuo modulo)
            row.RelativeItem().Border(0.5f).Padding(5).Column(col =>
            {
                // QUESTA È LA RIGA CHE CERCAVI:
                col.Item().Text(txt => {
                    txt.Span("Ragione Sociale: ").FontSize(8);
                    txt.Span(Cliente?.RagSociale ?? "________________").SemiBold().FontSize(10);
                });

                col.Item().PaddingTop(5).Text(txt => {
                    txt.Span("Codice Cliente: ").FontSize(8);
                    txt.Span(Ordine.IDCliente.ToString()).SemiBold().FontSize(10);
                });
            });
        });
    }

    // Qui costruiremo la griglia delle 14 righe
    void ComposeContent(IContainer container)
    {
        container.PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25); // Pos
                columns.RelativeColumn(3);  // Profili
                columns.ConstantColumn(40); // Col. Int
                columns.ConstantColumn(40); // Col. Est
                columns.ConstantColumn(35); // Verghe
                columns.RelativeColumn(2);  // Accessori
                columns.ConstantColumn(35); // Q.ta
                columns.RelativeColumn(2);  // Note
            });

            // Intestazione tabella (Semplificata per ora)
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Pos.");
                header.Cell().Element(CellStyle).Text("Profili");
                header.Cell().Element(CellStyle).Text("Int.");
                header.Cell().Element(CellStyle).Text("Est.");
                header.Cell().Element(CellStyle).Text("Vrg.");
                header.Cell().Element(CellStyle).Text("Accessori");
                header.Cell().Element(CellStyle).Text("Q.ta");
                header.Cell().Element(CellStyle).Text("Note");
            });

            // Ciclo per creare le 14 righe del tuo modulo
            for (int i = 1; i <= 14; i++)
            {
                var r = Ordine.Righe.ElementAtOrDefault(i - 1);
                table.Cell().Element(CellStyle).Text(i.ToString());
                table.Cell().Element(CellStyle).Text(!r?.IsAccessorio ?? false ? r.Descrizione : "");
                table.Cell().Element(CellStyle).Text(r?.ColoreInt ?? "");
                table.Cell().Element(CellStyle).Text(r?.ColoreEst ?? "");
                table.Cell().Element(CellStyle).Text(!r?.IsAccessorio ?? false ? r.Quantita.ToString() : "");
                table.Cell().Element(CellStyle).Text(r?.IsAccessorio ?? false ? r.Descrizione : "");
                table.Cell().Element(CellStyle).Text(r?.IsAccessorio ?? false ? r.Quantita.ToString() : "");
                table.Cell().Element(CellStyle).Text("");
            }
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text("ALQ GENOVA S.r.l. - Sede Operativa: Genova Bolzaneto").FontSize(7);
    }

    // Helper per lo stile delle celle (bordi neri come il tuo PDF)
    static IContainer CellStyle(IContainer container) =>
        container.Border(0.25f) // Linea molto sottile
             .BorderColor(Colors.Grey.Medium) // Grigio scuro invece di nero puro per un effetto meno "pesante"
             .PaddingVertical(2)
             .PaddingHorizontal(4)
             .AlignMiddle();
}
