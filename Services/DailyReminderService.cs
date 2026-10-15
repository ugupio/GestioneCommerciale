using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using GestioneCommerciale.Services;
using System.Data.SqlClient;
using Dapper;
using GestioneCommerciale.Models;

namespace GestioneCommerciale.Services;

public class DailyReminderService : BackgroundService
{
    private readonly ReminderSettings _settings;
    private readonly ILogger<DailyReminderService> _logger;
    private readonly string _conn = @"Server=.\SQLEXPRESS;Database=GestAgenti;Trusted_Connection=True;TrustServerCertificate=True;";
    private readonly SettingsService _settingsService;
    private readonly TelegramService? _telegram;
    private readonly IConfiguration _config;

    public DailyReminderService(ReminderSettings settings, ILogger<DailyReminderService> logger, SettingsService settingsService, TelegramService? telegram = null, IConfiguration? config = null)
    {
        _settings = settings;
        _logger = logger;
        _settingsService = settingsService;
        _telegram = telegram;
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyReminderService avviato");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                // parse reminder time HH:mm
                if (TimeSpan.TryParse(_settings.ReminderTime, out var targetTime))
                {
                    var next = DateTime.Today.Add(targetTime);
                    if (next <= now) next = next.AddDays(1);

                    var delay = next - now;
                    _logger.LogInformation($"DailyReminderService dormirà per {delay}");
                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested) break;

                    // Before checking, reload persisted settings (if any)
                    try
                    {
                        var persisted = await _settingsService.GetReminderSettingsAsync();
                        if (persisted != null)
                        {
                            _settings.LeadUnit = persisted.LeadUnit;
                            _settings.LeadValue = persisted.LeadValue;
                            _settings.ReminderTime = persisted.ReminderTime;
                        }
                    }
                    catch { }

                    await CheckAndLogReminders();
                }
                else
                {
                    // default sleep 1 hour se orario non valido
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
            catch (TaskCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DailyReminderService");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task CheckAndLogReminders()
    {
        try
        {
            DateTime threshold = DateTime.Now;
            if (_settings.LeadUnit == "Hours")
                threshold = DateTime.Now.AddHours(_settings.LeadValue);
            else
                threshold = DateTime.Now.AddDays(_settings.LeadValue);

            using var db = new SqlConnection(_conn);
            string sql = @"
                SELECT v.IDVisita, v.IDCliente, v.DataProssimoContatto, c.RagSociale
                FROM Visite v
                INNER JOIN Clienti c ON v.IDCliente = c.IdCliente
                WHERE v.DataProssimoContatto IS NOT NULL
                  AND v.DataProssimoContatto <= @threshold
                ORDER BY v.DataProssimoContatto";

            // include phone numbers to enrich the summary
            sql = @"
                SELECT v.IDVisita, v.IDCliente, v.DataProssimoContatto, c.RagSociale, c.Cel1, c.Cel2, c.Telefono
                FROM Visite v
                INNER JOIN Clienti c ON v.IDCliente = c.IdCliente
                WHERE v.DataProssimoContatto IS NOT NULL
                  AND v.DataProssimoContatto <= @threshold
                  AND NOT EXISTS (SELECT 1 FROM dbo.PromemoriaLog pl WHERE pl.IDVisita = v.IDVisita)
                ORDER BY v.DataProssimoContatto";

            var items = (await db.QueryAsync(sql, new { threshold })).ToList();
            int count = 0;
            if (items.Count == 0)
            {
                _logger.LogInformation($"DailyReminderService: nessun promemoria entro {threshold}");
                return;
            }

            // build HTML summary
            var baseUrl = _config["App:BaseUrl"] ?? "#";
            var parts = new List<string>();
            parts.Add("<b>Riepilogo promemoria</b>");
            parts.Add($"<i>Generato il {DateTime.Now:G}</i>");
            parts.Add("\n");

            int idx = 1;
            foreach (var it in items)
            {
                string nome = WebUtility.HtmlEncode((it.RagSociale as string) ?? "(cliente sconosciuto)");
                string telefono = WebUtility.HtmlEncode((it.Cel1 as string) ?? (it.Cel2 as string) ?? (it.Telefono as string) ?? "n.d.");
                string data = WebUtility.HtmlEncode(((DateTime)it.DataProssimoContatto).ToString("g"));
                string visitaLink = WebUtility.HtmlEncode($"{baseUrl.TrimEnd('/')}/visite/{it.IDVisita}");

                parts.Add($"{idx}. Visita <code>{it.IDVisita}</code> — <b>{nome}</b> ({telefono})\nData: <b>{data}</b> — <a href=\"{visitaLink}\">Apri visita</a>");
                idx++;
                count++;
            }

            var message = string.Join("\n", parts);

            bool sent = false;
            if (_telegram != null)
            {
                try
                {
                    sent = await _telegram.SendMessageIfConfiguredAsync(message, html: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore invio riepilogo Telegram");
                }
            }

            if (sent)
            {
                // mark each as logged to avoid duplicate reminders
                var createSql = @"IF OBJECT_ID('dbo.PromemoriaLog','U') IS NULL
                    CREATE TABLE dbo.PromemoriaLog(
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        IDVisita INT NOT NULL,
                        DataPromemoria DATETIME NOT NULL,
                        Metodo NVARCHAR(50) NULL,
                        Gestito BIT NOT NULL DEFAULT 0
                    );";
                await db.ExecuteAsync(createSql);

                var insertSql = @"IF NOT EXISTS (SELECT 1 FROM dbo.PromemoriaLog WHERE IDVisita = @IDVisita)
                    INSERT INTO dbo.PromemoriaLog (IDVisita, DataPromemoria, Metodo, Gestito) VALUES (@IDVisita, @DataPromemoria, @Metodo, 0)";

                foreach (var it in items)
                {
                    try
                    {
                        await db.ExecuteAsync(insertSql, new { IDVisita = (int)it.IDVisita, DataPromemoria = (DateTime)it.DataProssimoContatto, Metodo = "DAILY" });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Impossibile inserire PromemoriaLog per visita {it.IDVisita}");
                    }
                }
            }

            _logger.LogInformation($"DailyReminderService completato: inviati {count} promemoria entro {threshold}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il controllo promemoria");
        }
    }
}
