using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47
{
    public class BitOperator
    {
        public static readonly uint[] BitValue = {
            (uint)Math.Pow(2, 0),
            (uint)Math.Pow(2, 1),
            (uint)Math.Pow(2, 2),
            (uint)Math.Pow(2, 3),
            (uint)Math.Pow(2, 4),
            (uint)Math.Pow(2, 5),
            (uint)Math.Pow(2, 6),
            (uint)Math.Pow(2, 7),
            (uint)Math.Pow(2, 8),
            (uint)Math.Pow(2, 9),
            (uint)Math.Pow(2, 10),
            (uint)Math.Pow(2, 11),
            (uint)Math.Pow(2, 12),
            (uint)Math.Pow(2, 13),
            (uint)Math.Pow(2, 14),
            (uint)Math.Pow(2, 15),
            (uint)Math.Pow(2, 16),
            (uint)Math.Pow(2, 17),
            (uint)Math.Pow(2, 18),
            (uint)Math.Pow(2, 19),
            (uint)Math.Pow(2, 20),
            (uint)Math.Pow(2, 21),
            (uint)Math.Pow(2, 22),
            (uint)Math.Pow(2, 23),
            (uint)Math.Pow(2, 24),
            (uint)Math.Pow(2, 25),
            (uint)Math.Pow(2, 26),
            (uint)Math.Pow(2, 27),
            (uint)Math.Pow(2, 28),
            (uint)Math.Pow(2, 29),
            (uint)Math.Pow(2, 30),
            (uint)Math.Pow(2, 31),
        };

        public static bool CheckBit(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (value & BitValue[position]) > 0;
        }

        public static byte GetBit(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (byte)(value & BitValue[position]);
        }

        public static byte AddBit(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (byte)(value | BitValue[position]);
        }

        public static byte RemoveBit(in byte value, in int position)
        {
            //return CheckBit(value, position) ? (byte)(value - BitValue[position]) : value;
            return (byte)(value & ~BitValue[position]);
        }

        public static bool CheckBit(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (value & BitValue[position]) > 0;
        }

        public static uint GetBit(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return value & BitValue[position];
        }

        public static uint AddBit(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return value | BitValue[position];
        }

        public static uint RemoveBit(in uint value, in int position)
        {
            //return CheckBit(value, position) ? value - BitValue[position] : value;
            return value & ~BitValue[position];
        }
    }
}
