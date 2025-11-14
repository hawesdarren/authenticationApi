using Authentication.Application;
using Authentication.Application.Validations;
using Microsoft.Extensions.Primitives;
using Shouldly;

namespace UnitTests
{
    public class Tests
    {
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

        [TestCase("Password123", true)]
        [TestCase("Password123@", true)]
        [TestCase("Password1", true)]
        [TestCase("password1", false)]
        [TestCase("Password", false)]
        [TestCase("Password", false)]
        [TestCase("P23456", false)]
        [TestCase("P234567", true)]
        [TestCase("", false)]
        public void PasswordValidationTests(string password, bool expectedResult) { 
            bool result = PasswordValidation.PasswordComplexityCheck(password);
            result.ShouldBe(expectedResult);
        }

        [TestCase("someone@somewhere.com")]
        public void CreateTokenTest(string email) {
            var result = Token.GenerateJwtToken(email, true, 10);
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