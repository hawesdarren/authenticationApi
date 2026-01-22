using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authentication.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;

namespace UnitTests
{
    public class ChangePasswordTests
    {
        private IOptions<AuthenticationOptions> BuildOptions() {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secret-volume/secrets.json", optional: true, reloadOnChange: false)
                .AddUserSecrets(typeof(AuthenticationOptions).Assembly)
                .Build();
            var auth = new AuthenticationOptions();
            config.GetSection("Authentication").Bind(auth);
            return Options.Create(auth);
        }

        [TestCase("someone3@somewhere.co.nz", "Testing345$", "Testing345$", true)]
        [TestCase("someone3@somewhere.co.nz", "Testing$3455", "Testing$234", false)]
        [TestCase("someone3@somewhere.co.nz", "Password123", "Password123", false)]
        [TestCase("someone3@somewhere.co.nz", "TestingABC$", "TestingABC$", false)]
        [TestCase("someone3@somewhere.co.nz", "testing123$", "Testing123$", false)]
        public void ChangePasswordsTest(string email, string password, string renteredPasswrod, bool expectedResult) {
            var options = BuildOptions();
            
            
            Authentication.Json.Requests.ChangePasswordRequest request = new()
            {
                password = password,
                confirmPassword = renteredPasswrod,
            };
            ChangePassword changePassword = new(options);
            var result = changePassword.Change(request, email);
            result.Success.ShouldBe(expectedResult);
        }
    }
}
