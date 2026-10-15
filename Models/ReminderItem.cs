using System;

namespace GestioneCommerciale.Models
{
    public class ReminderItem
    {
        public int IDVisita { get; set; }
        public int IDCliente { get; set; }
        public DateTime DataProssimoContatto { get; set; }
        public string RagSociale { get; set; } = string.Empty;
    }
}
