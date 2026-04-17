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
                    c.Item().Text("Sede Legale: Via E. De Amicis, Milano").FontSize(7).FontColor(Colors.Grey.Medium);
                    c.Item().Text("Sede Operativa: Via Colano, 9 - Genova Bolzaneto").FontSize(7).FontColor(Colors.Grey.Medium);
                    c.Item().Text("Tel. +39 010 7491941 - Email: info@alqgenovasrl.com").FontSize(7).FontColor(Colors.Grey.Medium);
                    c.Item().Text("CF/P.IVA e N° Reg.Imprese MI.12574490962 - REA/CCIAA MI26700419").FontSize(7).FontColor(Colors.Grey.Medium);
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
                    columns.RelativeColumn(3);  // ARTICOLO (Codice + Desc)
                    columns.ConstantColumn(50); // COL. INT.
                    columns.ConstantColumn(50); // COL. EST.
                    columns.ConstantColumn(40); // Q.TÀ
                    columns.ConstantColumn(30); // UM (Reale)
                    columns.ConstantColumn(30); // Sc%
                    columns.RelativeColumn(1);  // FINITURA ACC.
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).AlignCenter().Text("Pos.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).Text("CODICE PRODOTTO / DESCRIZIONE").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("COL.INT.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("COL.EST.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Q.TÀ").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("UM").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Sc%").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).Text("FINITURA ACC.").FontSize(7).SemiBold();
                });

                // Ciclo fisso di 17 righe per mantenere il layout costante
                for (int i = 1; i <= 17; i++)
                {
                    var r = Ordine.Righe.ElementAtOrDefault(i - 1);

                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(i.ToString()).FontSize(8);

                    // CELLA ARTICOLO: Codice + Descrizione (+ Lunghezza Verga se Profilo)
                    table.Cell().Element(CellStyle).MinHeight(30).Text(txt =>
                    {
                        if (r != null)
                        {
                            txt.Span(r.CodiceProdotto).SemiBold().FontSize(8).FontColor(Colors.Blue.Medium);
                            txt.EmptyLine();

                            string descrizioneInfo = r.Descrizione;
                            // Se è una barra, aggiungiamo l'info della lunghezza per chiarezza magazzino
                            if (r.UmRiga == "BR" && r.LunghezzaVerga > 0)
                            {
                                descrizioneInfo += $" (Verga L: {r.LunghezzaVerga:N2} mt)";
                            }

                            txt.Span(descrizioneInfo).FontSize(7);
                        }
                    });

                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r?.ColoreInt ?? "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r?.ColoreEst ?? "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r != null ? r.Quantita.ToString() : "").FontSize(8);

                    // UM REALE: Ora legge UmRiga (BR, PZ, MT, KG) anziché calcolarla
                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r?.UmRiga ?? "").FontSize(7);

                    // Sconto Intero
                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r != null && r.ScontoRiga > 0 ? $"{(int)Math.Round(r.ScontoRiga)}%" : "").FontSize(8);

                    table.Cell().Element(CellStyle).MinHeight(30).Text(r?.IsAccessorio == true ? r.FinituraAccessorio : "").FontSize(7);
                }
            });

            // Blocco finale (Condizioni e Note)
            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem(2).Border(0.25f).Padding(5).Column(c => {
                    c.Item().Text("CONDIZIONI DI VENDITA").FontSize(8).SemiBold();

                    // Peso Totale Ordine 
                    decimal pesoTotale = Ordine.Righe.Sum(x => x.Quantita * x.PesoAlMetro * (x.LunghezzaVerga > 0 ? x.LunghezzaVerga : 1));
                    c.Item().Text($"Peso Totale: {pesoTotale:N2} Kg").FontSize(8);

                    c.Item().PaddingTop(10).Text(txt => {
                        txt.Span("Concordate con: ").FontSize(8);
                        txt.Span(string.IsNullOrEmpty(Ordine.ConcordatoCon) ? "____________________" : Ordine.ConcordatoCon).FontSize(8).SemiBold();
                    });
                });

                row.RelativeItem(3).PaddingLeft(10).Border(0.25f).Padding(5).Column(c => {
                    c.Item().Text("NOTE ORDINE").FontSize(8).SemiBold();
                    c.Item().PaddingTop(2).Text(Ordine.NoteOrdine ?? "").FontSize(7);
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
            col.Item().AlignCenter().Text("P.IVA 01234567890 - Tel: 010 7491941").FontSize(7);
            col.Item().AlignRight().Text(x => {
                x.Span("Pagina ");
                x.CurrentPageNumber();
            });
        });
    }

    static IContainer CellStyle(IContainer container) =>
        container.Border(0.25f).BorderColor(Colors.Grey.Medium).PaddingVertical(2).PaddingHorizontal(4).AlignMiddle();
}
