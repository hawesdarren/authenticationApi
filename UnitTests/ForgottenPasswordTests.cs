using Authentication.Application;
using Authentication.Json.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;

namespace UnitTests
{
    public class ForgottenPasswordTests
    {
        private (IOptions<AuthenticationOptions> Auth, IOptions<SmtpOptions> Smtp) BuildOptions()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secret-volume/secrets.json", optional: true, reloadOnChange: false)
                .AddUserSecrets(typeof(AuthenticationOptions).Assembly)
                .Build();
            var auth = new AuthenticationOptions();
            config.GetSection("authentication").Bind(auth);
            var smtp = new SmtpOptions();
            config.GetSection("smtp2go").Bind(smtp);

            return (Options.Create(auth), Options.Create(smtp));
        }

        // Only use emails that you have access to for testing
        [Test]
        public void ForgottenPassword()
        {
            var options = BuildOptions();
            var authOptions = options.Auth;
            var smtpOptions = options.Smtp;

            // Generate request
            Authentication.Json.Requests.ForgottenPasswordRequest request = new()
            {
                email = "darren@hawes.co.nz"
            };

            ForgottenPassword forgottenPassword = new(smtpOptions, authOptions);
            ForgottenPasswordResponse response = forgottenPassword.ProcessForgottenPasswordAsync(request).Result;
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBeTrue()
            );
        }

        [Test]
        public void ForgottenPasswordInvalidEmail()
        {
            var options = BuildOptions();
            var authOptions = options.Auth;
            var smtpOptions = options.Smtp;

            // Generate request
            Authentication.Json.Requests.ForgottenPasswordRequest request = new()
            {
                email = "emaildoesntexist@hawes.co.nz"
            };

            ForgottenPassword forgottenPassword = new(smtpOptions, authOptions);
            ForgottenPasswordResponse response = forgottenPassword.ProcessForgottenPasswordAsync(request).Result;
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.error.ShouldBe("EMAIL_NOT_FOUND"),
                () => response.Success.ShouldBeFalse()
            );
        }
    }
}
