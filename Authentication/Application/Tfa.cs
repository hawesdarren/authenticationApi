using Authentication.Json.Responses;
using OtpNet;

namespace Authentication.Application
{
    public class Tfa
    {
        public static TfaRegisterResponse CreateNewTotp(string email) {

            var key = KeyGeneration.GenerateRandomKey(20);
            var base32String = Base32Encoding.ToString(key); //Store in database
            var base32Bytes = Base32Encoding.ToBytes(base32String);
            var totp = new Totp(base32Bytes);
            var totpCode = totp.ComputeTotp();
            var uriTotp = new OtpUri(OtpType.Totp, totpCode, email, "hawes.co.nz").ToString();
            // Response 
            TfaRegisterResponse tfaRegisterResponse = new() { Success = false };
            tfaRegisterResponse.keyUri = uriTotp;
            tfaRegisterResponse.Success = true;
            return tfaRegisterResponse;
        }

        public static void Validate(string key, string input) {
            // todo
            var base32Bytes = Base32Encoding.ToBytes(key);
            var totp = new Totp(base32Bytes);
            long timeStepMatched;
            bool verify = totp.VerifyTotp(input, out timeStepMatched, window: null);
        }
    }
}
