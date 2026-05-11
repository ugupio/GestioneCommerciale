using GestioneCommerciale.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

public class DocumentoReportClienti : IDocument
{
    public List<Cliente> Lista { get; }
    public string Raggruppamento { get; }

    public DocumentoReportClienti(List<Cliente> lista, string raggruppamento)
    {
        Lista = lista;
        Raggruppamento = raggruppamento;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(1, Unit.Centimetre);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text(x => {
                x.Span("Pagina ");
                x.CurrentPageNumber();
            });
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row => {
            row.RelativeItem().Column(col => {
                col.Item().Text("REPORT ANAGRAFICA CLIENTI").FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                col.Item().Text($"{Lista.Count} Record - Formato: {Raggruppamento}").FontSize(9).Italic();
            });
            row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(9);
        });
    }

    // IL VIGILE: Decide quale layout disegnare
    void ComposeContent(IContainer container)
    {
        if (Raggruppamento == "SCHEDA")
        {
            ComposeSchedeVisita(container);
        }
        else
        {
            ComposeTabellaCompatta(container);
        }
    }

    // LAYOUT 1: Tabella Compatta
    void ComposeTabellaCompatta(IContainer container)
    {
        container.PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(3);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().BorderBottom(1).PaddingVertical(5).Text("Stato").SemiBold();
                header.Cell().BorderBottom(1).PaddingVertical(5).Text("Ragione Sociale").SemiBold();
                header.Cell().BorderBottom(1).PaddingVertical(5).Text("Località").SemiBold();
                header.Cell().BorderBottom(1).PaddingVertical(5).Text("Contatti").SemiBold();
                header.Cell().BorderBottom(1).PaddingVertical(5).AlignRight().Text("Ult.Ord.").SemiBold();
                header.Cell().BorderBottom(1).PaddingVertical(5).AlignRight().Text("Prev.").SemiBold();
            });

            foreach (var c in Lista)
            {
                var coloreSfondo = c.IsNuovo ? Colors.Yellow.Lighten4 : Colors.Transparent;

                table.Cell().Background(coloreSfondo).BorderBottom(0.1f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(2).AlignCenter().Text(c.IsNuovo ? "NEW" : "").FontSize(8).Bold();
                table.Cell().Background(coloreSfondo).BorderBottom(0.1f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(2).Text(c.RagSociale ?? "").FontSize(8).SemiBold();
                table.Cell().Background(coloreSfondo).BorderBottom(0.1f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(2).Text($"{c.Citta_Legale} ({c.Prov_Legale})").FontSize(8);
                table.Cell().Background(coloreSfondo).BorderBottom(0.1f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(2).Text($"{c.Telefono} {c.Cel1}").FontSize(8);

                var alertColore = (c.GiorniDallUltimoOrdine > 90) ? Colors.Red.Medium : Colors.Black;
                table.Cell().Background(coloreSfondo).BorderBottom(0.1f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(2).AlignRight().Text($"{c.GiorniDallUltimoOrdine ?? 0} gg").FontSize(8).FontColor(alertColore);

                table.Cell().Background(coloreSfondo).BorderBottom(0.1f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(2).AlignRight().Text(c.PrevisioneAnnuo?.ToString("C0") ?? "-").FontSize(8);
            }
        });
    }

    // LAYOUT 2: Scheda Visita Dettagliata
    void ComposeSchedeVisita(IContainer container)
    {
        container.PaddingTop(10).Column(col =>
        {
            foreach (var c in Lista)
            {
                col.Item().PaddingBottom(15).Border(0.5f).BorderColor(Colors.Grey.Medium).Column(innerCol =>
                {
                    innerCol.Item().Background(Colors.Grey.Lighten4).Padding(5).Row(row => {
                        row.RelativeItem().Text($"{c.RagSociale}").FontSize(11).Bold();
                        row.RelativeItem().AlignRight().Text($"{c.Citta_Legale} ({c.Prov_Legale})").FontSize(10);
                    });

                    innerCol.Item().Padding(5).Row(row => {
                        row.RelativeItem().Column(cDetails => {
                            cDetails.Item().Text($"Referente: {c.Referente}").FontSize(9);
                            cDetails.Item().Text($"Tel: {c.Telefono} - Cell: {c.Cel1}").FontSize(9);
                            cDetails.Item().Text($"Email: {c.Email}").FontSize(9);
                        });
                        row.RelativeItem().Column(cComm => {
                            cComm.Item().Text($"Ultimo Ordine: {c.GiorniDallUltimoOrdine ?? 0} gg fa").FontSize(9).SemiBold();
                            cComm.Item().Text($"Previsione Annua: {c.PrevisioneAnnuo?.ToString("C0") ?? "-"}").FontSize(9);
                            cComm.Item().Text(c.IsNuovo ? "STATO: NUOVO CLIENTE" : "STATO: STORICO").FontSize(8).Italic();
                        });
                    });

                    innerCol.Item().Padding(5).Column(nCol => {
                        nCol.Item().Text("ANNOTAZIONI PRECEDENTI:").FontSize(8).SemiBold();
                        nCol.Item().Text(string.IsNullOrEmpty(c.Annotazioni) ? "Nessuna nota." : c.Annotazioni).FontSize(8).Italic();
                        nCol.Item().PaddingTop(5).Height(50).Border(0.25f).BorderColor(Colors.Grey.Lighten2).Background(Colors.White);
                        nCol.Item().AlignRight().Text("Firma Visita: __________________________").FontSize(7).FontColor(Colors.Grey.Lighten1);
                    });
                });
            }
        });
    }
}
