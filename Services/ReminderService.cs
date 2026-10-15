using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using GestioneCommerciale.Models;

namespace GestioneCommerciale.Services
{
    using Microsoft.Extensions.Logging;
    using System.Net.Http;
    using Microsoft.Extensions.Configuration;

    public class ReminderService
    {
        private readonly string _conn = @"Server=.\SQLEXPRESS;Database=GestAgenti;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly TelegramService? _telegram;
        private readonly ILogger<ReminderService>? _logger;
        private readonly IConfiguration? _config;

        public ReminderService(TelegramService? telegram = null, ILogger<ReminderService>? logger = null, IConfiguration? config = null)
        {
            _telegram = telegram;
            _logger = logger;
            _config = config;
        }

        private static string EscapeForHtml(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
        }

        // Ensure table exists and return number of inserted rows (0 if existed)
        public async Task<int> CreateLogIfNotExistsAsync(int idVisita, DateTime dataPromemoria, string metodo = "AUTO")
        {
            using var db = new SqlConnection(_conn);
            var createSql = @"IF OBJECT_ID('dbo.PromemoriaLog','U') IS NULL
                CREATE TABLE dbo.PromemoriaLog(
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    IDVisita INT NOT NULL,
                    DataPromemoria DATETIME NOT NULL,
                    Metodo NVARCHAR(50) NULL,
                    Gestito BIT NOT NULL DEFAULT 0
                );";
            await db.ExecuteAsync(createSql);

            var exists = await db.QueryFirstOrDefaultAsync<int?>("SELECT TOP 1 Id FROM dbo.PromemoriaLog WHERE IDVisita = @id AND Gestito = 0", new { id = idVisita });
            if (exists.HasValue) return 0;

            var insert = "INSERT INTO dbo.PromemoriaLog (IDVisita, DataPromemoria, Metodo, Gestito) VALUES (@IDVisita, @DataPromemoria, @Metodo, 0)";
            var res = await db.ExecuteAsync(insert, new { IDVisita = idVisita, DataPromemoria = dataPromemoria, Metodo = metodo });

            if (res > 0 && _telegram != null)
            {
                try
                {
                    // Recupera informazioni cliente/contatti per arricchire la notifica
                    var cliente = await db.QueryFirstOrDefaultAsync(@"SELECT c.RagSociale, c.Cel1, c.Cel2, c.Telefono
                                                                        FROM Visite v
                                                                        LEFT JOIN Clienti c ON v.IDCliente = c.IdCliente
                                                                        WHERE v.IDVisita = @IdVisita",
                                                                        new { IdVisita = idVisita });

                    string nome = cliente?.RagSociale ?? "(cliente sconosciuto)";
                    string telefono = cliente?.Cel1 ?? cliente?.Cel2 ?? cliente?.Telefono ?? "n.d.";

                    // costruisci messaggio HTML con link alla visita (se possibile)
                    var configuredBase = _config?[
                        "Telegram:BaseUrl"] ?? _config?["App:BaseUrl"];
                    var visitaPath = $"/nuova-visita?id={idVisita}"; // route usata in UI
                    string msg;
                    if (string.IsNullOrWhiteSpace(configuredBase))
                    {
                        _logger?.LogWarning("App:BaseUrl o Telegram:BaseUrl non configurati: il link nel messaggio Telegram sarà relativo. Impostare App:BaseUrl nelle configurazioni per abilitare link cliccabili.");
                        // mostra percorso testuale per copia/incolla
                        msg = $"<b>Promemoria creato</b>\nVisita <code>{idVisita}</code>\nCliente: <b>{EscapeForHtml(nome)}</b>\nTel: <code>{EscapeForHtml(telefono)}</code>\nData: <b>{dataPromemoria:g}</b>\nApri visita: <code>{visitaPath}</code>";
                    }
                    else
                    {
                        var baseUrl = configuredBase.TrimEnd('/');
                        msg = $"<b>Promemoria creato</b>\nVisita <code>{idVisita}</code>\nCliente: <b>{EscapeForHtml(nome)}</b>\nTel: <code>{EscapeForHtml(telefono)}</code>\nData: <b>{dataPromemoria:g}</b>\n<a href=\"{baseUrl}{visitaPath}\">Apri visita</a>";
                    }
                    await _telegram.SendMessageIfConfiguredAsync(msg, html: true);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Errore invio notifica Telegram");
                }
            }

            return res;
        }

        public async Task<IEnumerable<dynamic>> GetPendingPromemoriaAsync()
        {
            using var db = new SqlConnection(_conn);
            var sql = @"
                SELECT pl.Id AS LogId, pl.IDVisita, pl.DataPromemoria, pl.Metodo, v.IDVisita AS VisitaID, v.IDCliente, v.DataVisita, v.DataProssimoContatto, c.RagSociale
                FROM dbo.PromemoriaLog pl
                INNER JOIN Visite v ON pl.IDVisita = v.IDVisita
                LEFT JOIN Clienti c ON v.IDCliente = c.IdCliente
                WHERE pl.Gestito = 0
                ORDER BY pl.DataPromemoria, v.DataProssimoContatto";
            var items = await db.QueryAsync(sql);
            return items;
        }

        public async Task MarkGestitoAsync(int logId)
        {
            using var db = new SqlConnection(_conn);
            await db.ExecuteAsync("UPDATE dbo.PromemoriaLog SET Gestito = 1 WHERE Id = @Id", new { Id = logId });
        }

        public async Task<List<GestioneCommerciale.Models.ReminderItem>> GetDueRemindersAsync(Models.ReminderSettings settings)
        {
            DateTime threshold = DateTime.Now;
            if (settings.LeadUnit == "Hours")
                threshold = DateTime.Now.AddHours(settings.LeadValue);
            else
                threshold = DateTime.Now.AddDays(settings.LeadValue);

            using var db = new SqlConnection(_conn);
            string sql = @"
                SELECT v.IDVisita, v.IDCliente, v.DataProssimoContatto, c.RagSociale
                FROM Visite v
                INNER JOIN Clienti c ON v.IDCliente = c.IdCliente
                LEFT JOIN dbo.PromemoriaLog pl ON pl.IDVisita = v.IDVisita
                WHERE v.DataProssimoContatto IS NOT NULL
                  AND v.DataProssimoContatto <= @threshold
                  AND pl.Id IS NULL -- esclude visite che hanno già un promemoria (pendente o gestito)
                ORDER BY v.DataProssimoContatto";

            var items = (await db.QueryAsync<GestioneCommerciale.Models.ReminderItem>(sql, new { threshold })).ToList();
            return items;
        }
    }
}
