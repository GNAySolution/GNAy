using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47
{
    public class PackBCDConverter
    {
        public char ToPackBCD(in byte source)
        {
            byte b1 = (byte)(source / 10); //高四位
            byte b2 = (byte)(source % 10); //低四位

            return (char)((b1 << 4) | b2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source1">高四位</param>
        /// <param name="source2">低四位</param>
        /// <returns></returns>
        public static char ToPackBCD(in char source1, in char source2)
        {
            byte b1 = (byte)(source1 - '0'); //高四位
            byte b2 = (byte)(source2 - '0'); //低四位

            return (char)((b1 << 4) | b2);
        }

        public static char[] ToPackBCD(in string source)
        {
            List<char> result = new List<char>();

            if (string.IsNullOrEmpty(source))
            {
                return result.ToArray();
            }

            bool isOdd = source.Length % 2 == 1;

            if (isOdd)
            {
                result.Add(ToPackBCD('0', source[0]));

                if (source.Length == 1)
                {
                    return result.ToArray();
                }
            }

            for (int i = isOdd ? 1 : 0; i < source.Length; i += 2)
            {
                result.Add(ToPackBCD(source[i], source[i + 1]));
            }

            return result.ToArray();
        }

        public static int ToInt(in char source)
        {
            return (source >> 4) * 10 + (source & 0x0F);
        }

        public static int ToInt(in char[] source, in int count = sizeof(int))
        {
            if (source == null || source.Length <= 0 || count <= 0 || count > sizeof(int))
            {
                return -1;
            }

            int cnt = source.Length > count ? count : source.Length;

            int result = ToInt(source[0]);

            for (int i = 1; i < cnt; ++i)
            {
                result = result * 100 + ToInt(source[i]);
            }

            return result;
        }

        public static long ToLong(in char[] source, in int count = sizeof(long))
        {
            if (source == null || source.Length <= 0 || count <= 0 || count > sizeof(long))
            {
                return -1;
            }

            int cnt = source.Length > count ? count : source.Length;

            long result = ToInt(source[0]);

            for (int i = 1; i < cnt; ++i)
            {
                result = result * 100 + ToInt(source[i]);
            }

            return result;
        }
    }
}
