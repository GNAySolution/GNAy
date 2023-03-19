#ifndef _TOOLS_IL01_DEFINITION_H
#define _TOOLS_IL01_DEFINITION_H

#include <cstdio>
#include <limits>

#pragma pack(1)

namespace Tools
{
    template<typename T, long long N = std::numeric_limits<T>::max()>
    class NumberWidthMax
    {
        public:
            enum
            {
                StringLength = NumberWidthMax<T, N / 10>::StringLength + (std::numeric_limits<T>::min() >= 0 ? 0 : 1),
                ArraySize = NumberWidthMax<T, N / 10>::ArraySize + (std::numeric_limits<T>::min() >= 0 ? 0 : 1),
            };
    };

    template<typename T>
    class NumberWidthMax<T, 0>
    {
        public:
            enum
            {
                StringLength = 1,
                ArraySize = 1 + 1,
            };
    };

    constexpr int BoolMaxStringLength = NumberWidthMax<bool>::StringLength;
    constexpr int BoolMaxArraySize = NumberWidthMax<bool>::ArraySize;
    constexpr int CharMaxStringLength = NumberWidthMax<char>::StringLength;
    constexpr int CharMaxArraySize = NumberWidthMax<char>::ArraySize;
    constexpr int ShortMaxStringLength = NumberWidthMax<short>::StringLength;
    constexpr int ShortMaxArraySize = NumberWidthMax<short>::ArraySize;
    constexpr int IntMaxStringLength = NumberWidthMax<int>::StringLength;
    constexpr int IntMaxArraySize = NumberWidthMax<int>::ArraySize;
    constexpr int LongMaxStringLength = NumberWidthMax<long long>::StringLength;
    constexpr int LongMaxArraySize = NumberWidthMax<long long>::ArraySize;

    struct BoolWithStr
    {
        bool Value;
        char ValueStr[BoolMaxArraySize];
        int ValueStrLen = 0;
    };

    struct ShortWithStr
    {
        short Value;
        char ValueStr[ShortMaxArraySize];
        int ValueStrLen = 0;
    };

    struct IntWithStr
    {
        int Value;
        char ValueStr[IntMaxArraySize];
        int ValueStrLen = 0;
    };

    struct LongWithStr
    {
        long long Value;
        char ValueStr[LongMaxArraySize];
        int ValueStrLen = 0;
    };

    class NumberWithString
    {
        public:
            static const struct BoolWithStr CreateBoolWithStr(const bool& value, const char *format = "%d")
            {
                struct BoolWithStr result;

                result.Value = value;
                result.ValueStrLen = sprintf(result.ValueStr, format, value);

                return result;
            }

            static const struct ShortWithStr CreateShortWithStr(const short& value, const char *format = "%h")
            {
                struct ShortWithStr result;

                result.Value = value;
                result.ValueStrLen = sprintf(result.ValueStr, format, value);

                return result;
            }

            static const struct IntWithStr CreateIntWithStr(const int& value, const char *format = "%d")
            {
                struct IntWithStr result;

                result.Value = value;
                result.ValueStrLen = sprintf(result.ValueStr, format, value);

                return result;
            }

            static const struct LongWithStr CreateLongWithStr(const long long& value, const char *format = "%ld")
            {
                struct LongWithStr result;

                result.Value = value;
                result.ValueStrLen = sprintf(result.ValueStr, format, value);

                return result;
            }
    };

    template<long long N>
    class NumberWidth
    {
        public:
            enum
            {
                Value = NumberWidthMax<long long, N>::StringLength + (N > 0 ? -1 : 0),
            };
    };

    class LogLevel
    {
        public:
            enum Enum
            {
                Trace = 0,
                Debug = 1,
                Info = 2,
                Warn = 3,
                Error = 4,
            };

            static constexpr const char *TraceStr = "TRACE";
            static constexpr const char *DebugStr = "DEBUG";
            static constexpr const char *InfoStr = "INFO";
            static constexpr const char *WarnStr = "WARN";
            static constexpr const char *ErrorStr = "ERROR";

            static constexpr const char *EnumStr[Error + 1] = {TraceStr, DebugStr, InfoStr, WarnStr, ErrorStr};
    };

    constexpr const char *LogLevel::EnumStr[];

    template<typename T>
    struct ItemReturn
    {
        using type = T&&;
    };

    template<typename T>
    const typename ItemReturn<T>::type Convert(T&& arg)
    {
        return static_cast<T&&>(arg);
    }
}

#pragma pack()

#endif
