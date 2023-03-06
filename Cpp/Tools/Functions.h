#ifndef _TOOLS_FUNCTIONS_H
#define _TOOLS_FUNCTIONS_H

#include "NumberWithString.h"
#include "ThreadHelper.h"
#include "TimeHelper.h"

#pragma pack(1)

namespace Tools
{
    class Functions
    {
        public:
            static const int GetBuildDate(char *buffer)
            {
                return sprintf(buffer, "%s", __DATE__);
            }

            static const int GetBuildTime(char *buffer)
            {
                return sprintf(buffer, "%s", __TIME__);
            }

            static const int GetLineNumber(char *buffer)
            {
                return sprintf(buffer, "%d", __LINE__);
            }

            static const int GetFunctionName(char *buffer)
            {
                return sprintf(buffer, "%s", __FUNCTION__);
            }

            static const int GetFilePath(char *buffer)
            {
                return sprintf(buffer, "%s", __FILE__);
            }

            static void CleanStdin()
            {
                int c;

                do {
                    c = getchar();
                } while (c != '\n' && c != EOF);
            }

            static const char *FindValueStr(const char *key, const int& argc, const char *argv[])
            {
                const int keyLen = strlen(key);
                const int _keyWithEqual = keyLen + 1;

                for (int i = 0; i < argc; ++i)
                {
                    const char *arg = argv[i];
                    const int argLen = strlen(arg);

                    if (argLen < _keyWithEqual)
                    {
                        continue;
                    }
                    else if (arg[keyLen] != '=')
                    {
                        continue;
                    }

                    for (int j = 0; j < keyLen; ++j)
                    {
                        if (arg[j] != key[j])
                        {
                            break;
                        }
                        else if (j == keyLen - 1)
                        {
                            return argLen == _keyWithEqual ? NULL : &arg[j + 2];
                        }
                    }
                }

                printf("%s|%s|%s|key=%s|argc=%d|%d|%s|%s|\r\n", TimeHelper::GetHHmmssffffff(), LogLevel::Warn, ThreadHelper::ThreadID.ValueStr, key, argc, __LINE__, __FUNCTION__, __FILE__);

                return NULL;
            }

            static void CopyStrWithLength1(char *destination, const char *source, const int& size)
            {
                memmove(&destination[1], source, size);

                destination[1 + size] = 0;
                destination[0] = size;
            }

            static void CopyStrWithLength2(char *destination, const char *source, const int& size)
            {
                memmove(&destination[2], source, size);

                destination[2 + size] = 0;
                memcpy(&destination[0], (char *)&size, 2);
            }
    };
}

#pragma pack()

#endif
