using Authentication.Application;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Shouldly;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace UnitTests
{
    public class RegisterTests
    {
        private readonly IOptions<AuthenticationOptions> _authenticationOptions;

        private IOptions<AuthenticationOptions> BuildOptions()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secret-volume/secrets.json", optional: true, reloadOnChange: false)
                .AddUserSecrets(typeof(AuthenticationOptions).Assembly)
                .Build();
            var auth = new AuthenticationOptions();
            config.GetSection("Authentication").Bind(auth);
            return Options.Create(auth);
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Testing123$", "Testing123$", true)]
        [TestCase("Password123", "Password123", false)]
        [TestCase("Testing123", "Testing1234", false)]
        [TestCase("testing123", "testing123", false)]
        public void RegisterUserTest(string password, string rentteredPassword,bool expectedResult)
        {
            var options = BuildOptions();

            string email = Tools.RandonString() + "@somewhere.com";
            Authentication.Json.Requests.RegisterRequest request = new()  { 
                email = email,
                password = password,
                renteredPassword = rentteredPassword
            };
            RegisterUser registerUser = new(options);
            RegisterResponse response = registerUser.Register(request);
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBe(expectedResult)
                );

        }

        [Test]
        public void AlreadyRegisteredUserTest()
        {
            var options = BuildOptions(); 
            
            string email = Tools.RandonString() + "@somewhere.com";
            Authentication.Json.Requests.RegisterRequest request = new()
            {
                email = email,
                password = "Testing123$",
                renteredPassword = "Testing123$"
            };
            RegisterUser registerUser = new(options);
            RegisterResponse response1 = registerUser.Register(request);
            RegisterResponse response2 = registerUser.Register(request);
            response2.ShouldSatisfyAllConditions(
                () => response2.ShouldNotBeNull(),
                () => response2.Success.ShouldBeFalse()
                );

        }

        [Test]
        public void RegisteredUserEmailFormatTest()
        {
            var options = BuildOptions();

            Authentication.Json.Requests.RegisterRequest request = new()
            {
                email = "someone@",
                password = "Testing123$",
                renteredPassword = "Testing123$"
            };
            RegisterUser registerUser = new(options);
            RegisterResponse response = registerUser.Register(request);
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBeFalse()
                );

        }

    }
}