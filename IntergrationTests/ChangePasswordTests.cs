using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authentication.Application;
using FluentAssertions;
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
            loginResponse.Success.Should().BeTrue();
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
            response.ToString().Should().NotBeNull();
            response.Success.Should().BeTrue();

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

            response.StatusCode.Should().Be(401);

        }

    }
}
