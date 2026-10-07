using System;
using System.IO;
using System.Threading.Tasks;

namespace GestioneCommerciale.Shared
{
    public static class LocalLogger
    {
        private static readonly object _lock = new object();

        public static async Task LogErrorAsync(string context, Exception ex)
        {
            try
            {
                var basePath = AppContext.BaseDirectory ?? Environment.CurrentDirectory;
                var logsDir = Path.Combine(basePath, "logs");
                if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);

                var file = Path.Combine(logsDir, "error.log");
                var text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Context: {context}\n{ex}\n----------------------------------------------------------------\n";

                // Use FileStream to avoid conflicts when multiple threads write
                lock (_lock)
                {
                    File.AppendAllText(file, text);
                }

                await Task.CompletedTask;
            }
            catch
            {
                // Do not throw from logger
            }
        }
    }
}
