#ifndef GNAY_STATIC_TOOLS_FUNCTIONS_H_
#define GNAY_STATIC_TOOLS_FUNCTIONS_H_

#include "../MetaTools/Functions.h"

using namespace MetaTools;

#pragma pack(1)

namespace StaticTools
{
class Functions
{
    protected:
    static thread_local int _lCCnt;
    static thread_local struct FNameArr _locationBuf[CompileArgs::LogLocationArrayMax];

    public:
    static const struct FNameArr *GetLocation(const char *line, const char *function, const char *fileName, const char *fmt = "%s|%s|%s")
    {
        const int mod = ++_lCCnt % CompileArgs::LogLocationArrayMax;

        _locationBuf[mod].Length = snprintf(_locationBuf[mod].Data, NAME_MAX, fmt, line, function, fileName);

        return &_locationBuf[mod];
    }

    public:
    static const bool IsBigEndian()
    {
        const int value = 0x12345678;
        const char *ptr = (char *)&value;

        // return ptr[0] == 0x12 && ptr[1] == 0x34 && ptr[2] == 0x56 && ptr[3] == 0x78;
        return ptr[0] == 0x12;
    }

    public:
    static const int Print2Hexdecimal(const int& size, void *ptr, const char& separator = ' ', const int& startIndex = 0)
    {
        int result = -1;
        const char *data = (char *)ptr;

        if (size > 0 && startIndex >= 0 && startIndex < size)
        {
            if (separator == (int)NULL)
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
    static const int CallCommand2File(const char *command, const char *outputFPath)
    {
        if (outputFPath == NULL)
        {
            return system(command);
        }

        char _cmd[PATH_MAX * 2] = {0};
        snprintf(_cmd, sizeof(_cmd), "%s >%s", command, outputFPath);

        return system(_cmd);
    }

    public:
    static const bool CallCommand2Buffer(const char *command, char *stdoutBuf, const int& size, const std::function<bool(const char *)>& func)
    {
        if (size <= 0)
        {
            return false;
        }

        char _cmd[PATH_MAX * 2] = {0};
        snprintf(_cmd, sizeof(_cmd), "%s 2>&1", command);

        FILE *pipe = popen(_cmd, "r");

        if (pipe)
        {
            while (!feof(pipe))
            {
                if (fgets(stdoutBuf, size, pipe))
                {
                    if (func(stdoutBuf) == true) //LoopResult //const bool Continue = false; //const bool Break = true;
                    {
                        break;
                    }
                }
            }

            pclose(pipe);

            return true;
        }

        return false;
    }

    public:
    static const int CallCommand2Buffer(const char *command, std::string& stdoutBuf)
    {
        char _buf[4096];
        int fgetsCnt = 0;

        CallCommand2Buffer(command, _buf, sizeof(_buf), [&](const char *buf)
        {
            ++fgetsCnt;
            stdoutBuf += buf;

            return false;
        });

        return fgetsCnt;
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
    static const int SetMathCMDPack(char *rawCMDBuf, char *mathBuf, const int& mathBufSize)
    {
        #if (_WIN64)
        return snprintf(mathBuf, mathBufSize, "set /a %s", rawCMDBuf);
        #else
        return snprintf(mathBuf, mathBufSize, "awk 'BEGIN { print (%s) }'", rawCMDBuf);
        #endif
    }

    public:
    static const int SetMathCMDPack(char *rawCMDBuf, const int& rawCMDBufSize)
    {
        char mathBuf[rawCMDBufSize];
        
        SetMathCMDPack(rawCMDBuf, mathBuf, sizeof(mathBuf));

        return snprintf(rawCMDBuf, rawCMDBufSize, "%s", mathBuf);
    }

    public:
    static const int ScanStdin(const char *format, char *buffer, const int& size)
    {
        memset(buffer, 0, size);

        int result = -1;

        do {
            if (result == 0 && buffer[0] == 0)
            {
                CleanStdin();
            }

            result = scanf(format, buffer);

        } while (result == 0 && buffer[0] == 0);

        if (result <= 0)
        {
            memset(buffer, 0, size);
        }

        CleanStdin();

        return result > 0 ? strlen(buffer) : result;
    }

    public:
    static const int ScanStdin(char *buffer, const int& size)
    {
        char format[NAME_MAX];
        snprintf(format, sizeof(format), "%%%d[^\n]", size - 1); //"%31[^\\n]"

        return ScanStdin(format, buffer, size);
    }

    public:
    static const int MathScanStdin(char *stdinBuf, const int& stdinBufSize, char *mathBuf, const int& mathBufSize)
    {
        char format[NAME_MAX];
        snprintf(format, sizeof(format), "%%%d[^\n]", stdinBufSize - 1); //"%31[^\\n]"

        const int scanRes = ScanStdin(format, stdinBuf, stdinBufSize);

        if (scanRes <= 0)
        {
            memset(stdinBuf, 0, stdinBufSize);
            memset(mathBuf, 0, mathBufSize);

            return scanRes == 0 ? -1 : scanRes;
        }

        return SetMathCMDPack(stdinBuf, mathBuf, mathBufSize);
    }

    public:
    static const int MathScanStdin(char *mathBuf, const int& mathBufSize)
    {
        char stdinBuf[mathBufSize];

        return MathScanStdin(stdinBuf, sizeof(stdinBuf), mathBuf, mathBufSize);
    }

    public:
    static char *ASCII2Upper(char *src, const unsigned int& size, const unsigned int& startIndex = 0)
    {
        for (unsigned int i = startIndex; i < size; ++i)
        {
            src[i] = MetaFunc::ASCII2Upper(src[i]);
        }

        return src;
    }

    public:
    static char *ASCII2Lower(char *src, const unsigned int& size, const unsigned int& startIndex = 0)
    {
        for (unsigned int i = startIndex; i < size; ++i)
        {
            src[i] = MetaFunc::ASCII2Lower(src[i]);
        }

        return src;
    }

    public:
    static const bool PadLeft(char *str, int& strLen, const char& padChar = ' ')
    {
        if (str == NULL)
        {
            return false;
        }

        int idx = -1;

        for (int i = 0; i < strLen; ++i)
        {
            if (str[i] != padChar)
            {
                idx = i;
                break;
            }
        }

        if (idx < 0)
        {
            strLen = 0;
            str[0] = 0;

            return true;
        }
        else if (idx == 0)
        {
            return false;
        }

        strLen -= idx;

        memcpy(&str[0], &str[idx], strLen);
        str[strLen] = 0;

        return true;
    }

    public:
    static const bool PadRight(char *str, int& strLen, const char& padChar = ' ')
    {
        if (str == NULL || strLen <= 0)
        {
            return false;
        }

        for (int i = strLen - 1; i >= 0; --i)
        {
            if (str[i] != padChar)
            {
                if (i == strLen - 1)
                {
                    return false;
                }

                strLen = i + 1;
                str[strLen] = 0;

                return true;
            }
        }

        strLen = 0;
        str[0] = 0;

        return true;
    }

    public:
    static const char *FindValueStr(const char *key, const char *keyValue, bool& found, const char& separator = '=')
    {
        found = false;

        const int indexOfSep = MetaTools::Functions::IndexOf(keyValue, separator);

        if (indexOfSep < 0)
        {
            return NULL;
        }

        const int keyLen = strlen(key);

        if (indexOfSep < keyLen)
        {
            return NULL;
        }

        for (int i = 0; i < indexOfSep; ++i)
        {
            if (keyValue[i] == ' ')
            {
                continue;
            }
            else if (keyValue[i] != key[0])
            {
                return NULL;
            }
            else if (indexOfSep - i < keyLen)
            {
                return NULL;
            }

            for (int j = 1; j < keyLen; ++j)
            {
                if (keyValue[j + i] != key[j])
                {
                    return NULL;
                }
            }

            for (int j = keyLen + i; j < indexOfSep; ++j)
            {
                if (keyValue[j] != ' ')
                {
                    return NULL;
                }
            }

            break;
        }

        found = true;

        for (int i = indexOfSep + 1; i < (int)strlen(keyValue); ++i)
        {
            if (keyValue[i] == ' ')
            {
                continue;
            }

            return &keyValue[i];
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

            if (found == true)
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

    public:
    static const int GetFolderPath(char *buffer, const char *filePath)
    {
        const int cnt = MetaFunc::LastIndexOfPathSeparator(filePath);

        if (cnt <= 0)
        {
            return 0;
        }

        memmove(buffer, filePath, cnt);
        buffer[cnt] = 0;

        return cnt;
    }
};

    thread_local int Functions::_lCCnt = 0;
    thread_local struct FNameArr Functions::_locationBuf[CompileArgs::LogLocationArrayMax];

class STFunc: public Functions
{
};
}

    // #define _LC_ [](const char *ln, const char *fn) { std::stringstream ss; ss << ln << "|" << fn; return ss.str(); } (_LINE_STR_, MetaFunc::GetFileName(__FILE__)).c_str()
    #define _LC_ STFunc::GetLocation(_LINE_STR_, __FUNCTION__, __FILE_NAME__)->Data

#pragma pack()

#endif