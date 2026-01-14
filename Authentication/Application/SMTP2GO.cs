using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Authentication.Application
{
    public class SMTP2GO
    {
        //todo use SMTP2GO instead

        private const string ApiUrl = "https://api.smtp2go.com/v3/email/send";
        private const string FromAddress = "noreply@hawes.co.nz";
        private readonly SmtpOptions _smtpOptions;

        public SMTP2GO(IOptions<SmtpOptions> smtpOptions)
        {
            _smtpOptions = smtpOptions.Value;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            // Get secrets from configuration when ready
            var ApiKey = _smtpOptions.ApiKey;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("X-Smtp2go-Api-Key", ApiKey);

            var payload = new
            {
                sender = FromAddress,
                to = new[] { to },
                subject = subject,
                html_body = body
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(ApiUrl, content);
            return response.IsSuccessStatusCode;
        }
    }
}
