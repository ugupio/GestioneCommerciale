namespace GestioneCommerciale.Models
{
    public class Ordine
    {
        public int IDOrdine { get; set; }
        public int IDCliente { get; set; }
        public DateTime DataOrdine { get; set; } = DateTime.Now;
        public string NumeroDocumento { get; set; }
        public string StatoOrdine { get; set; }
        public decimal TotaleImponibile { get; set; }
        public decimal TotaleIva { get; set; }
    }

    // Questa è la classe fondamentale per l'inserimento
    public class OrdineTestata : Ordine
    {
        // È qui che definiamo la lista delle righe!
        public List<DettaglioOrdine> Righe { get; set; } = new List<DettaglioOrdine>();
    }

    public class DettaglioOrdine
    {
        public int IDOrdine { get; set; }
        public string Descrizione { get; set; }
        public decimal Quantita { get; set; }
        public decimal PrezzoUnitario { get; set; }
        public decimal ScontoRiga { get; set; }
        public decimal PrezzoTotaleRiga { get; set; }
    }
}
