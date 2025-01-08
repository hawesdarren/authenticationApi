using Authentication.Application;
using Authentication.Json.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net;

namespace UnitTests
{
    public class LoginTests
    {
        [TestCase("someone@somewhere.co.nz", "Testing123", true)]
        [TestCase("someone@somewhere.co.nz", "Testing1234", false)]
        [TestCase("noone@somewhere.co.nz", "Testing123", false)]
        [TestCase("@somewhere.co.nz", "Testing123", false)]
        public void LoginUserTest(string email, string password, bool expectedResult)
        {
            
            Authentication.Json.Requests.LoginRequest request = new()
            {
                email = email,
                password = password,
                
            };

            LoginResponse response = LoginUser.ValidatePassword(request);
            response.Should().NotBeNull();
            response.Success.Should().Be(expectedResult);

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
            };
            // Lock the password for the user
            for (int i = 0; i <= 6; i++) {
                response = LoginUser.ValidatePassword(request);
            }
            // Login with locked password
            request.password = "Testing123";
            response = LoginUser.ValidatePassword(request);

            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.error.Should().Be("PASSWORD_TEMP_BLOCK");
            response.token.Should().BeNull();

        }
    }
}
