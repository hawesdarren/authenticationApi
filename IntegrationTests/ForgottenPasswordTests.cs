using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Shouldly;
using System;
using Flurl;
using Flurl.Http;


namespace IntegrationTests
{
    public class ForgottenPasswordTests
    {

        [Test]
        public async Task ForgottenPasswordTest()
        {
            // Create request for forgotten password
            ForgottenPasswordRequest request = new()
            {
                email = "darren@hawes.co.nz"
            };
            // Send request to the appropriate endpoint
            var response = await Settings.GetSUT()
                .AppendPathSegment("/api/authentication/forgotten/password")
                .PostJsonAsync(request)
                .ReceiveJson<ForgottenPasswordResponse>();
            // Validate response
            response.ShouldSatisfyAllConditions(
               () => response.ShouldNotBeNull(),
               () => response.Success.ShouldBeTrue()
            );

        }

    }
}
