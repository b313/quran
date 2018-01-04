using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Engine
{
    public static class Utility
    {
        public static string GetArabicNo(int i)
        {
            string result = "";

            int sadgan = i / 100;
            int dahgan = i / 10 - sadgan * 10;
            int yekan = i - (sadgan * 100 + dahgan * 10);

            if (sadgan > 0)
                result += GetChar(sadgan);

            if (dahgan > 0 || (sadgan > 0))
                result += GetChar(dahgan);

            result += GetChar(yekan);

            return result;
        }

        private static char GetChar(int i)
        {
            char ch = '٠';

            return (char)(ch + i);
        }


        internal static string GetSuraNo(int suraNo)
        {
            return GetArabicNo(suraNo).PadLeft(3, ' ');
            //if (suraNo >= 100)
            //    return GetArabicNo(suraNo);

            //string zero = GetArabicNo(0);
            
            //if (suraNo >= 10)
            //    return zero + GetArabicNo(suraNo);

            //return zero + zero + GetArabicNo(suraNo);

        }
    }
}
