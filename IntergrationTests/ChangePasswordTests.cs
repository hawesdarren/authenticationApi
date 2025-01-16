using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authentication.Application;
using Shouldly;
using Flurl;
using Flurl.Http;

namespace IntergrationTests
{
    public class ChangePasswordTests : CustomClientFactory
    {
        [Test]
        public async Task ChangePasswordsTest() {
            // Login 1st as token is required for change password
            Authentication.Json.Requests.LoginRequest loginRequest = new()
            {
                email = "someone4@somewhere.co.nz",
                password = "Testing123",

            };

            var loginResponse = await "https://localhost:443"
                            .AppendPathSegment("api/authentication/login")
                            .PostJsonAsync(loginRequest)
                            .ReceiveJson<Authentication.Json.Responses.LoginResponse>();
            loginResponse.Success.ShouldBeTrue();
            var token = loginResponse.token;

            // Change password
            Authentication.Json.Requests.ChangePasswordRequest request = new()
            {
                password = "Testing123",
                confirmPassword = "Testing123",
            };

            var response = await "https://localhost:443"
                            .AppendPathSegment("api/authentication/change/password")
                            .WithHeader("Authorization", "Bearer " + token)
                            .PostJsonAsync(request)
                            .ReceiveJson<Authentication.Json.Responses.ChangePasswordResponse>();
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBeTrue()
                );
        }

        [Test]
        public async Task ChangePasswordsTestNoToken()
        {
           
            // Change password
            Authentication.Json.Requests.ChangePasswordRequest request = new()
            {
                password = "Testing123",
                confirmPassword = "Testing123",
            };

            var response = await "https://localhost:443"
                            .AppendPathSegment("api/authentication/change/password")
                            .WithHeader("Authorization", "Bearer " + null)
                            .AllowAnyHttpStatus()
                            .PostJsonAsync(request);

            response.StatusCode.ShouldBe(401);

        }

    }
}
