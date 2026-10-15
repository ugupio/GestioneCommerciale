using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GestioneCommerciale.Services
{
    public class TelegramService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<TelegramService> logger)
        {
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendMessageIfConfiguredAsync(string text, bool html = false)
        {
            try
            {
                var token = _config["Telegram:BotToken"];
                var chatId = _config["Telegram:ChatId"];
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatId))
                {
                    _logger.LogDebug("Telegram not configured - skipping message");
                    return false;
                }

                var client = _httpFactory.CreateClient();
                var url = $"https://api.telegram.org/bot{token}/sendMessage";
                var values = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string,string>("chat_id", chatId),
                    new KeyValuePair<string,string>("text", text)
                };
                if (html)
                {
                    values.Add(new KeyValuePair<string,string>("parse_mode", "HTML"));
                    // disable web page preview to keep message compact
                    values.Add(new KeyValuePair<string,string>("disable_web_page_preview", "true"));
                }

                var content = new FormUrlEncodedContent(values);

                var res = await client.PostAsync(url, content);
                if (!res.IsSuccessStatusCode)
                {
                    var body = await res.Content.ReadAsStringAsync();
                    _logger.LogWarning("Telegram send failed {Status} {Body}", res.StatusCode, body);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore invio Telegram");
                return false;
            }
        }
    }
}
