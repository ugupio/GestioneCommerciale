namespace GestioneCommerciale.models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string RagSociale { get; set; }
    public string Referente { get; set; }
    public string Ind_legale { get; set; }
    public string Citta_Legale { get; set; }
    public string Prov_Legale { get; set; }
    public string Telefono { get; set; }    
    public string Fax { get; set; }
    public string Email { get; set; }
    public string Cel1 { get; set; }
    public string Cel2 { get; set; }
    public string PIva { get; set; }
    public string CodFiscale { get; set; }
    public string SDI { get; set; }
    public string Banca { get; set; }
    public string Agenzia { get; set; }
    public string ABI { get; set; }
    public string CAB { get; set; }
    public string IBAN { get; set; }
    public string PagProposto { get; set; }
    public string PagAccettato { get; set; }
    public string InfoCommerciali { get; set; }
    public int PrevisioneAnnuo { get; set; }
    public string Annotazioni { get; set; }
    public decimal ScontoProfili { get; set; }
    public decimal ScontoAccessori { get; set; }
    public bool Carrello { get; set; }
    public bool ScaricoAgevole { get; set; }
}


public class Ordine
{
    public int IDOrdine { get; set; }
    public int IDCliente { get; set; }
    public DateTime DataOrdine { get; set; }
    public string NumeroDocumento { get; set; }
    public string StatoOrdine { get; set; }
    public decimal TotaleImponibile{ get; set; }
    public decimal TotaleIva { get; set; }

}

public class  Reso
{
    public int IDReso { get; set; }
    public int IDCliente { get; set; }
    public int IDOrdine { get; set; }
    public DateTime DataReso { get; set; }
    public string StatoReso { get; set; }
    public string Motivazione { get; set; } 
    public string Aspetto { get; set; }

}

    public class DettaglioOrdine
{
    public int IDRigaOrdine { get; set; }
    public int IDOrdine { get; set; }
    public string Descrizione { get; set; }
    public decimal Quantita { get; set; }
    public decimal PrezzoUnitario { get; set; }
    public decimal ScontoRiga { get; set; }
    public decimal PrezzoTotaleRiga { get; set; } // SQL lo calcola, noi lo leggiamo
}

    public class Visita
{   public int IDVisita { get; set; }
    public int IDCliente { get; set; }
    public DateTime DataVisita { get; set; }
    public string StatoVisita { get; set; }
    public string Motivazione { get; set; }
    public string ReferenteIncontrato { get; set; }
    public string NoteEsito { get; set; }
    public int TemperaturaCliente { get; set; }
    public bool RichiedeSeguito { get; set; }
    public DateTime DataProssimoContatto { get; set; }
    
}

public class OrdineTestata
{
    public int IDCliente { get; set; }
    public DateTime DataOrdine { get; set; } = DateTime.Now;
    public string NumeroDocumento { get; set; }
    public decimal TotaleImponibile => Righe.Sum(r => r.PrezzoTotaleRiga);
    public List<DettaglioOrdine> Righe { get; set; } = new();
}



