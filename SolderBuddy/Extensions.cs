using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolderBuddy
{
    public static class Extensions
    {
        public static string SubstringToNonDigit(this string s, int startIndex)
        {
            StringBuilder sb = new StringBuilder();
            int index = startIndex;

            while ( Char.IsDigit(s[index]))
            {
                sb.Append(s[index]);
                ++index;
            }

            return sb.ToString();
        }


        
    }
}
