using Authentication.Application;
using Authentication.Json.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Flurl;
using Flurl.Http;
using System.Net;


namespace IntergrationTests
{
    public class LoginTests : CustomClientFactory
    {
        [Test]
        public async Task LoginUserTest()
        {
            
            Authentication.Json.Requests.LoginRequest request = new()
            {
                email = "someone@somewhere.co.nz",
                password = "Testing123",
                
            };

            var response = await "https://localhost:443"
                            .AppendPathSegment("api/authentication/login")
                            .PostJsonAsync(request)
                            .ReceiveJson<Authentication.Json.Responses.LoginResponse>();
            response.ShouldSatisfyAllConditions(
                () => response.ToString().ShouldNotBeNullOrEmpty(),
                () => response.Success.ShouldBeTrue(),
                () => response.token.ShouldNotBeNullOrEmpty()
                );

        }
              
    }
}
