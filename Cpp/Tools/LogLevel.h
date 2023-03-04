#ifndef _TOOLS_LOG_LEVEL_H
#define _TOOLS_LOG_LEVEL_H

#pragma pack(1)

namespace Tools
{
    class LogLevel
    {
        public:
            static const char *Trace;
            static const char *Debug;
            static const char *Info;
            static const char *Warn;
            static const char *Error;
    };

    const char *LogLevel::Trace = "TRACE";
    const char *LogLevel::Debug = "DEBUG";
    const char *LogLevel::Info = "INFO";
    const char *LogLevel::Warn = "WARN";
    const char *LogLevel::Error = "ERROR";
}

#pragma pack()

#endif
