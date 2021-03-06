using System;
using System.Collections.Generic;

namespace Sma5h.Helpers
{
    public static class Base36IncrementHelper
    {
        const int Base = 36;

        public static string ToString(int counter)
        {
            List<char> chars = new List<char>();

            do
            {
                int c = (counter % Base);
                char ascii = (char)(c + (c < 10 ? 48 : 55));
                chars.Add(ascii);
            }
            while ((counter /= Base) != 0);
            chars.Reverse();
            string charCounter = new string(chars.ToArray()).PadLeft(3, '0');
            return charCounter;
        }

        public static int ToInt(string charCounter)
        {
            var chars = charCounter.ToCharArray();

            int counter = 0;

            for (int i = (chars.Length - 1), j = 0; i >= 0; i--, j++)
            {
                int chr = chars[i];

                int value = (chr - (chr > 57 ? 55 : 48)) * (int)Math.Pow(Base, j);

                counter += value;
            }

            return counter;
        }
    }
}
