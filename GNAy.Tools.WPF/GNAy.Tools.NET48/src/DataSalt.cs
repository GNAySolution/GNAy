using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET48
{
    public class DataSalt
    {
        public static byte Reverse(byte value)
        {
            //https://ref.gamer.com.tw/redir.php?url=https%3A%2F%2Fiter01.com%2F536953.html
            value = (byte)(((value & 0xaa) >> 1) | ((value & 0x55) << 1));
            value = (byte)(((value & 0xcc) >> 2) | ((value & 0x33) << 2));

            return (byte)((value >> 4) | (value << 4));
        }

        public static uint Reverse(uint value)
        {
            //https://ref.gamer.com.tw/redir.php?url=https%3A%2F%2Fiter01.com%2F536953.html
            value = ((value & 0xaaaaaaaa) >> 1) | ((value & 0x55555555) << 1);
            value = ((value & 0xcccccccc) >> 2) | ((value & 0x33333333) << 2);
            value = ((value & 0xf0f0f0f0) >> 4) | ((value & 0x0f0f0f0f) << 4);
            value = ((value & 0xff00ff00) >> 8) | ((value & 0x00ff00ff) << 8);

            return (value >> 16) | (value << 16);
        }

        public static byte ExclusiveOR(byte value, params byte[] keys)
        {
            foreach (byte key in keys)
            {
                value ^= key;
            }

            return value;
        }

        public static int ExclusiveOR(int value, params int[] keys)
        {
            foreach (int key in keys)
            {
                value ^= key;
            }

            return value;
        }
    }
}
