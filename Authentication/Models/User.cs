using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.Security.Claims;

namespace Authentication.Models
{
    public class User
    {

        public string Email { get; set; }
        public string Password { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<Claim> Claims() 
        {
            var claims = new List<Claim>
            { new Claim(ClaimTypes.Email, Email) };
            claims.AddRange(Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            return claims;
        }

       
    }
}
