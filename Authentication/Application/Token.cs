using Authentication.Json.Responses;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authentication.Application
{
    public class Token
    {
        private readonly AuthenticationOptions _authenticationOptions;

        public Token(IOptions<AuthenticationOptions> authenticationOptions)
        {
            _authenticationOptions = authenticationOptions.Value;
        }


        public string GenerateJwtToken(string email, bool authentication, int timeout)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationOptions.IssuerSigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _authenticationOptions.ValidIssuer,
                audience: _authenticationOptions.ValidAudience,
                claims: new List<Claim> {
                    new Claim(ClaimTypes.Authentication, authentication.ToString()),
                    new Claim(ClaimTypes.Email, email),
                },
                expires: DateTime.Now.AddMinutes(timeout),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string RefreshToken(int timeout) {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationOptions.IssuerSigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _authenticationOptions.ValidIssuer,
                audience: _authenticationOptions.ValidAudience,
                // Most likely need cliams that a different to the authToken
                /*claims: new List<Claim> {
                    new Claim(ClaimTypes.Authentication, authentication.ToString()),
                    new Claim(ClaimTypes.Email, email),
                },*/
                expires: DateTime.Now.AddMinutes(timeout),
                signingCredentials: creds);


            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public long? GetExpiryFromToken(string token) 
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var exp = jwtToken.Payload.Expiration; // This is a long? (nullable)
            return exp;
        }

    }
}
