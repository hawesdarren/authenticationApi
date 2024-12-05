﻿using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authentication.Application
{
    public class Token
    {
                
        public static string GenerateJwtToken(string email)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["authentication:issuerSigningKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["authentication:validIssuer"],
                audience: config["authentication:validAudience"],
                claims: new List<Claim> {
                    //new Claim("Email", email),
                    new Claim(ClaimTypes.Email, email),
                },
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);
                
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        
    }
}