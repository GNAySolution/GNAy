#ifndef _TOOLS_IL01_DEFINITION_H
#define _TOOLS_IL01_DEFINITION_H

#include <cstdio>
#include <limits>

#pragma pack(1)

namespace Tools
{
    constexpr size_t StrLength(const char *str)
    {
        return *str ? 1 + StrLength(str + 1) : 0;
    }

    constexpr unsigned int GetTargetPos(const char *str, const char& target)
    {
        return *str == 0 ? -1 : *str != target ? 1 + GetTargetPos(str + 1, target) : 0;
    }

    constexpr long long Absolute(const long long& x)
    {
        return x < 0 ? -x : x;
    }

    template<typename T, long long N = std::numeric_limits<T>::max()>
    class NumericWidthMax
    {
        public:
            enum
            {
                StrLength = NumericWidthMax<T, N / 10>::StrLength + (std::numeric_limits<T>::min() >= 0 ? 0 : 1),
                ArraySize = NumericWidthMax<T, N / 10>::ArraySize + (std::numeric_limits<T>::min() >= 0 ? 0 : 1),
            };
    };

    template<typename T>
    class NumericWidthMax<T, 0>
    {
        public:
            enum
            {
                StrLength = 1,
                ArraySize = 1 + 1,
            };
    };

    constexpr int BoolMaxStrLength = NumericWidthMax<bool>::StrLength;
    constexpr int BoolMaxArraySize = NumericWidthMax<bool>::ArraySize;
    constexpr int CharMaxStrLength = NumericWidthMax<char>::StrLength;
    constexpr int CharMaxArraySize = NumericWidthMax<char>::ArraySize;
    constexpr int ShortMaxStrLength = NumericWidthMax<short>::StrLength;
    constexpr int ShortMaxArraySize = NumericWidthMax<short>::ArraySize;
    constexpr int IntMaxStrLength = NumericWidthMax<int>::StrLength;
    constexpr int IntMaxArraySize = NumericWidthMax<int>::ArraySize;
    constexpr int LongMaxStrLength = NumericWidthMax<long long>::StrLength;
    constexpr int LongMaxArraySize = NumericWidthMax<long long>::ArraySize;

    template<long long N>
    class NumericWidth
    {
        public:
            enum
            {
                Value = NumericWidthMax<long long, N>::StrLength + (N > 0 ? -1 : 0),
            };
    };

    constexpr const char *GetFileName(const char *fullPath, const unsigned int& pos)
    {
        return pos <= 0 ? fullPath : (fullPath[pos] == '/' || fullPath[pos] == '\\') ? fullPath + pos + 1 : GetFileName(fullPath, pos - 1);
    }

    constexpr const char *GetFileName(const char *fullPath)
    {
        return GetFileName(fullPath, StrLength(fullPath) - 1);
    }

    #define _FILE_NAME_ GetFileName(__FILE__)

    template<char... Args>
    struct MetaString
    {
        int DataSize = (sizeof... (Args));
        // int DataLength = (sizeof... (Args)) - 1;
        const char Data[sizeof... (Args)] = {Args...};
    };

    template<int Width, long long N, char... Args>
    struct ASCIINumericBuilder
    {
        typedef typename ASCIINumericBuilder<Width - 1, N / 10, Absolute(N) % 10 + '0', Args...>::Type Type;
    };

    template<long long N, char... Args>
    struct ASCIINumericBuilder<2, N, Args...>
    {
        typedef MetaString<N < 0 ? '-' : N / 10 + '0', Absolute(N) % 10 + '0', Args...> Type;
    };

    template<long long N, char... Args>
    struct ASCIINumericBuilder<1, N, Args...>
    {
        typedef MetaString<N + '0', Args...> Type;
    };

    template<long long N>
    class ASCIINumeric
    {
        private:
            typedef typename ASCIINumericBuilder<NumericWidth<N>::Value, N, '\0'>::Type t;

            static constexpr t metaStr {};

        public:
            static constexpr int GetDataSize()
            {
                return metaStr.DataSize;
            }

            static constexpr const char *GetValue()
            {
                return metaStr.Data;
            }
    };

    template<long long N>
    constexpr typename ASCIINumeric<N>::t ASCIINumeric<N>::metaStr;

    constexpr long long Hash2Long(const char *str, const unsigned int& pos)
    {
        return pos <= 0 ? ((str[pos] + StrLength(str)) % __LINE__ % 2 ? 12345 : 54321) : str[pos] / 2 * (pos + 1) + Hash2Long(str, pos - 1);
    }

    constexpr long long Hash2Long(const char *str)
    {
        return Hash2Long(str, StrLength(str) - 1);
    }

    #define _LOG_POS_ ASCIINumeric<Hash2Long(__FILE__) * 1000000 + __LINE__>::GetValue()

    template<int Width, unsigned int N, char... Args>
    struct ASCIINumericFormatter
    {
        typedef typename ASCIINumericFormatter<Width - 1, N / 10, N % 10 + '0', Args...>::Type Type;
    };

    template<unsigned int N, char... Args>
    struct ASCIINumericFormatter<1, N, Args...>
    {
        typedef MetaString<'%', N + '0', Args...> Type;
    };

    template<size_t N>
    class ASCIINumericConverter
    {
        private:
            typedef typename ASCIINumericFormatter<NumericWidth<(int)(N - 1)>::Value, (unsigned int)(N - 1), 's', '\0'>::Type t;

            static constexpr t metaStr {};

        public:
            static constexpr const char *GetFormat()
            {
                return metaStr.Data;
            }
    };

    template<size_t N>
    constexpr typename ASCIINumericConverter<N>::t ASCIINumericConverter<N>::metaStr;

    template <typename T, unsigned int N>
    struct ArrayRecord
    {
        static constexpr int DataSize = N;

        T Data[N];
    };

    template <unsigned int N>
    struct CharArrayRecord: ArrayRecord<char, N>
    {
        static constexpr const char *ASCIINumericFormat = ASCIINumericConverter<N>::GetFormat();

        char Data[N] = {0};
    };

    template <unsigned int N>
    constexpr const char *CharArrayRecord<N>::ASCIINumericFormat;

    struct BoolWithStr
    {
        bool Value;
        char ValueStr[BoolMaxArraySize];
        int ValueStrLen = 0;

        BoolWithStr() {}

        BoolWithStr(const bool& value, const char *format = "%d")
        {
            Value = value;
            ValueStrLen = sprintf(ValueStr, format, value);
        }
    };

    struct ShortWithStr
    {
        short Value;
        char ValueStr[ShortMaxArraySize];
        int ValueStrLen = 0;

        ShortWithStr() {}

        ShortWithStr(const short& value, const char *format = "%h")
        {
            Value = value;
            ValueStrLen = sprintf(ValueStr, format, value);
        }
    };

    struct IntWithStr
    {
        int Value;
        char ValueStr[IntMaxArraySize];
        int ValueStrLen = 0;

        IntWithStr() {}

        IntWithStr(const int& value, const char *format = "%d")
        {
            Value = value;
            ValueStrLen = sprintf(ValueStr, format, value);
        }
    };

    struct LongWithStr
    {
        long long Value;
        char ValueStr[LongMaxArraySize];
        int ValueStrLen = 0;

        LongWithStr() {}

        LongWithStr(const long long& value, const char *format = "%ld")
        {
            Value = value;
            ValueStrLen = sprintf(ValueStr, format, value);
        }
    };
}

#pragma pack()

#endif
