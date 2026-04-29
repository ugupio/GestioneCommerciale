using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestioneCommerciale.Models;

public class DocumentoOrdine : IDocument
{
    public Ordine Ordine { get; }
    public Cliente Cliente { get; }
    public string Filtro { get; }

    public DocumentoOrdine(Ordine ordine, Cliente cliente, string filtro = "TUTTO")
    {
        Ordine = ordine;
        Cliente = cliente;
        Filtro = filtro;
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
        // 1. LOGICA DI CONTROLLO
        bool isStampaAccessori = Filtro == "ACCESSORI";
        bool isStampaProfili = Filtro == "PROFILI";
        bool mostraEconomica = string.Equals(Ordine.StatoOrdine, "Preventivo", StringComparison.OrdinalIgnoreCase)
                               || isStampaAccessori
                               || isStampaProfili;

        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                // 2. COLONNE
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25); // Pos.
                    columns.RelativeColumn(3);  // ARTICOLO
                    if (!isStampaAccessori) { columns.ConstantColumn(45); columns.ConstantColumn(45); }
                    columns.ConstantColumn(35); // Q.TÀ
                    columns.ConstantColumn(25); // UM
                    if (mostraEconomica) { columns.ConstantColumn(60); columns.ConstantColumn(30); columns.ConstantColumn(70); }
                    else { columns.ConstantColumn(30); }
                    if (!isStampaProfili) { columns.RelativeColumn(1); }
                });

                // 3. HEADER
                table.Header(header => {
                    header.Cell().Element(CellStyle).AlignCenter().Text("Pos.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).Text("ARTICOLO").FontSize(7).SemiBold();
                    if (!isStampaAccessori)
                    {
                        header.Cell().Element(CellStyle).AlignCenter().Text("INT.").FontSize(7).SemiBold();
                        header.Cell().Element(CellStyle).AlignCenter().Text("EST.").FontSize(7).SemiBold();
                    }
                    header.Cell().Element(CellStyle).AlignCenter().Text("Q.TÀ").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("UM").FontSize(7).SemiBold();
                    if (mostraEconomica)
                    {
                        header.Cell().Element(CellStyle).AlignCenter().Text("PREZZO").FontSize(7).SemiBold();
                        header.Cell().Element(CellStyle).AlignCenter().Text("SC%").FontSize(7).SemiBold();
                        header.Cell().Element(CellStyle).AlignCenter().Text("TOTALE").FontSize(7).SemiBold();
                    }
                    else
                    {
                        header.Cell().Element(CellStyle).AlignCenter().Text("SC%").FontSize(7).SemiBold();
                    }
                    if (!isStampaProfili) { header.Cell().Element(CellStyle).Text("FINITURA").FontSize(7).SemiBold(); }
                });

                // 4. CICLO RIGHE CON CALCOLO MATEMATICO ESPLICITO
                decimal totaleDocumentoFiltrato = 0;

                foreach (var r in Ordine.Righe)
                {
                    // --- CALCOLO MATEMATICO SECONDO TUA FORMULA ---
                    decimal totaleRigaCalcolato = 0;
                    if (!r.IsAccessorio)
                    {
                        // Peso/ml * Lunghezza * Barre = KG RIGA
                        decimal kgRiga = r.PesoAlMetro * r.LunghezzaVerga * r.Quantita;
                        // KG RIGA * Prezzo * Sconto
                        totaleRigaCalcolato = kgRiga * r.PrezzoUnitario * (1 - (r.ScontoRiga / 100));
                    }
                    else
                    {
                        // Quantità * Prezzo * Sconto
                        totaleRigaCalcolato = r.Quantita * r.PrezzoUnitario * (1 - (r.ScontoRiga / 100));
                    }

                    totaleDocumentoFiltrato += totaleRigaCalcolato;

                    // --- STAMPA RIGA ---
                    table.Cell().Element(CellStyle).AlignCenter().Text(r.IDRigaOrdine.ToString()).FontSize(7);
                    table.Cell().Element(CellStyle).Text(txt => {
                        txt.Span($"{r.CodiceProdotto} - ").SemiBold().FontSize(8).FontColor(Colors.Blue.Medium);
                        txt.Span(r.Descrizione).FontSize(7);
                        if (!r.IsAccessorio && mostraEconomica)
                        {
                            decimal metriTot = r.Quantita * r.LunghezzaVerga;
                            decimal pesoTot = metriTot * r.PesoAlMetro;
                            txt.EmptyLine();
                            txt.Span($"{r.Quantita:N0} pz x {r.LunghezzaVerga:N2} mt = {metriTot:N2} ml | Peso Tot: {pesoTot:N2} Kg").FontSize(6).Italic().FontColor(Colors.Grey.Medium);
                        }
                    });

                    if (!isStampaAccessori)
                    {
                        table.Cell().Element(CellStyle).AlignCenter().Text(r.ColoreInt).FontSize(7);
                        table.Cell().Element(CellStyle).AlignCenter().Text(r.ColoreEst).FontSize(7);
                    }

                    table.Cell().Element(CellStyle).AlignCenter().Text(r.Quantita.ToString("N0")).FontSize(7);
                    table.Cell().Element(CellStyle).AlignCenter().Text(r.IsConfezione ? r.UmSecondaria : r.UmRiga).FontSize(7);

                    if (mostraEconomica)
                    {
                        table.Cell().Element(CellStyle).AlignCenter().Text($"{r.PrezzoUnitario:C2} {(r.IsAccessorio ? "/pz" : "/Kg")}").FontSize(7);
                        table.Cell().Element(CellStyle).AlignCenter().Text(r.ScontoRiga > 0 ? $"{(int)r.ScontoRiga}%" : "-").FontSize(7);
                        table.Cell().Element(CellStyle).AlignRight().Text(totaleRigaCalcolato.ToString("C2")).FontSize(7).SemiBold();
                    }
                    else
                    {
                        table.Cell().Element(CellStyle).AlignCenter().Text(r.ScontoRiga > 0 ? $"{(int)r.ScontoRiga}%" : "").FontSize(7);
                    }

                    if (!isStampaProfili)
                    {
                        table.Cell().Element(CellStyle).Text(r.IsAccessorio ? r.FinituraAccessorio : "").FontSize(7);
                    }
                }

                // 5. FOOTER CON IL NUOVO TOTALE CALCOLATO
                if (mostraEconomica)
                {
                    col.Item().AlignRight().PaddingTop(10).Table(t => {
                        t.ColumnsDefinition(c => { c.ConstantColumn(120); c.ConstantColumn(90); });
                        t.Cell().Text("TOTALE IMPONIBILE:").FontSize(10).SemiBold();
                        t.Cell().AlignRight().BorderTop(1).PaddingTop(2).Text(totaleDocumentoFiltrato.ToString("C2")).FontSize(11).Bold();
                        t.Cell().ColumnSpan(2).AlignRight().Text("Iva Esclusa").FontSize(7).Italic();
                    });
                }
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
