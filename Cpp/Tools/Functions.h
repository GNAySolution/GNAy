#ifndef _TOOLS_FUNCTIONS_H
#define _TOOLS_FUNCTIONS_H

#include <string.h>
#include <sys/time.h>

namespace Tools
{
    inline const struct tm *GetLocalTime(const time_t& rawTime)
    {
        return localtime(&rawTime);
    }

    inline const struct tm *GetTimeNow()
    {
        return GetLocalTime(time(NULL));
    }

    inline const size_t GetTime(char *buffer, const size_t& size, struct tm *timeM, const char *format)
    {
        memset(buffer, 0, size);
        mktime(timeM);

        return strftime(buffer, size, format, timeM);
    }
}

#endif