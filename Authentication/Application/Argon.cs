using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;


namespace Authentication.Application
{
    
    public class Argon
    {
        const int degreeOfParallelism = 1;
        const int iterations = 4;
        const int memorySize = 32 * 1024;
        
        public static byte[] CreateSalt() {
            var buffer = new byte[32];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);
            return buffer;
        }

        public static byte[] CreateHashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = degreeOfParallelism;
            argon2.Iterations = iterations;
            argon2.MemorySize = memorySize;
            //Console.WriteLine("Salt: " + Convert.ToHexString(salt));
            return argon2.GetBytes(32);
        }

        public static bool MatchPassword(string password, string hashedPassword, string hashedSalt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            //Get salt
            var salt = Convert.FromHexString(hashedSalt);
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = degreeOfParallelism;
            argon2.Iterations = iterations;
            argon2.MemorySize = memorySize;
            var passwordBytes = argon2.GetBytes(32);
            var passwordHex = Convert.ToHexString(passwordBytes);

            var result = false;
            if (hashedPassword == passwordHex)
            {
                result = true;
            }

            return result;
        }
    }
}
