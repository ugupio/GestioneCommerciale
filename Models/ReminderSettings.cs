namespace GestioneCommerciale.Models;

public class ReminderSettings
{
    // Value for lead time (integer)
    public int LeadValue { get; set; } = 1;
    // Unit: "Days" or "Hours"
    public string LeadUnit { get; set; } = "Days";
    // Daily reminder time (HH:mm)
    public string ReminderTime { get; set; } = "09:00";
}
