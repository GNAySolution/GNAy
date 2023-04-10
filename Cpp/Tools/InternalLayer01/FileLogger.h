#ifndef _TOOLS_IL01_FILE_LOGGER_H
#define _TOOLS_IL01_FILE_LOGGER_H

#include <cstdio>
#include <thread>
// #include <utility>

#pragma pack(1)

namespace Tools
{
    #define STR(N) #N
    #define XSTR(N) STR(N)

    #define _LINE_STR_ XSTR(__LINE__)
    // #define _LOG_POS_ _LINE_STR_ "|" __FILE__

    class IL01FileLogger
    {
        public:
            static constexpr int BufSizeMax = 1024;
            static constexpr int QueueCntMax = 64 * 2 * 2;

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
