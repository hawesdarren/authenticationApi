using Authentication.Application;
using Authentication.Json.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    
    public class LoginTests
    {
        private IOptions<AuthenticationOptions> BuildOptions()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("app/secret-volume/secrets.json", optional: true, reloadOnChange: false)
                .AddUserSecrets(typeof(AuthenticationOptions).Assembly)
                .Build();
            var auth = new AuthenticationOptions();
            config.GetSection("Authentication").Bind(auth);
            return Options.Create(auth);
        }

        [TestCase("someone@somewhere.co.nz", "Testing123$", true, true)]
        [TestCase("someone@somewhere.co.nz", "Testing1234$", false, false)]
        [TestCase("noone@somewhere.co.nz", "Testing123$", false, false)]
        [TestCase("@somewhere.co.nz", "Testing123$", false, false)]
        public void LoginUserTest(string email, string password, bool success, bool authenticated)
        {
            var options = BuildOptions();

            Authentication.Json.Requests.LoginRequest request = new()
            {
                email = email,
                password = password,
                
            };
            LoginUser loginUser = new(options);
            LoginResponse response = loginUser.ValidatePassword(request);
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBe(success),
                () => Convert.ToBoolean(response.Authenticated).ShouldBe(authenticated)
                );

        }
        
        [Test]
        public void PasswordBlockedLoginUserTest()
        {
            var options = BuildOptions();

            LoginUser loginUser = new(options);
            Authentication.Json.Requests.LoginRequest request = new()
            {
                email = "someone2@somewhere.co.nz",
                password = "testing1234",

            };

            LoginResponse response =  new() { 
                Success = true,
                Authenticated = false
            };
            // Lock the password for the user
            for (int i = 0; i <= 6; i++) {
                response = loginUser.ValidatePassword(request);
            }
            // Login with locked password
            request.password = "Testing123$";
            response = loginUser.ValidatePassword(request);

            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBeFalse(),
                () => response.error.ShouldBe("PASSWORD_TEMP_BLOCK"),
                () => response.token.ShouldBeNullOrEmpty()
                );

        }

        [TestCase("someone4@somewhere.co.nz", "Testing123$", true)]
        [TestCase("someone5@somewhere.co.nz", "Testing123$", false)]
        public void LoginTfaEnabled(string email, string password, bool tfaEnabled) {
            var options = BuildOptions();

            Authentication.Json.Requests.LoginRequest request = new()
            {
                email = email,
                password = password,

            };

            LoginResponse response = new()
            {
                Success = true,
                Authenticated = false
            };

            LoginUser loginUser = new(options);
            response = loginUser.ValidatePassword(request);
            response.ShouldSatisfyAllConditions(
               () => response.ShouldNotBeNull(),
               () => response.Success.ShouldBeTrue(),
               () => response.tfaEnabled.ShouldBe(tfaEnabled)
               );
        }
    }
}
