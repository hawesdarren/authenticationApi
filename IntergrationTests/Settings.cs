using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntergrationTests
{
    public class Settings
    {
        public static string GetSUT()
        {
            var sutEnv = Environment.GetEnvironmentVariable("SUT");
            if (string.IsNullOrEmpty(sutEnv))
            {
                return "https://localhost:443";
            }
            return sutEnv;
        }
    }
}
