#ifndef _TOOLS_FILE_LOGGER_H
#define _TOOLS_FILE_LOGGER_H

#include <thread>

#pragma pack(1)

namespace Tools
{
    class FileLogger
    {
        public:
            static thread_local int MsgLen;
    };

    thread_local int FileLogger::MsgLen = -1;
}

#pragma pack()

#endif
