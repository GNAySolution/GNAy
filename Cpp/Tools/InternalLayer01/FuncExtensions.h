#ifndef _TOOLS_IL01_FUNC_EXTENSIONS_H
#define _TOOLS_IL01_FUNC_EXTENSIONS_H

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <functional>
#include <sys/time.h>
#include <thread>

#pragma pack(1)

template<unsigned int Len>
constexpr const char *GetFileName(const char (&fullPath)[Len], const unsigned int pos)
{
    return pos <= 0 ? fullPath : (fullPath[pos] == '/' || fullPath[pos] == '\\') ? fullPath + pos + 1 : GetFileName(fullPath, pos - 1);
}

template<unsigned int Len>
constexpr const char *GetFileName(const char (&fullPath)[Len])
{
    return GetFileName(fullPath, Len - 1);
}

#define _FILE_NAME_ GetFileName(__FILE__)

constexpr int SizeOfTimeval = sizeof(struct timeval);

namespace Tools
{
    class IL01FuncExtensions
    {
        protected:
            static thread_local char _timeLogBuf[32];

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

            static const char *FindValueStr(const char *key, const int& argc, const char *argv[], bool& keyIsNotFound, const char& separator = '=')
            {
                const int keyLen = strlen(key);
                const int _keyWithEqual = keyLen + 1;

                keyIsNotFound = false;

                for (int i = 0; i < argc; ++i)
                {
                    const char *arg = argv[i];
                    const int argLen = strlen(arg);

                    if (argLen < _keyWithEqual)
                    {
                        continue;
                    }
                    else if (arg[keyLen] != separator)
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

                keyIsNotFound = true;

                return NULL;
            }

            static const char *FindValueStr(const char *key, const int& argc, const char *argv[], const char& separator = '=')
            {
                bool keyIsNotFound = false;

                return FindValueStr(key, argc, argv, keyIsNotFound, separator);
            }
            
            static const struct tm *GetLocalTime(const time_t& rawTime)
            {
                return localtime(&rawTime);
            }

            static const int GetTime(char *buffer, const int& size, struct tm *timeM, const char *format)
            {
                mktime(timeM);

                return strftime(buffer, size, format, timeM);
            }

            static const int GetTime(char *buffer, const int& size, const struct timeval& timeV, const char *format)
            {
                return GetTime(buffer, size, (struct tm *)GetLocalTime(timeV.tv_sec), format);
            }

            static const struct tm *GetTimeNow()
            {
                return GetLocalTime(time(NULL));
            }

            static const int GetTimeNow(char *buffer, const int& size, int& bufSizeIsNotEnough)
            {
                memset(buffer, 0, size);

                bufSizeIsNotEnough = 0;

                const char *_buf = asctime(GetTimeNow());
                int len = strlen(_buf);

                if (len + 1 >= size)
                {
                    bufSizeIsNotEnough = len + 1;

                    len = size - 1;
                }

                memcpy(buffer, _buf, len);

                return len;
            }

            static const int GetTimeNow(char *buffer, const int& size)
            {
                int bufSizeIsNotEnough = 0;

                return GetTimeNow(buffer, size, bufSizeIsNotEnough);
            }

            static const int GetTimeNow(char *buffer, const int& size, const char *format)
            {
                return GetTime(buffer, size, (struct tm *)GetLocalTime(time(NULL)), format);
            }

            static const int GetTimeWithMicroseconds(char *buffer, const int& size, struct tm *timeM, const long& usec, const char *format)
            {
                const int length = GetTime(buffer, size, timeM, format);
                const int result = length + snprintf(&buffer[length], 6 + 1, "%06ld", usec);

                return result;
            }

            static const int GetTimeWithMicroseconds(char *buffer, const int& size, const struct timeval& timeV, const char *format)
            {
                return GetTimeWithMicroseconds(buffer, size, (struct tm *)GetLocalTime(timeV.tv_sec), timeV.tv_usec, format);
            }

            static const struct timeval GetTimeNowWithMicroseconds()
            {
                struct timeval now;
                gettimeofday(&now, NULL);

                return now;
            }

            static const int GetTimeNowWithMicroseconds(char *buffer, const int& size, const char *format)
            {
                return GetTimeWithMicroseconds(buffer, size, GetTimeNowWithMicroseconds(), format);
            }

            static const struct tm *GetTimeNowWithMicroseconds(long& usec)
            {
                const struct timeval now = GetTimeNowWithMicroseconds();

                usec = now.tv_usec;

                return GetLocalTime(now.tv_sec);
            }

            static const char *GetHHmmssffffff()
            {
                GetTimeNowWithMicroseconds(_timeLogBuf, sizeof(_timeLogBuf), "%H:%M:%S.");

                return _timeLogBuf;
            }

            static const int GetDate(char *buffer, const int& size, struct tm timeM, const char *format)
            {
                timeM.tm_hour = 0;
                timeM.tm_min = 0;
                timeM.tm_sec = 0;

                return GetTime(buffer, size, &timeM, format);
            }

            static const int GetDate(char *buffer, const int& size, const struct timeval& timeV, const char *format)
            {
                return GetDate(buffer, size, *(struct tm *)GetLocalTime(timeV.tv_sec), format);
            }

            static const int GetDateToday(char *buffer, const int& size, const char *format = "%Y/%m/%d")
            {
                return GetDate(buffer, size, *(struct tm *)GetTimeNow(), format);
            }

            static const int GetDateYesterday(char *buffer, const int& size, const char *format = "%Y/%m/%d")
            {
                struct tm localTime = *(struct tm *)GetTimeNow();
                --localTime.tm_mday;

                return GetDate(buffer, size, localTime, format);
            }

            static const int GetDateTomorrow(char *buffer, const int& size, const char *format = "%Y/%m/%d")
            {
                struct tm localTime = *(struct tm *)GetTimeNow();
                ++localTime.tm_mday;

                return GetDate(buffer, size, localTime, format);
            }

            static const int64_t GetTimeElapsedInMicroseconds(const struct timeval& startTime)
            {
                const struct timeval endTime = GetTimeNowWithMicroseconds();

                // return (endTime.tv_sec * 1000000 + endTime.tv_usec) - (startTime.tv_sec * 1000000 + startTime.tv_usec);
                return (endTime.tv_sec - startTime.tv_sec) * 1000000 + endTime.tv_usec - startTime.tv_usec;
            }

            static const int64_t GetTimeElapsedInMicroseconds(const std::function<void()> func)
            {
                const struct timeval startTime = GetTimeNowWithMicroseconds();

                func();

                return GetTimeElapsedInMicroseconds(startTime);
            }
    };

    thread_local char IL01FuncExtensions::_timeLogBuf[sizeof(IL01FuncExtensions::_timeLogBuf)];
}

#pragma pack()

#endif
