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

        public static bool CheckBits(in byte value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint bits = 0;

            foreach (int position in positions)
            {
                if (position < 0 || position >= sizeof(byte) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(position)}={position}");
                }

                bits |= BitValue[position];
            }

            return (value & bits) == bits;
        }

        public static byte GetBit(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (byte)(value & BitValue[position]);
        }

        public static byte GetBits(in byte value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint bits = 0;

            foreach (int position in positions)
            {
                if (position < 0 || position >= sizeof(byte) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(position)}={position}");
                }

                bits |= BitValue[position];
            }

            return (byte)(value & bits);
        }

        public static byte AddBit(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (byte)(value | BitValue[position]);
        }

        public static byte AddBits(in byte value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint bits = 0;

            foreach (int position in positions)
            {
                if (position < 0 || position >= sizeof(byte) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(position)}={position}");
                }

                bits |= BitValue[position];
            }

            return (byte)(value | bits);
        }

        public static byte RemoveBit(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            //return CheckBit(value, position) ? (byte)(value - BitValue[position]) : value;
            return (byte)(value & ~BitValue[position]);
        }

        public static byte RemoveBits(in byte value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint bits = 0;

            foreach (int position in positions)
            {
                if (position < 0 || position >= sizeof(byte) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(position)}={position}");
                }

                bits |= BitValue[position];
            }

            return (byte)(value & ~bits);
        }

        public static bool CheckBit(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (value & BitValue[position]) > 0;
        }

        public static bool CheckBits(in uint value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint bits = 0;

            foreach (int position in positions)
            {
                if (position < 0 || position >= sizeof(uint) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(position)}={position}");
                }

                bits |= BitValue[position];
            }

            return (value & bits) == bits;
        }

        public static uint GetBit(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return value & BitValue[position];
        }

        public static uint GetBits(in uint value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint bits = 0;

            foreach (int position in positions)
            {
                if (position < 0 || position >= sizeof(uint) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(position)}={position}");
                }

                bits |= BitValue[position];
            }

            return value & bits;
        }

        public static uint AddBit(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return value | BitValue[position];
        }

        public static uint AddBits(in uint value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint bits = 0;

            foreach (int position in positions)
            {
                if (position < 0 || position >= sizeof(uint) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(position)}={position}");
                }

                bits |= BitValue[position];
            }

            return value | bits;
        }

        public static uint RemoveBit(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            //return CheckBit(value, position) ? value - BitValue[position] : value;
            return value & ~BitValue[position];
        }

        public static uint RemoveBits(in uint value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint bits = 0;

            foreach (int position in positions)
            {
                if (position < 0 || position >= sizeof(uint) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(position)}={position}");
                }

                bits |= BitValue[position];
            }

            return value & ~bits;
        }

        public static byte Reverse(byte value)
        {
            value = (byte)(((value & 0xaa) >> 1) | ((value & 0x55) << 1));
            value = (byte)(((value & 0xcc) >> 2) | ((value & 0x33) << 2));

            return (byte)((value >> 4) | (value << 4));
        }

        public static uint Reverse(uint value)
        {
            value = ((value & 0xaaaaaaaa) >> 1) | ((value & 0x55555555) << 1);
            value = ((value & 0xcccccccc) >> 2) | ((value & 0x33333333) << 2);
            value = ((value & 0xf0f0f0f0) >> 4) | ((value & 0x0f0f0f0f) << 4);
            value = ((value & 0xff00ff00) >> 8) | ((value & 0x00ff00ff) << 8);

            return (value >> 16) | (value << 16);
        }
    }
}
