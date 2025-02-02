using Authentication.Application;
using Authentication.Json.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using System.Net;

namespace UnitTests
{
    
    public class LoginTests
    {
        [TestCase("someone@somewhere.co.nz", "Testing123", true, true)]
        [TestCase("someone@somewhere.co.nz", "Testing1234", false, false)]
        [TestCase("noone@somewhere.co.nz", "Testing123", false, false)]
        [TestCase("@somewhere.co.nz", "Testing123", false, false)]
        public void LoginUserTest(string email, string password, bool success, bool authenticated)
        {
            
            Authentication.Json.Requests.LoginRequest request = new()
            {
                email = email,
                password = password,
                
            };

            LoginResponse response = LoginUser.ValidatePassword(request);
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBe(success),
                () => Convert.ToBoolean(response.Authenticated).ShouldBe(authenticated)
                );

        }
        
        [Test]
        public void PasswordBlockedLoginUserTest()
        {
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
                response = LoginUser.ValidatePassword(request);
            }
            // Login with locked password
            request.password = "Testing123";
            response = LoginUser.ValidatePassword(request);

            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBeFalse(),
                () => response.error.ShouldBe("PASSWORD_TEMP_BLOCK"),
                () => response.token.ShouldBeNullOrEmpty()
                );

        }

        [TestCase("someone4@somewhere.co.nz", "Testing123", true)]
        [TestCase("someone5@somewhere.co.nz", "Testing123", false)]
        public void LoginTfaEnabled(string email, string password, bool tfaEnabled) {
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

            response = LoginUser.ValidatePassword(request);
            response.ShouldSatisfyAllConditions(
               () => response.ShouldNotBeNull(),
               () => response.Success.ShouldBeTrue(),
               () => response.tfaEnabled.ShouldBe(tfaEnabled)
               );
        }
    }
}
