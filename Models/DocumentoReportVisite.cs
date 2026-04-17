using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestioneCommerciale.Models
{
    public class DocumentoReportVisite : IDocument
    {
        public DateTime Data { get; }
        public List<dynamic> Visite { get; }

        public DocumentoReportVisite(DateTime data, List<dynamic> visite)
        {
            Data = data;
            Visite = visite;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text($"REPORT ATTIVITÀ GIORNALIERA - {Data:dd/MM/yyyy}")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Cliente
                        columns.ConstantColumn(80); // Stato
                        columns.ConstantColumn(80); // Temp.
                        columns.RelativeColumn(4); // Note
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).Text("Cliente").SemiBold();
                        header.Cell().BorderBottom(1).Text("Stato").SemiBold();
                        header.Cell().BorderBottom(1).Text("Clima").SemiBold();
                        header.Cell().BorderBottom(1).Text("Esito Incontro").SemiBold();
                    });

                    foreach (var v in Visite)
                    {
                        // Usiamo Convert per trasformare i valori in modo sicuro
                        string ragSociale = v.RagSociale?.ToString() ?? "";

                        // Gestiamo lo stato e la temperatura convertendo da stringa a numero
                        int stato = Convert.ToInt32(v.StatoVisita ?? 0);
                        int temp = Convert.ToInt32(v.TemperaturaCliente ?? 0);

                        string motivazione = v.Motivazione?.ToString() ?? "";
                        string noteEsito = v.NoteEsito?.ToString() ?? "";
                        bool seguito = Convert.ToBoolean(v.RichiedeSeguito ?? false);

                        // Ora usiamo le variabili pulite per il PDF
                        table.Cell().Element(CellStyle).Text(ragSociale);

                        string statoTesto = stato == 1 ? "Eseguita" : stato == 2 ? "Prog." : "Ann.";
                        table.Cell().Element(CellStyle).Text(statoTesto);

                        string tempTesto = temp == 1 ? "Freddo" : temp == 2 ? "Tiepido" : "Caldo";
                        table.Cell().Element(CellStyle).Text(tempTesto);

                        table.Cell().Element(CellStyle).Column(c => {
                            c.Item().Text(motivazione).FontSize(9).SemiBold();
                            c.Item().Text(noteEsito).FontSize(8);
                        });

                        table.Cell().Element(CellStyle).AlignCenter().Text(seguito ? "SI" : "no");

                        static IContainer CellStyle(IContainer container) =>
                            container.PaddingVertical(5).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
                    }



                });
            });
        }
    }
}
