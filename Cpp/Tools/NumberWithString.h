#ifndef _TOOLS_NUMBER_WITH_STRING_H
#define _TOOLS_NUMBER_WITH_STRING_H

#include <cstdio>

#include "NumberWidthMax.h"

#pragma pack(1)

namespace Tools
{
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
}

#pragma pack()

#endif
