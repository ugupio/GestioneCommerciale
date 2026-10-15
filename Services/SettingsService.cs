using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using GestioneCommerciale.Models;
using System.Linq;

namespace GestioneCommerciale.Services
{
    public class SettingsService
    {
        private readonly string _conn = @"Server=.\SQLEXPRESS;Database=GestAgenti;Trusted_Connection=True;TrustServerCertificate=True;";

        public async Task<ReminderSettings> GetReminderSettingsAsync()
        {
            using var db = new SqlConnection(_conn);
            // Ensure table exists
            var createSql = @"IF OBJECT_ID('dbo.ReminderSettings','U') IS NULL
                CREATE TABLE dbo.ReminderSettings(
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    LeadValue INT NOT NULL,
                    LeadUnit NVARCHAR(20) NOT NULL,
                    ReminderTime NVARCHAR(10) NOT NULL
                );";
            await db.ExecuteAsync(createSql);

            var item = (await db.QueryAsync<ReminderSettings>("SELECT TOP 1 LeadValue AS LeadValue, LeadUnit AS LeadUnit, ReminderTime AS ReminderTime FROM dbo.ReminderSettings ORDER BY Id DESC")).FirstOrDefault();
            if (item == null)
            {
                var def = new ReminderSettings();
                var insert = "INSERT INTO dbo.ReminderSettings (LeadValue, LeadUnit, ReminderTime) VALUES (@LeadValue, @LeadUnit, @ReminderTime)";
                await db.ExecuteAsync(insert, def);
                return def;
            }
            return item;
        }

        public async Task SaveReminderSettingsAsync(ReminderSettings settings)
        {
            using var db = new SqlConnection(_conn);
            var exists = await db.QueryFirstOrDefaultAsync<int?>("SELECT TOP 1 Id FROM dbo.ReminderSettings ORDER BY Id DESC");
            if (exists.HasValue)
            {
                await db.ExecuteAsync("UPDATE dbo.ReminderSettings SET LeadValue=@LeadValue, LeadUnit=@LeadUnit, ReminderTime=@ReminderTime WHERE Id=@Id", new { settings.LeadValue, settings.LeadUnit, settings.ReminderTime, Id = exists.Value });
            }
            else
            {
                await db.ExecuteAsync("INSERT INTO dbo.ReminderSettings (LeadValue, LeadUnit, ReminderTime) VALUES (@LeadValue, @LeadUnit, @ReminderTime)", settings);
            }
        }
    }
}
