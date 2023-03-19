#ifndef _TOOLS_IL01_FILE_LOGGER_H
#define _TOOLS_IL01_FILE_LOGGER_H

#include <cstdio>
#include <thread>
// #include <utility>

#pragma pack(1)

template<unsigned int Len>
constexpr const char *GetFileName(const char (&fullPath)[Len], unsigned int pos)
{
    return pos == 0 ? fullPath : (fullPath[pos] == '/' || fullPath[pos] == '\\') ? fullPath + pos + 1 : GetFileName(fullPath, pos - 1);
}

template<unsigned int Len>
constexpr const char *GetFileName(const char (&fullPath)[Len])
{
    return GetFileName(fullPath, Len - 1);
}

thread_local char LogPosBuf[512];

#define _LOG_POS_ \
    [] (const int& ln, const char *fn, const char *fi) \
    { \
        snprintf(LogPosBuf, sizeof(LogPosBuf), "%d|%s|%s", ln, fn, fi); \
        return LogPosBuf; \
    } (__LINE__, __FUNCTION__, GetFileName(__FILE__))

namespace Tools
{
    class IL01FileLogger
    {
        public:
            static constexpr int BufSizeMax = sizeof(LogPosBuf) * 2;
            static constexpr int QueueCntMax = 128;

            static thread_local int MsgLen;

            // template <class... Args>
            // static const int Print2(char *buffer, const int& size, const char *format, Args&&... args)
            // {
            //     return snprintf(buffer, size, format, std::forward<Args>(args)...);
            // }

            // template <class... Args>
            // static const int Print2Terminal(const char *format, Args&&... args)
            // {
            //     return printf(format, std::forward<Args>(args)...);
            // }
    };

    constexpr int IL01FileLogger::BufSizeMax;
    constexpr int IL01FileLogger::QueueCntMax;

    thread_local int IL01FileLogger::MsgLen = -1;
}

#pragma pack()

#endif
