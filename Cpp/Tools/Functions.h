#ifndef _TOOLS_FUNCTIONS_H
#define _TOOLS_FUNCTIONS_H

#include <cstring>
#include <iostream>
#include <sstream>
#include <stdio.h>
#include <sys/time.h>
#include <thread>

namespace Tools
{
    void CleanStdin()
    {
        int c;

        do {
            c = getchar();
        } while (c != '\n' && c != EOF);
    }
    
    const uint64_t GetThreadID(const std::thread::id& threadID)
    {
        std::stringstream ss;

        ss << threadID;

        return std::stoull(ss.str());
    }

    const uint64_t GetThreadID()
    {
        return GetThreadID(std::this_thread::get_id());
    }

    const int GetThreadIDAndStr(uint64_t& threadID, char *buffer, int& length)
    {
        if (threadID <= 0)
        {
            threadID = GetThreadID();
        }
        else
        {
            return length;
        }

        return length = sprintf(buffer, "%ld", threadID);
    }

    const struct tm *GetLocalTime(const time_t& rawTime)
    {
        return localtime(&rawTime);
    }

    const struct tm *GetTimeNow()
    {
        return GetLocalTime(time(NULL));
    }

    const size_t GetTime(char *buffer, const size_t& size, struct tm *timeM, const char *format)
    {
        memset(buffer, 0, size);
        mktime(timeM);

        return strftime(buffer, size, format, timeM);
    }
}

#endif
