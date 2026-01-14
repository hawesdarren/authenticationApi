using Authentication.Application;
using Authentication.Application.Validations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Shouldly;

namespace UnitTests
{
    public class Tests
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

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("someone@somewhere.com", true)]
        [TestCase("someone.else@somewhere.com", true)]
        [TestCase("someone@somewhere.co.nz", true)]
        [TestCase("someone.com", false)]
        [TestCase("someone@com", true)]
        [TestCase("someone@", false)]
        public void EmailValidationTest(string email, bool expectedResult)
        {
            
            bool result = EmailValidation.ValidateFormat(email);
            result.ShouldBe(expectedResult);
        }

        [TestCase("Password123", false)]
        [TestCase("Password123@", true)]
        [TestCase("Password!@#", false)]
        [TestCase("Password", false)]
        [TestCase("password123$", false)]
        [TestCase("Password", false)]
        [TestCase("P23456", false)]
        [TestCase("P234567", false)]
        [TestCase("", false)]
        public void PasswordValidationTests(string password, bool expectedResult) { 
            bool result = PasswordValidation.PasswordComplexityCheck(password);
            result.ShouldBe(expectedResult);
        }

        [TestCase("someone@somewhere.com")]
        public void CreateTokenTest(string email) {
            var options = BuildOptions();
            Token token = new Token(options);
            var result = token.GenerateJwtToken(email, true, 10);
            result.ShouldSatisfyAllConditions(
                () => result.ShouldNotBeNullOrEmpty(),
                () => result.ShouldBeOfType<string>()
            );
 
        }

        [Test]
        public void ArgonSaltTests() {
            var salt = Argon.CreateSalt();
            salt.ShouldSatisfyAllConditions(
                () => salt.ShouldNotBeNull(),
                () => salt.ShouldBeOfType<byte[]>()
            );

        }

        [Test]
        public void ArgonCreatePasswordHashTests()
        {
            var salt = Argon.CreateSalt();
            var hashedPassword = Argon.CreateHashPassword("Password123", salt);
            hashedPassword.ShouldSatisfyAllConditions(
                () => hashedPassword.ShouldNotBeNull(),
                () => hashedPassword.ShouldBeOfType<byte[]>(),
                () => hashedPassword.Length.ShouldBe(32)
                );

        }

        [TestCase("Password123", true)]
        [TestCase("Password1234", false)]
        public void ArgonMatchPasswordTests(string password, bool expectedResult)
        {
            var salt = Argon.CreateSalt();
            var hashedPassword = Argon.CreateHashPassword("Password123", salt);
            var result = Argon.MatchPassword(password, Convert.ToHexString(hashedPassword), Convert.ToHexString(salt));
            result.ShouldBe(expectedResult);
        }
    }
}