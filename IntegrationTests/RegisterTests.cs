using Authentication.Application;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Microsoft.AspNetCore.Identity.Data;
using Org.BouncyCastle.Bcpg.OpenPgp;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using Shouldly;
using Flurl;
using Flurl.Http;


namespace IntegrationTests
{
    public class RegisterTests : CustomClientFactory
    {
    

        [Test]
        public async Task RegisterUserTest()
        {
            string email = RandonString() + "@somewhere.co.nz";
            Authentication.Json.Requests.RegisterRequest request = new()  { 
                email = email,
                password = "Testing123$",
                renteredPassword = "Testing123$"
            };

            var response = await Settings.GetSUT()
                            .AppendPathSegment("api/authentication/register")
                            .PostJsonAsync(request)
                            .ReceiveJson<Authentication.Json.Responses.RegisterResponse>();
            response.ShouldSatisfyAllConditions(
                () => response.ToString().ShouldNotBeNullOrEmpty(),
                () => response.Success.ShouldBeTrue()
                //() => response.token.ShouldNotBeNullOrEmpty()
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