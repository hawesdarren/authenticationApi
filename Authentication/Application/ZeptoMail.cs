using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Authentication.Application
{
    public class ZeptoMail
    {
        // Move to secrets when ready
        private const string ApiUrl = "https://api.zeptomail.com/v1.1/email";
        private const string ApiKey = "YOUR_ZEPTOMAIL_API_KEY"; // Replace with your actual API key
        private const string FromAddress = "noreply@hawes.co.nz"; // Replace with your sender address

        public static async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-enczapikey", ApiKey);

            var payload = new
            {
                from = new { address = FromAddress },
                to = new[] { new { email_address = to } },
                subject = subject,
                htmlbody = body
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(ApiUrl, content);
            return response.IsSuccessStatusCode;
        }
    }
}
