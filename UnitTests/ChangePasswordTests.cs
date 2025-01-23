using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authentication.Application;
using Shouldly;

namespace UnitTests
{
    public class ChangePasswordTests
    {
        [TestCase("someone3@somewhere.co.nz", "Testing345", "Testing345", true)]
        [TestCase("someone3@somewhere.co.nz", "Testing3455", "Testing234", false)]
        [TestCase("someone3@somewhere.co.nz", "Password123", "Password123", false)]
        [TestCase("someone3@somewhere.co.nz", "TestingABC", "TestingABC", false)]
        [TestCase("someone3@somewhere.co.nz", "testing123", "Testing123", false)]
        public void ChangePasswordsTest(string email, string password, string renteredPasswrod, bool expectedResult) {
            Authentication.Json.Requests.ChangePasswordRequest request = new()
            {
                password = password,
                confirmPassword = renteredPasswrod,
            };
                        
            var result = ChangePassword.Change(request, email);
            result.Success.ShouldBe(expectedResult);
        }
    }
}
