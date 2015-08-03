using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICtrlDLL
{
    public class CryptoPWD
    {
        public static string Str2Hex(string origin)
        {
            StringBuilder retval = new StringBuilder(128);

            try
            {
                for (int i = 0; i < origin.Length; i += 2)
                {
                    string tmp = origin.Substring(i, 2);

                    int iHexVal = int.Parse(tmp, System.Globalization.NumberStyles.AllowHexSpecifier);

                    retval.Append((char)iHexVal);
                }
            }
            catch { }

            return retval.ToString();
        }

        public static string XorMask(string str, string mask)
        {
            StringBuilder result = new StringBuilder(128);

            int count = 0;

            try
            {
                while (count < str.Length)
                {
                    for (int i = 0; i < mask.Length; i++)
                    {
                        if (count < str.Length)
                        {
                            int Symbol = (int)(str[count]) ^ (int)(mask[i]);

                            result.Append((char)Symbol);

                            count++;
                        }
                    }
                }
            }
            catch { }

            return result.ToString();
        }
    }
}
