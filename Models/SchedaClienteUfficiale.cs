using GestioneCommerciale.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

public class SchedaClienteUfficiale : IDocument
{
    public Cliente C { get; }
    private readonly string _pathLogoAlq;
    private readonly string _pathLogoTwin;

    public SchedaClienteUfficiale(Cliente cliente, string rootPath)
    {
        C = cliente;
        _pathLogoAlq = Path.Combine(rootPath, "logo-alq.jpg");
        _pathLogoTwin = Path.Combine(rootPath, "logo-twin.png");
    }

    public void Compose(IDocumentContainer container)
    {
        // PAGINA 1: ANAGRAFICA E DESTINAZIONE
        container.Page(page =>
        {
            page.Margin(1, Unit.Centimetre);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });

        // PAGINA 2: BANCA E LOGISTICA
        container.Page(page =>
        {
            page.Margin(1, Unit.Centimetre);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposePage2);
            page.Footer().Element(ComposeFooter);
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                if (File.Exists(_pathLogoTwin)) row.RelativeItem().Height(40).Image(_pathLogoTwin);
                else row.RelativeItem().Text("TWIN SYSTEMS").FontSize(10).Bold();

                if (File.Exists(_pathLogoAlq)) row.RelativeItem().AlignRight().Height(60).Image(_pathLogoAlq);
                else row.RelativeItem().AlignRight().Text("ALQ GENOVA").FontSize(14).Bold();
            });
            col.Item().PaddingTop(25).AlignCenter().Text("SCHEDA CLIENTE").FontSize(18).ExtraBold();
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingTop(30).Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem(2).Element(e => CampoAgenteSenzaLinea(e, "AGENTE:", "GUCCI"));
                row.RelativeItem().PaddingLeft(20).Column(c => {
                    c.Item().Text("☐ Show room").FontSize(8);
                    c.Item().Text("☐ Serramentista").FontSize(8);
                    c.Item().Text("☐ Falegnameria").FontSize(8);
                    c.Item().Text("☐ Imprese costruzioni").FontSize(8);
                });
            });

            col.Item().PaddingTop(40);
            col.Item().Element(e => CampoDato(e, "RAGIONE SOCIALE", C.RagSociale));
            col.Item().PaddingTop(15).Element(e => CampoDato(e, "REFERENTE", C.Referente));
            col.Item().PaddingTop(15).Element(e => CampoDato(e, "SEDE LEGALE", C.Ind_legale));

            col.Item().PaddingTop(15).Row(row => {
                row.ConstantItem(80).Element(e => CampoDato(e, "CAP", C.CAP_Legale));
                row.RelativeItem().PaddingLeft(15).Element(e => CampoDato(e, "CITTA'", C.Citta_Legale));
                row.ConstantItem(100).PaddingLeft(15).Element(e => CampoDato(e, "PROV.", C.Prov_Legale));
            });

            col.Item().PaddingTop(30);
            col.Item().Row(row => {
                row.RelativeItem().Element(e => CampoDato(e, "TEL", C.Telefono));
                row.RelativeItem().PaddingLeft(15).Element(e => CampoDato(e, "FAX", C.Fax));
                row.RelativeItem().PaddingLeft(15).Element(e => CampoDato(e, "E-MAIL", C.Email));
            });

            col.Item().PaddingTop(15).Row(row => {
                row.RelativeItem().Element(e => CampoDato(e, "CELLULARE 1", C.Cel1));
                row.RelativeItem().PaddingLeft(15).Element(e => CampoDato(e, "CELLULARE 2", C.Cel2));
            });

            col.Item().PaddingTop(15).Row(row => {
                row.RelativeItem().Element(e => CampoDato(e, "P.IVA", C.PIva));
                row.RelativeItem().PaddingLeft(15).Element(e => CampoDato(e, "COD.FISCALE", C.CodFiscale));
                row.RelativeItem().PaddingLeft(15).Element(e => CampoDato(e, "UNIVOCO SDI", C.SDI));
            });

            col.Item().PaddingTop(50).AlignCenter().Text("DESTINAZIONE MERCE").FontSize(14).Bold().Underline();
            col.Item().PaddingTop(20).Element(e => CampoDato(e, "CLIENTE", C.RagSociale));
            col.Item().PaddingTop(15).Element(e => CampoDato(e, "INDIRIZZO", ""));
            col.Item().PaddingTop(15).Row(row => {
                row.RelativeItem().Element(e => CampoDato(e, "CITTA'", ""));
                row.ConstantItem(100).PaddingLeft(15).Element(e => CampoDato(e, "PROV.", ""));
            });
        });
    }

    void ComposePage2(IContainer container)
    {
        container.PaddingTop(20).Column(col =>
        {
            // --- CONDIZIONI DI VENDITA / PAGAMENTO ---
            col.Item().Text("CONDIZIONI DI VENDITA / PAGAMENTO").FontSize(11).Bold().Underline();

            // Creiamo una variabile sicura: se PagProposto è null, diventa una stringa vuota
            string pag = C.PagProposto ?? "";

            col.Item().PaddingTop(10).Row(row => {
                // Ora il .Contains non darà più errore perché "pag" non è mai null
                row.RelativeItem().Text($"{(pag.Contains("AUTISTA") ? "☒" : "☐")} RIMESSA AUTISTA/AGENTE").FontSize(9);
                row.RelativeItem().Text($"{(pag.Contains("BANCARIE") ? "☒" : "☐")} RICEVUTE BANCARIE").FontSize(9);
            });

            // --- BANCA ---
            col.Item().PaddingTop(15).Element(e => CampoDato(e, "BANCA", C.Banca));

            col.Item().PaddingTop(10).Row(row => {
                row.RelativeItem(2).Element(e => CampoDato(e, "AGENZIA", C.Agenzia));
                row.RelativeItem().PaddingLeft(10).Element(e => CampoDato(e, "COD. ABI", C.ABI));
                row.RelativeItem().PaddingLeft(10).Element(e => CampoDato(e, "COD. CAB", C.CAB));
            });

            col.Item().PaddingTop(10).Element(e => CampoDato(e, "IBAN", C.IBAN));

            col.Item().PaddingTop(15).Element(e => CampoDato(e, "Pagamento proposto", C.PagProposto));

            // Sezione Riservata Proprietà
            col.Item().PaddingTop(10).Row(row => {
                row.RelativeItem().Element(e => CampoDato(e, "Pagamento accettato (a cura della Proprietà)", ""));
                row.RelativeItem().PaddingLeft(15).Element(e => CampoDato(e, "FIDO EURO (a cura della Proprietà)", ""));
            });

            col.Item().PaddingTop(30);

            // --- INFORMAZIONI COMMERCIALI ---
            col.Item().Text("INFORMAZIONI COMMERCIALI").FontSize(11).Bold().Underline();
            col.Item().PaddingTop(5).Text("Indicare listino profili, attuale fornitore, attività prevalente (serramenti, persiane, facciate)").FontSize(8).Italic();

            col.Item().PaddingTop(10).Border(0.5f).MinHeight(60).Padding(5).Text(C.InfoCommerciali ?? "").FontSize(9);

            col.Item().PaddingTop(10).Element(e => CampoDato(e, "Previsione consumo annuo (kg circa o euro):", C.PrevisioneAnnuo?.ToString("N0") ?? ""));

            col.Item().PaddingTop(30);

            // --- INFORMAZIONI LOGISTICHE ---
            col.Item().Text("INFORMAZIONI LOGISTICHE").FontSize(11).Bold().Underline();
            col.Item().PaddingTop(5).Text("Barrare la casella interessata").FontSize(8).Italic();

            col.Item().PaddingTop(10).Column(c => {
                c.Item().Text($"{(C.Carrello ? "☒" : "☐")} Il cliente possiede carrello elevatore o carroponte per scaricare").FontSize(9);
                c.Item().PaddingTop(5).Text($"{(C.ScaricoAgevole ? "☒" : "☐")} Il luogo di scarico è agevolmente raggiungibile con un mezzo pesante").FontSize(9);
            });

            col.Item().PaddingTop(15).Element(e => CampoDato(e, "Altre annotazioni:", C.Annotazioni));

            // --- SPAZIO TIMBRO ---
            col.Item().PaddingTop(40).AlignRight().Column(c => {
                c.Item().Width(200).Height(80).Border(0.5f).AlignCenter().AlignMiddle().Text("TIMBRO DITTA").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }


    void CampoDato(IContainer container, string etichetta, string valore)
    {
        container.Row(row => {
            row.AutoItem().PaddingRight(5).Text(etichetta).FontSize(9).Bold();
            row.RelativeItem().BorderBottom(0.5f).PaddingBottom(1).Text(valore).FontSize(10);
        });
    }

    void CampoAgenteSenzaLinea(IContainer container, string etichetta, string valore)
    {
        container.Row(row => {
            row.AutoItem().AlignBottom().PaddingRight(10).Text(etichetta).FontSize(16).Bold();
            row.RelativeItem().AlignBottom().Text(valore).FontSize(20).ExtraBold();
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.Column(col => {
            col.Item().PaddingBottom(8).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2);
            col.Item().AlignCenter().Column(c => {
                c.Item().AlignCenter().Text("ALQ GENOVA S.r.l.").FontSize(15).ExtraBold();
                string[] info = {
                    "Sede Legale: via Santa Lucia, 5 – 20122 Milano (MI)",
                    "Sede Operativa: 16162 Genova Bolzaneto (GE) - via Colano, 9/A",
                    "Sede Operativa: 50013 Campi Bisenzio (FI) - via Gobetti, 3",
                    "Sede Amministrativa: 13030 Formigliana (VC) S.S. 230 – Fornace Crocicchio",
                    "Tel. +39 010 7491941 - E-mail: info@alqgenovasrl.com - Pec: alqgenova@pro-pec.it",
                    "C.F./P.Iva e N. Reg.Imprese MI. 12574490962 - REA/CCIAA MI26700419 - Cap. Soc. €. 150.000,00 i.v."
                };
                foreach (var line in info) c.Item().AlignCenter().Text(line).FontSize(10);
            });
        });
    }
}
