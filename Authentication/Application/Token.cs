using Authentication.Json.Responses;
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
                
        public static string GenerateJwtToken(string email, bool authentication, int timeout)
        {
            
            //var config = new ConfigurationBuilder().AddUserSecrets<Program>().AddJsonFile("secrets.json", optional: true).Build();
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var projectDir = Directory.GetParent("Authentication");
            var secretFiles = Directory.EnumerateFiles(".", "secrets.json", SearchOption.AllDirectories);
            foreach (var path in secretFiles)
            {
                builder.AddJsonFile(path, optional: true);
                //config.Configuration.AddJsonFile(path);
            }
            var config = builder.Build();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["authentication:issuerSigningKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["authentication:validIssuer"],
                audience: config["authentication:validAudience"],
                claims: new List<Claim> {
                    new Claim(ClaimTypes.Authentication, authentication.ToString()),
                    new Claim(ClaimTypes.Email, email),
                },
                expires: DateTime.Now.AddMinutes(timeout),
                signingCredentials: creds);
                
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string RefreshToken(int timeout) {

            //var config = new ConfigurationBuilder().AddUserSecrets<Program>().AddJsonFile("secrets.json", optional: true).Build();
            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var projectDir = Directory.GetParent("Authentication");
            var secretFiles = Directory.EnumerateFiles(".", "secrets.json", SearchOption.AllDirectories);
            foreach (var path in secretFiles)
            {
                builder.AddJsonFile(path, optional: true);
                //config.Configuration.AddJsonFile(path);
            }
            var config = builder.Build();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["authentication:issuerSigningKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["authentication:validIssuer"],
                audience: config["authentication:validAudience"],
                // Most likely need cliams that a different to the authToken
                /*claims: new List<Claim> {
                    new Claim(ClaimTypes.Authentication, authentication.ToString()),
                    new Claim(ClaimTypes.Email, email),
                },*/
                expires: DateTime.Now.AddMinutes(timeout),
                signingCredentials: creds);


            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public static long? GetExpiryFromToken(string token) 
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var exp = jwtToken.Payload.Expiration; // This is a long? (nullable)
            return exp;
        }

    }
}
