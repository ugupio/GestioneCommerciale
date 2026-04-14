namespace GestioneCommerciale.Models;

public class Cliente
{
    public int IdCliente { get; set; }
    public string RagSociale { get; set; } = "";
    public string Referente { get; set; }
    public string Ind_legale { get; set; }
    public string CAP_Legale { get; set; }
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
    public int? PrevisioneAnnuo { get; set; }
    public bool Carrello { get; set; } // bit in SQL è bool in C#
    public bool ScaricoAgevole { get; set; }
    public string Annotazioni { get; set; }
    public decimal ScontoProfili { get; set; }
    public decimal ScontoAccessori { get; set; }
    // coordinate columns added to DB as float
    public float? Lat { get; set; } // Da Latitudine a Lat
    public float? Lon { get; set; } // Da Longitudine a Lon
}




public class Reso
{
    public int IDReso { get; set; }
    public int IDCliente { get; set; }
    public int? IDOrdine { get; set; }
    public DateTime? DataReso { get; set; }
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
    public decimal PrezzoTotaleRiga { get; set; } // Campo calcolato SQL
    public string ColoreInt { get; set; } = "";
    public string ColoreEst { get; set; } = "";
    public bool IsAccessorio { get; set; } // Identifica se è un profilo o un accessorio
    public int Verghe => !IsAccessorio ? (int)Quantita : 0;
    public string FinituraAccessorio { get; set; } = ""; // Solo per accessori
    public decimal LunghezzaVerga { get; set; }
    public decimal PesoAlMetro { get; set; }
    public List<DettaglioOrdine> Righe { get; set; } = new List<DettaglioOrdine>();

}

public class Visita
{
    public int IDVisita { get; set; }
    public int IDCliente { get; set; }
    public DateTime? DataVisita { get; set; }
    public string StatoVisita { get; set; }
    public string Motivazione { get; set; }
    public string ReferenteIncontrato { get; set; }
    public string NoteEsito { get; set; }
    public int TemperaturaCliente { get; set; }
    public bool RichiedeSeguito { get; set; }
    public DateTime? DataProssimoContatto { get; set; }
}


public class Ordine
{
    public int IDOrdine { get; set; }
    public int IDCliente { get; set; }
    public DateTime? DataOrdine { get; set; }
    public string NumeroDocumento { get; set; }
    public string StatoOrdine { get; set; }
    public decimal TotaleImponibile { get; set; }
    public DateTime DataConsegnaPrevista { get; set; }
    public DateTime? DataConsegnaEffettiva { get; set; }
    public string? ConcordatoCon { get; set; }


    public List<DettaglioOrdine> Righe { get; set; } = new List<DettaglioOrdine>();
}

public class RigaReso
{
    public int IDRigaReso { get; set; }
    public int IDReso { get; set; }
    public string Codice { get; set; }
    public string Descrizione { get; set; }
    public decimal Quantita { get; set; }
}

public class PerformanceData
{
    public decimal Fatturato { get; set; }
    public int Ordini { get; set; }
    public int Visite { get; set; }
    public int Resi { get; set; }
}


