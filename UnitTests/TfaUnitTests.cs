using Authentication.Application;
using Authentication.Json.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using OtpNet;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class TfaUnitTests : DatabaseConnector
    {
        private IOptions<AuthenticationOptions> BuildOptions()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secret-volume/secrets.json", optional: true, reloadOnChange: false)
                .AddUserSecrets(typeof(AuthenticationOptions).Assembly)
                .Build();
            var auth = new AuthenticationOptions();
            config.GetSection("Authentication").Bind(auth);
            return Options.Create(auth);
        }

        private string GenerateTfaCode(string secret) { 
            Totp totp = new Totp(secretKey: Base32Encoding.ToBytes(secret));
            return totp.ComputeTotp();
        }
        
        [TestCase("tfa.testing@hawes.co.nz")]
        public void GenerateTotp(string email)
        {
            var options = BuildOptions();
            // Email must be registerd to create a totp 
            Tfa tfa = new Tfa(options);
            TfaSetupResponse response = tfa.CreateNewTotp(email, "test-issuer");
            response.ShouldSatisfyAllConditions(
                () => response.ShouldNotBeNull(),
                () => response.Success.ShouldBeTrue()
                );

        }

        [Test]
        public void IsTfaEnabled() {
            var options = BuildOptions();
            //Get data from database
            StringBuilder query = new StringBuilder();
            query.Append("SELECT users.email ");
            query.Append("FROM Authentication.users ");
            query.Append("INNER JOIN Authentication.tfa ON users.id=tfa.idUsers ");
            query.Append("WHERE tfa.enabled = 1;");
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            MySqlCommand cmd = new MySqlCommand(query.ToString(), conn);
            var queryResult = cmd.ExecuteReader();
            queryResult.Read();
            string email = (string)queryResult["email"];

            // Check enabled
            Tfa tfa = new Tfa(options);
            bool result = tfa.IsTfaEnabled(email);
            result.ShouldBeTrue();
        }

        [Test]
        public void IsTfaNotEnabled()
        {
            var options = BuildOptions();
            //Get data from database
            StringBuilder query = new StringBuilder();
            query.Append("SELECT users.email ");
            query.Append("FROM Authentication.users ");
            query.Append("LEFT JOIN Authentication.tfa ON users.id=tfa.idUsers ");
            query.Append("WHERE tfa.enabled IS NULL;");
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            MySqlCommand cmd = new MySqlCommand(query.ToString(), conn);
            var queryResult = cmd.ExecuteReader();
            queryResult.Read();
            string email = (string)queryResult["email"];

            // Check enabled
            Tfa tfa = new Tfa(options);
            bool result = tfa.IsTfaEnabled(email);
            result.ShouldBeFalse();
        }

        [Test]
        public void VerfiyTotp() {
            var options = BuildOptions();
            // Get and user from the database that is registered and tfa enabled
            StringBuilder query = new StringBuilder();
            query.Append("SELECT ");
            query.Append("users.email, tfa.key ");
            query.Append("FROM Authentication.users ");
            query.Append("LEFT JOIN Authentication.tfa ON users.id=tfa.idUsers ");
            query.Append("WHERE tfa.enabled = 1;");
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            MySqlCommand cmd = new MySqlCommand(query.ToString(), conn);
            var queryResult = cmd.ExecuteReader();
            queryResult.Read();
            string email = (string)queryResult["email"];
            string key = (string)queryResult["key"];
            // Generate totp code
            byte[] bytes =Base32Encoding.ToBytes(key);
            var totp = new Totp(bytes);
            string totpCode = totp.ComputeTotp();

            // Verify totp
            Tfa tfa = new Tfa(options);
            TfaValidateResponse tfaValidateResponse = tfa.Validate(email, totpCode);
            tfaValidateResponse.ShouldSatisfyAllConditions(
                () => tfaValidateResponse.Success.ShouldBeTrue(),
                () => tfaValidateResponse.Authenticated.ShouldBeTrue()
                );
        }

        [Test]
        public void VerfiyTotpInvlaidTotp()
        {
            var options = BuildOptions();
            // Get and user from the database that is registered and tfa enabled
            StringBuilder query = new StringBuilder();
            query.Append("SELECT ");
            query.Append("users.email, tfa.key ");
            query.Append("FROM Authentication.users ");
            query.Append("LEFT JOIN Authentication.tfa ON users.id=tfa.idUsers ");
            query.Append("WHERE tfa.enabled = 1;");
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            MySqlCommand cmd = new MySqlCommand(query.ToString(), conn);
            var queryResult = cmd.ExecuteReader();
            queryResult.Read();
            string email = (string)queryResult["email"];
            string key = (string)queryResult["key"];
            // Generate totp invalid code
            string totpCode = "123456";

            // Verify totp
            Tfa tfa = new Tfa(options);
            TfaValidateResponse tfaValidateResponse = tfa.Validate(email, totpCode);
            tfaValidateResponse.ShouldSatisfyAllConditions(
                () => tfaValidateResponse.Success.ShouldBeFalse(),
                () => tfaValidateResponse.Authenticated.ShouldBeFalse()
                );
        }
    }
        
}
