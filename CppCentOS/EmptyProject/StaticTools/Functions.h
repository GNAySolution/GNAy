#ifndef _STATIC_TOOLS_FUNCTIONS_H
#define _STATIC_TOOLS_FUNCTIONS_H

#include "../MetaTools/BaseInclude.h"
#include "../MetaTools/Functions.h"

#pragma pack(1)

namespace StaticTools
{
class Functions
{
    public:
    static constexpr const char *FilePath = __FILE__;

    public:
    static constexpr const char *FileName = MetaTools::_FILE_NAME_;

    public:
    static void ChangeConst(const int& source, const int& addValue)
    {
        int *ptr = (int *)&source;
        *ptr = source + addValue;
    }

    public:
    static const int Print2Hexdecimal(const int& size, const char *data, const char& separator = 0, const int& startIndex = 0)
    {
        int result = -1;

        if (size > 0 && startIndex >= 0 && startIndex < size)
        {
            if (separator == 0)
            {
                for (int i = startIndex; i < size; ++i)
                {
                    result = printf("%02hhX", data[i]);
                }
            }
            else
            {
                for (int i = startIndex; i < size - 1; ++i)
                {
                    printf("%02hhX%c", data[i], separator);
                }

                result = printf("%02hhX", data[size - 1]);
            }
        }

        return result;
    }

    public:
    static void CleanStdin()
    {
        int c;

        do {
            c = getchar();
        } while (c != '\n' && c != EOF);
    }

    public:
    static const char *FindValueStr(const char *key, const char *keyValue, bool& found, const char& separator = '=')
    {
        found = false;

        const int keyLen = strlen(key);
        const int keyLenPlus = keyLen + sizeof(separator);

        const int argLen = strlen(keyValue);

        if (argLen < keyLenPlus)
        {
            return NULL;
        }
        else if (keyValue[keyLen] != separator)
        {
            return NULL;
        }

        for (int i = 0; i < keyLen; ++i)
        {
            if (keyValue[i] != key[i])
            {
                break;
            }
            else if (i == keyLen - 1)
            {
                found = true;

                return argLen == keyLenPlus ? NULL : &keyValue[i + sizeof(separator) + 1];
            }
        }

        return NULL;
    }

    public:
    static const char *FindValueStr(const char *key, const int& argc, const char *argv[], bool& found, const char& separator = '=')
    {
        found = false;

        for (int i = 0; i < argc; ++i)
        {
            const char *valueStr = FindValueStr(key, argv[i], found, separator);

            if (found)
            {
                return valueStr;
            }
        }

        return NULL;
    }

    public:
    static const char *FindValueStr(const char *key, const int& argc, const char *argv[], const char& separator = '=')
    {
        bool found = false;

        return FindValueStr(key, argc, argv, found, separator);
    }
};

class STFunc: public Functions
{
};
}

#pragma pack()

#endif