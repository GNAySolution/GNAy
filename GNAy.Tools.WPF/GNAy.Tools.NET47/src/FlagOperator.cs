using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47
{
    public class FlagOperator
    {
        public static readonly uint[] FlagValue = {
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

        public static Enum ConvertToEnum(in string source, in Type enumType)
        {
            string trim = source.Trim(' ', '.').ToLower();

            foreach (Enum value in Enum.GetValues(enumType))
            {
                if (value.ToString().ToLower().StartsWith(trim) || trim == ((int)(object)value).ToString())
                {
                    return value;
                }
            }

            return (Enum)Enum.Parse(enumType, trim);
        }

        public static T ConvertTo<T>(in string source) where T : Enum
        {
            return (T)ConvertToEnum(source, typeof(T));
        }

        public static bool HasFlag(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (value & FlagValue[position]) > 0;
        }

        public static bool HaveFlags(in byte value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint flags = 0;

            foreach (int pos in positions)
            {
                if (pos < 0 || pos >= sizeof(byte) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(pos)}={pos}");
                }

                flags |= FlagValue[pos];
            }

            return (value & flags) == flags;
        }

        public static byte GetFlag(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (byte)(value & FlagValue[position]);
        }

        public static byte GetFlags(in byte value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint flags = 0;

            foreach (int pos in positions)
            {
                if (pos < 0 || pos >= sizeof(byte) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(pos)}={pos}");
                }

                flags |= FlagValue[pos];
            }

            return (byte)(value & flags);
        }

        public static byte SetFlag(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (byte)(value | FlagValue[position]);
        }

        public static byte SetFlags(in byte value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint flags = 0;

            foreach (int pos in positions)
            {
                if (pos < 0 || pos >= sizeof(byte) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(pos)}={pos}");
                }

                flags |= FlagValue[pos];
            }

            return (byte)(value | flags);
        }

        public static byte RemoveFlag(in byte value, in int position)
        {
            if (position < 0 || position >= sizeof(byte) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            //return HasFlag(value, position) ? (byte)(value - FlagValue[position]) : value;
            return (byte)(value & ~FlagValue[position]);
        }

        public static byte RemoveFlags(in byte value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint flags = 0;

            foreach (int pos in positions)
            {
                if (pos < 0 || pos >= sizeof(byte) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(pos)}={pos}");
                }

                flags |= FlagValue[pos];
            }

            return (byte)(value & ~flags);
        }

        public static bool HasFlag(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return (value & FlagValue[position]) > 0;
        }

        public static bool HaveFlags(in uint value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint flags = 0;

            foreach (int pos in positions)
            {
                if (pos < 0 || pos >= sizeof(uint) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(pos)}={pos}");
                }

                flags |= FlagValue[pos];
            }

            return (value & flags) == flags;
        }

        public static uint GetFlag(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return value & FlagValue[position];
        }

        public static uint GetFlags(in uint value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint flags = 0;

            foreach (int pos in positions)
            {
                if (pos < 0 || pos >= sizeof(uint) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(pos)}={pos}");
                }

                flags |= FlagValue[pos];
            }

            return value & flags;
        }

        public static uint SetFlag(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            return value | FlagValue[position];
        }

        public static uint SetFlags(in uint value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint flags = 0;

            foreach (int pos in positions)
            {
                if (pos < 0 || pos >= sizeof(uint) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(pos)}={pos}");
                }

                flags |= FlagValue[pos];
            }

            return value | flags;
        }

        public static uint RemoveFlag(in uint value, in int position)
        {
            if (position < 0 || position >= sizeof(uint) * 8)
            {
                throw new IndexOutOfRangeException($"{nameof(position)}={position}");
            }

            //return HasFlag(value, position) ? value - FlagValue[position] : value;
            return value & ~FlagValue[position];
        }

        public static uint RemoveFlags(in uint value, params int[] positions)
        {
            if (positions == null || positions.Length <= 0)
            {
                throw new ArgumentException($"{nameof(positions)} == null || {nameof(positions)}.Length <= 0");
            }

            uint flags = 0;

            foreach (int pos in positions)
            {
                if (pos < 0 || pos >= sizeof(uint) * 8)
                {
                    throw new IndexOutOfRangeException($"{nameof(pos)}={pos}");
                }

                flags |= FlagValue[pos];
            }

            return value & ~flags;
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
