#if NETCOREAPP
using System;

namespace Benchmarks.Libs
{
    public static class ByteUtil
    {
        // An idea from vcsjones, via https://gist.github.com/vcsjones/35823b7131dce67c0e6ed9145aaad1a4
        public static bool TryParse(ReadOnlySpan<char> str, out byte result)
        {
            result = 0;

            int b = 0, ch, i = 0;
            switch (str.Length)
            {
                case 3:
                    goto Three;
                case 2:
                    goto Two;
                case 1:
                    goto One;
                default:
                    return false;
            }

        Three:
            ch = str[i] - '0';
            if (ch > 9)
                return false;
            b = 100 * ch;
            i++;

        Two:
            ch = str[i] - '0';
            if (ch > 9)
                return false;
            b += 10 * ch;
            i++;

        One:
            ch = str[i] - '0';
            if (ch > 9)
                return false;
            b += ch;

            if (b > byte.MaxValue)
                return false;

            result = (byte)b;
            return true;
        }
    }
}
#endif