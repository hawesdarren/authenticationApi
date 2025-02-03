using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class Tools
    {
        public static string RandonString()
        {
            Random random = new Random();
            int stringlen = random.Next(7, 14);
            int randValue;
            string str = "";
            char letter;
            for (int i = 0; i < stringlen; i++)
            {

                // Generating a random number. 
                randValue = random.Next(0, 26);

                // Generating random character by converting 
                // the random number into character. 
                letter = Convert.ToChar(randValue + 65);

                // Appending the letter to string. 
                str = str + letter;
            }
            return str;

        }
    }
}
