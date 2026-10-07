using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using GestioneCommerciale.Models;

public class ProdottiService
{
    private string conn = @"Server=.\SQLEXPRESS;Database=GestAgenti;Trusted_Connection=True;TrustServerCertificate=True;";

    /// <summary>
    /// Cerca prodotti per termine (codice o descrizione). Se isAccessorio è true cerca solo accessori, se false solo profili, se null cerca entrambi.
    /// </summary>
    public async Task<List<Prodotti>> SearchProdottiAsync(string term, bool? isAccessorio = null, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(term)) return new List<Prodotti>();

        using var db = new SqlConnection(conn);
        string like = "%" + term.Replace("%", "[%]") + "%";

        string sql = @"
            SELECT TOP (@Limit) *
            FROM Prodotti
            WHERE (CodiceProdotto LIKE @Like OR Descrizione LIKE @Like)";

        if (isAccessorio.HasValue)
        {
            sql += " AND IsAccessorio = @IsAccessorio";
        }

        sql += " ORDER BY CodiceProdotto";

        var result = await db.QueryAsync<Prodotti>(sql, new { Like = like, IsAccessorio = isAccessorio == true ? 1 : 0, Limit = limit });
        return result.ToList();
    }

    /// <summary>
    /// Carica un prodotto esatto per codice
    /// </summary>
    public async Task<Prodotti> GetProdottoByCodiceAsync(string codice)
    {
        if (string.IsNullOrWhiteSpace(codice)) return null;
        using var db = new SqlConnection(conn);
        string sql = "SELECT TOP(1) * FROM Prodotti WHERE CodiceProdotto = @Codice";
        return await db.QueryFirstOrDefaultAsync<Prodotti>(sql, new { Codice = codice });
    }
}
