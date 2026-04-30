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
        // 1. LOGICA DI FILTRAGGIO
        bool isStampaAccessori = Filtro == "ACCESSORI";
        bool isStampaProfili = Filtro == "PROFILI";
        bool mostraEconomica = string.Equals(Ordine.StatoOrdine, "Preventivo", StringComparison.OrdinalIgnoreCase)
                               || isStampaAccessori
                               || isStampaProfili;

        decimal totaleEconomicoStampa = 0;
        decimal pesoTotaleStampa = 0;

        container.Column(col =>
        {
            col.Item().Table(table =>
            {
                // 2. DEFINIZIONE COLONNE DINAMICHE
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25); // Pos.
                    columns.RelativeColumn(3);  // ARTICOLO

                    if (!isStampaAccessori)
                    {
                        columns.ConstantColumn(50); // COL. INT.
                        columns.ConstantColumn(50); // COL. EST.
                    }

                    columns.ConstantColumn(40); // Q.TÀ
                    columns.ConstantColumn(30); // UM

                    if (mostraEconomica)
                    {
                        columns.ConstantColumn(60); // PREZZO UNIT
                        columns.ConstantColumn(30); // SC%
                        columns.ConstantColumn(70); // TOTALE RIGA
                    }
                    else
                    {
                        columns.ConstantColumn(35); // SC%
                    }

                    if (!isStampaProfili)
                    {
                        columns.RelativeColumn(1); // FINITURA ACC.
                    }
                });

                // 3. HEADER
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).AlignCenter().Text("Pos.").FontSize(7).SemiBold();
                    header.Cell().Element(CellStyle).Text("CODICE PRODOTTO / DESCRIZIONE").FontSize(7).SemiBold();
                    if (!isStampaAccessori)
                    {
                        header.Cell().Element(CellStyle).AlignCenter().Text("COL.INT.").FontSize(7).SemiBold();
                        header.Cell().Element(CellStyle).AlignCenter().Text("COL.EST.").FontSize(7).SemiBold();
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
                    if (!isStampaProfili) { header.Cell().Element(CellStyle).Text("FINITURA ACC.").FontSize(7).SemiBold(); }
                });

                // 4. CICLO FISSO DI 17 RIGHE (Per layout costante)
                for (int i = 1; i <= 17; i++)
                {
                    var r = Ordine.Righe.ElementAtOrDefault(i - 1);
                    decimal valoreRigaStampa = 0;
                    decimal pesoRigaStampa = 0;

                    if (r != null)
                    {
                        // Calcolo matematico riga
                        if (!r.IsAccessorio)
                        {
                            pesoRigaStampa = r.PesoAlMetro * r.LunghezzaVerga * r.Quantita;
                            valoreRigaStampa = pesoRigaStampa * r.PrezzoUnitario * (1 - (r.ScontoRiga / 100));
                        }
                        else
                        {
                            valoreRigaStampa = r.Quantita * r.PrezzoUnitario * (1 - (r.ScontoRiga / 100));
                        }
                        totaleEconomicoStampa += valoreRigaStampa;
                        pesoTotaleStampa += pesoRigaStampa;
                    }

                    // CELLE
                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(i.ToString()).FontSize(8);

                    table.Cell().Element(CellStyle).MinHeight(30).Text(txt =>
                    {
                        if (r != null)
                        {
                            txt.Span(r.CodiceProdotto).SemiBold().FontSize(8).FontColor(Colors.Blue.Medium);
                            txt.EmptyLine();
                            string desc = r.Descrizione + (r.UmRiga == "BR" && r.LunghezzaVerga > 0 ? $" (L: {r.LunghezzaVerga:N2} mt)" : "");
                            txt.Span(desc).FontSize(7);

                            if (!r.IsAccessorio && mostraEconomica)
                            {
                                decimal mlTot = r.Quantita * r.LunghezzaVerga;
                                txt.EmptyLine();
                                txt.Span($"{r.Quantita:N0} pz x {r.LunghezzaVerga:N2} mt = {mlTot:N2} ml | Peso: {pesoRigaStampa:N2} Kg").FontSize(6).Italic().FontColor(Colors.Grey.Medium);
                            }
                        }
                    });

                    if (!isStampaAccessori)
                    {
                        table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r?.ColoreInt ?? "").FontSize(8);
                        table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r?.ColoreEst ?? "").FontSize(8);
                    }

                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r != null ? r.Quantita.ToString("N0") : "").FontSize(8);
                    table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r != null ? (r.IsConfezione ? r.UmSecondaria : r.UmRiga) : "").FontSize(7);

                    if (mostraEconomica)
                    {
                        table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r != null ? $"{r.PrezzoUnitario:C2} {(r.IsAccessorio ? "/pz" : "/Kg")}" : "").FontSize(7);
                        table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r != null && r.ScontoRiga > 0 ? $"{(int)r.ScontoRiga}%" : "").FontSize(7);
                        table.Cell().Element(CellStyle).MinHeight(30).AlignRight().Text(r != null ? valoreRigaStampa.ToString("C2") : "").FontSize(7).SemiBold();
                    }
                    else
                    {
                        table.Cell().Element(CellStyle).MinHeight(30).AlignCenter().Text(r != null && r.ScontoRiga > 0 ? $"{(int)r.ScontoRiga}%" : "").FontSize(8);
                    }

                    if (!isStampaProfili)
                    {
                        table.Cell().Element(CellStyle).MinHeight(30).Text(r?.IsAccessorio == true ? r.FinituraAccessorio : "").FontSize(7);
                    }
                }
            });

            // 5. BLOCCO FINALE (Condizioni e Note ripristinate)
            col.Item().PaddingTop(10).Row(row =>
            {
                // Sinistra: Dati Tecnici e Condizioni
                row.RelativeItem(2).Border(0.25f).Padding(5).Column(c => {
                    c.Item().Text("CONDIZIONI DI VENDITA").FontSize(8).SemiBold();
                    c.Item().Text($"Peso Totale Merce: {pesoTotaleStampa:N2} Kg").FontSize(8);

                    c.Item().PaddingTop(5).Text(txt => {
                        txt.Span("Concordate con: ").FontSize(8);
                        txt.Span(string.IsNullOrEmpty(Ordine.ConcordatoCon) ? "____________________" : Ordine.ConcordatoCon).FontSize(8).SemiBold();
                    });
                });

                // Destra: Note o Totale Economico
                if (mostraEconomica)
                {
                    row.RelativeItem(3).PaddingLeft(10).AlignRight().Column(c => {
                        c.Item().Table(t => {
                            t.ColumnsDefinition(cd => { cd.ConstantColumn(120); cd.ConstantColumn(90); });
                            t.Cell().Text("TOTALE IMPONIBILE:").FontSize(10).SemiBold();
                            t.Cell().AlignRight().BorderBottom(1).Text(totaleEconomicoStampa.ToString("C2")).FontSize(11).Bold();
                        });
                        c.Item().PaddingTop(2).Text("Iva Esclusa").FontSize(7).Italic().FontColor(Colors.Grey.Medium);
                    });
                }
                else
                {
                    row.RelativeItem(3).PaddingLeft(10).Border(0.25f).Padding(5).Column(c => {
                        c.Item().Text("NOTE ORDINE").FontSize(8).SemiBold();
                        c.Item().PaddingTop(2).Text(Ordine.NoteOrdine ?? "").FontSize(7);
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
