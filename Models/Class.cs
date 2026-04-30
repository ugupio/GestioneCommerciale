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
    public int? GiorniDallUltimoOrdine { get; set; }
    public double DistanzaKM { get; set; }
    public int IdZona { get; set; }
    public int FrequenzaVisitaGg { get; set; }
}

public class Prodotti
{
    public int IDProdotto { get; set; }
    public string CodiceProdotto { get; set; } = "";
    public string Descrizione { get; set; } = "";
    public bool IsAccessorio { get; set; } // True = Accessorio, False = Profilo
    public string FinituraAccessorio { get; set; } = "";
    public decimal LunghezzaVerga { get; set; } = 0;
    public decimal PesoAlMetro { get; set; } = 0;
    public string NoteTecniche { get; set; } = "";
    public string UmProdotto { get; set; } = ""; // Esempio: "PZ" o "ML"
    public int PezziPerConfezione { get; set; } = 1; // Default a 1 per evitare divisioni per zero
    public string UmSecondaria { get; set; } = "CT"; // Default Cartone/Confezione

    // Helper per l'interfaccia Blazor
    public bool HaConfezione => IsAccessorio && PezziPerConfezione > 1;
}



public class Attivita
{
    public int IDAttivita { get; set; }
    public string Descrizione { get; set; } = "";
    public DateTime DataScadenza { get; set; } = DateTime.Now;
    public bool Completata { get; set; } = false;
    public int Priorita { get; set; } // 1=Bassa, 2=Media, 3=Alta

    // --- COLLEGAMENTI ATTIVI (Tutti opzionali) ---
    public int? IDClienteAttivita { get; set; }   // Se riferito a un Cliente
    public int? IDOrdineAttivita { get; set; }    // Se riferito a un Ordine specifico
    public int? IDProdottoAttivita { get; set; }  // Se riferito a un Prodotto (es. "Aggiornare peso")

    // Proprietà di navigazione per la UI (comode per Dapper)
    public string NomeRiferimento { get; set; } = ""; // Es: "Rossi SRL" o "Ordine #123"
    public string CodiceRiferimento { get; set; } = "";
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

    public string RagSociale { get; set; }
    public string Agente { get; set; } = "Gucci";
    public string RifDocumentoVendita { get; set; }
    public DateTime? DataDocumentoVendita { get; set; }
       
    public string Indirizzo { get; set; }
    public string CAP { get; set; }
    public string Citta { get; set; }
    public string Provincia { get; set; }
    public string PIva { get; set; }
    // ----------------------------

    public List<DettaglioReso> Righe { get; set; } = new();
}


public class DettaglioReso
{
    public string Codice { get; set; } = "";
    public string Descrizione { get; set; } = "";
    public decimal Quantita { get; set; }
    public string Colore { get; set; } = "";
    public string StatoBene { get; set; } = "";
}


public class DettaglioOrdine
{
    public int IDRigaOrdine { get; set; }
    public int IDOrdine { get; set; }
    public int IDProdotto { get; set; } // <--- AGGIUNTO: serve per il collegamento con l'archivio
    public string Descrizione { get; set; }
    public decimal Quantita { get; set; }
    public decimal PrezzoUnitario { get; set; }
    public decimal ScontoRiga { get; set; }
    public decimal PrezzoTotaleRiga { get; set; }
    public string ColoreInt { get; set; } = "";
    public string ColoreEst { get; set; } = "";
    public bool IsAccessorio { get; set; }
    public int Verghe => !IsAccessorio ? (int)Quantita : 0;
    public string FinituraAccessorio { get; set; } = "";
    public decimal LunghezzaVerga { get; set; }
    public decimal PesoAlMetro { get; set; }
    public string CodiceProdotto { get; set; } = "";
    public string UmRiga { get; set; }

    // --- AGGIORNAMENTI v2.6 PER GESTIONE CONFEZIONI ---
    public int PezziPerConfezione { get; set; } = 1; // Valore ereditato dall'anagrafica
    public bool IsConfezione { get; set; } = false; // Stato del Toggle (PZ vs CT)
    public string UmSecondaria { get; set; } = "CT"; // Etichetta (es. CT, PACCO, BOX)
    public decimal QuantitaOriginalePezzi { get; set; } // La nostra "memoria"
    public bool MostraToggleConfezione { get; set; }
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
    public int TemperaturaCliente { get; set; } = 2; // Cambia da int a string
    public bool RichiedeSeguito { get; set; }
    public DateTime? DataProssimoContatto { get; set; }
    public string RagSociale { get; set; }

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
    public string? RiferimentoCliente { get; set; }
    public string? NoteOrdine { get; set; }



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

public class ElementoAgenda
{
    public int IdAgenda { get; set; } // Identificativo sul DB
    public int? IdCliente { get; set; } // Per sapere da chi stiamo andando
    public DateTime Data { get; set; }
    public DateTime DataFine { get; set; }
    public string Titolo { get; set; }
    public string Tipo { get; set; } // Potresti usarlo per distinguere "Visita Programmata" da "Extra"
    public string Dettaglio { get; set; }
    public int PrioritaOLevel { get; set; }
    public bool IsChiuso { get; set; }

    // Questo rimane utile per legare l'oggetto UI al record DB
    public object OriginalObject { get; set; }
}

// In class.cs o Models.cs

public class TappaGiro
{
    public int IdCliente { get; set; }
    public string RagSociale { get; set; }
    public string Citta { get; set; }
    public DateTime OrarioArrivo { get; set; }
    public DateTime OrarioPartenza { get; set; }
    public double KmDaPuntoPrecedente { get; set; }
}

public class NominatimResult
{
    public string Lat { get; set; }
    public string Lon { get; set; }
    public string Display_Name { get; set; }
}

public class Zona
{
    public int IdZona { get; set; }
    public string NomeZona { get; set; }
    public string Descrizione { get; set; }
}


