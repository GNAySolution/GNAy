#ifndef _TOOLS_FUNCTIONS_H
#define _TOOLS_FUNCTIONS_H

#include <cstring>
#include <functional>
#include <iostream>
#include <sstream>
#include <stdio.h>
#include <sys/time.h>
#include <thread>

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

            static const int GetFilePath(char *buffer)
            {
                return sprintf(buffer, "%s", __FILE__);
            }

            static const int GetLineNumber(char *buffer)
            {
                return sprintf(buffer, "%d", __LINE__);
            }

            static void CleanStdin()
            {
                int c;

                do {
                    c = getchar();
                } while (c != '\n' && c != EOF);
            }

            static const uint64_t GetThreadID(const std::thread::id& threadID)
            {
                std::stringstream ss;

                ss << threadID;

                return std::stoull(ss.str());
            }

            static const uint64_t GetThreadID()
            {
                return GetThreadID(std::this_thread::get_id());
            }

            static const int GetThreadIDAndStr(uint64_t& threadID, char *buffer, int& length)
            {
                if (threadID <= 0)
                {
                    threadID = GetThreadID();

                    return length = sprintf(buffer, "%ld", threadID);
                }

                return length;
            }

            static void CopyStrWithLength1(char *destination, const char *source, const size_t& size)
            {
                memmove(&destination[1], source, size);

                destination[1 + size] = 0;
                destination[0] = size;
            }

            static void CopyStrWithLength2(char *destination, const char *source, const size_t& size)
            {
                memmove(&destination[2], source, size);

                destination[2 + size] = 0;
                memcpy(&destination[0], (char *)&size, 2);
            }

            static const struct tm *GetLocalTime(const time_t& rawTime)
            {
                return localtime(&rawTime);
            }

            static const size_t GetTime(char *buffer, const size_t& size, struct tm *timeM, const char *format)
            {
                mktime(timeM);

                return strftime(buffer, size, format, timeM);
            }

            static const size_t GetTime(char *buffer, const size_t& size, const struct timeval& timeV, const char *format)
            {
                return GetTime(buffer, size, (struct tm *)GetLocalTime(timeV.tv_sec), format);
            }

            static const struct tm *GetTimeNow()
            {
                return GetLocalTime(time(NULL));
            }

            static const size_t GetTimeNow(char *buffer, const size_t& size)
            {
                memset(buffer, 0, size);

                const char *_buf = asctime(GetTimeNow());
                size_t len = strlen(_buf);

                if (len + 1 >= size)
                {
                    printf("len(%ld) + 1 >= size(%ld)|Tools::GetTimeNow|\r\n", len, size);

                    len = size - 1;
                }

                memcpy(buffer, _buf, len);

                return len;
            }

            static const size_t GetTimeNow(char *buffer, const size_t& size, const char *format)
            {
                return GetTime(buffer, size, (struct tm *)GetLocalTime(time(NULL)), format);
            }

            static const size_t GetTimeWithMicroseconds(char *buffer, const size_t& size, struct tm *timeM, const long& usec, const char *format)
            {
                const size_t length = GetTime(buffer, size, timeM, format);

                // char _buf[7];
                // const int _ulen = snprintf(_buf, sizeof(_buf), "%06lld", usec); //6

                // memcpy(&buffer[length], _buf, sizeof(_buf));

                return length + snprintf(&buffer[length], 6 + 1, "%06ld", usec);
            }

            static const size_t GetTimeWithMicroseconds(char *buffer, const size_t& size, const struct timeval& timeV, const char *format)
            {
                return GetTimeWithMicroseconds(buffer, size, (struct tm *)GetLocalTime(timeV.tv_sec), timeV.tv_usec, format);
            }

            static const struct timeval GetTimeNowWithMicroseconds()
            {
                struct timeval now;
                gettimeofday(&now, NULL);

                return now;
            }

            static const size_t GetTimeNowWithMicroseconds(char *buffer, const size_t& size, const char *format)
            {
                return GetTimeWithMicroseconds(buffer, size, GetTimeNowWithMicroseconds(), format);
            }

            static const struct tm *GetTimeNowWithMicroseconds(long& usec)
            {
                const struct timeval now = GetTimeNowWithMicroseconds();

                usec = now.tv_usec;

                return GetLocalTime(now.tv_sec);
            }

            static const size_t GetDate(char *buffer, const size_t& size, struct tm timeM, const char *format)
            {
                timeM.tm_hour = 0;
                timeM.tm_min = 0;
                timeM.tm_sec = 0;

                return GetTime(buffer, size, &timeM, format);
            }

            static const size_t GetDate(char *buffer, const size_t& size, const struct timeval& timeV, const char *format)
            {
                return GetDate(buffer, size, *(struct tm *)GetLocalTime(timeV.tv_sec), format);
            }

            static const size_t GetDateToday(char *buffer, const size_t& size, const char *format = "%Y/%m/%d")
            {
                return GetDate(buffer, size, *(struct tm *)GetTimeNow(), format);
            }

            static const size_t GetDateYesterday(char *buffer, const size_t& size, const char *format = "%Y/%m/%d")
            {
                struct tm localTime = *(struct tm *)GetTimeNow();
                --localTime.tm_mday;

                return GetDate(buffer, size, localTime, format);
            }

            static const size_t GetDateTomorrow(char *buffer, const size_t& size, const char *format = "%Y/%m/%d")
            {
                struct tm localTime = *(struct tm *)GetTimeNow();
                ++localTime.tm_mday;

                return GetDate(buffer, size, localTime, format);
            }

            static const int64_t GetTimeElapsedInMicroseconds(const struct timeval& startTime)
            {
                const struct timeval endTime = GetTimeNowWithMicroseconds();

                return (endTime.tv_sec * 1000000 + endTime.tv_usec) - (startTime.tv_sec * 1000000 + startTime.tv_usec);
            }

            static const int64_t GetTimeElapsedInMicroseconds(const std::function<void()> function)
            {
                const struct timeval startTime = GetTimeNowWithMicroseconds();

                function();

                return GetTimeElapsedInMicroseconds(startTime);
            }
    };
}

#pragma pack()

#endif
