using Authentication.Application;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Microsoft.AspNetCore.Identity.Data;
using Org.BouncyCastle.Bcpg.OpenPgp;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using Shouldly;

namespace UnitTests
{
    public class RegisterTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("Testing123", "Testing123", true)]
        [TestCase("Password123", "Password123", false)]
        [TestCase("Testing123", "Testing1234", false)]
        [TestCase("testing123", "testing123", false)]
        public void RegisterUserTest(string password, string rentteredPassword,bool expectedResult)
        {
            string email = RandonString() + "@somewhere.com";
            Authentication.Json.Requests.RegisterRequest request = new()  { 
                email = email,
                password = password,
                renteredPassword = rentteredPassword
            };

            RegisterResponse response = RegisterUser.Register(request);
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBe(expectedResult)
                );

        }

        [Test]
        public void AlreadyRegisteredUserTest()
        {
            string email = RandonString() + "@somewhere.com";
            Authentication.Json.Requests.RegisterRequest request = new()
            {
                email = email,
                password = "Testing123",
                renteredPassword = "Testing123"
            };

            RegisterResponse response1 = RegisterUser.Register(request);
            RegisterResponse response2 = RegisterUser.Register(request);
            response2.ShouldSatisfyAllConditions(
                () => response2.ShouldNotBeNull(),
                () => response2.Success.ShouldBeFalse()
                );

        }

        [Test]
        public void RegisteredUserEmailFormatTest()
        {
            Authentication.Json.Requests.RegisterRequest request = new()
            {
                email = "someone@",
                password = "Testing123",
                renteredPassword = "Testing123"
            };
                        
            RegisterResponse response = RegisterUser.Register(request);
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBeFalse()
                );

        }

        private string RandonString() { 
            Random random = new Random();
            int stringlen = random.Next(7, 14);
            int randValue;
            string str = "";
            char letter;
            for (int i = 0; i < stringlen; i++)
            {

                // Generating a random number. 
                randValue = random.Next(0, 26);

                // Generating random character by converting 
                // the random number into character. 
                letter = Convert.ToChar(randValue + 65);

                // Appending the letter to string. 
                str = str + letter;
            }
            return str;
        }
    }
}