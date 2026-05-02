using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using GestioneCommerciale.Models;

public class VisitaService
{
    // Usiamo la tua stringa di connessione esatta
    private string conn = @"Server=.\SQLEXPRESS;Database=GestAgenti;Trusted_Connection=True;TrustServerCertificate=True;";

    public async Task<List<ReportVisitaRiunione>> GetVisitePerReportRiunione(DateTime inizio, DateTime fine)
    {
        using var connection = new SqlConnection(conn);

        var sql = @"
            SELECT 
                v.IDVisita, v.DataVisita, v.Motivazione, v.NoteEsito, 
                v.ReferenteIncontrato, v.TemperaturaCliente, v.RichiedeSeguito,
                c.RagSociale, c.Citta_Legale, c.Prov_Legale, c.IsNuovo
            FROM Visite v
            INNER JOIN Clienti c ON v.IDCliente = c.IdCliente
            WHERE CAST(v.DataVisita AS DATE) BETWEEN CAST(@inizio AS DATE) AND CAST(@fine AS DATE)
            ORDER BY c.Prov_Legale ASC, c.Citta_Legale ASC, v.DataVisita DESC";

        var result = await connection.QueryAsync<ReportVisitaRiunione>(sql, new { inizio, fine });
        return result.ToList();
    }
}
