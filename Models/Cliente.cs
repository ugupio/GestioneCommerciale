namespace GestioneCommerciale.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string RagSociale { get; set; }
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
        public int PrevisioneAnnuo { get; set; }
        public string Annotazioni { get; set; }
        public decimal ScontoProfili { get; set; }
        public decimal ScontoAccessori { get; set; }
        public bool Carrello { get; set; }
        public bool ScaricoAgevole { get; set; }
    }
}
