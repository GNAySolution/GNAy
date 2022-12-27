#ifndef _TOOLS_FUNCTIONS_H
#define _TOOLS_FUNCTIONS_H

#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <sys/time.h>

namespace Tools
{
    void CleanStdin()
    {
        int c;

        do {
            c = getchar();
        } while (c != '\n' && c != EOF);
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