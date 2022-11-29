#ifndef _TOOLS_H
#define _TOOLS_H

#include <time.h>

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
}

#endif