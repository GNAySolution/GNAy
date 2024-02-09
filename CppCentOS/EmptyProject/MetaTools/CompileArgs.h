#ifndef GNAY_META_TOOLS_COMPILE_ARGS_H_
#define GNAY_META_TOOLS_COMPILE_ARGS_H_

// #include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/time.h>
#include <unistd.h>

#if __linux
#include <linux/limits.h>
#endif

#if _WIN64
#include <winsock2.h>
#endif

#include <fstream>
#include <functional>
#include <future>
#include <queue>
#include <sstream>
#include <thread>
#include <vector>

#pragma pack(1)

namespace MetaTools
{
class CompileArgs
{
    public:
    #if RUN_TEST
    static constexpr bool RunTest = true;
    #else
    static constexpr bool RunTest = false;
    #endif

    // public:
    // #if GOLD_VERSION
    // static constexpr bool GoldVer = RunTest == false ? true : throw std::invalid_argument("");
    // #else
    // static constexpr bool GoldVer = false;
    // #endif

    public:
    #if LOG_BUFFER_SIZE
    static constexpr int LogBufSize = LOG_BUFFER_SIZE < 512 ? throw std::invalid_argument("") : LOG_BUFFER_SIZE;
    #else
    static constexpr int LogBufSize = 1024;
    #endif

    public:
    #if LOG_QUEUE_CNT_MAX
    static constexpr int LogQueueCntMax = LOG_QUEUE_CNT_MAX < 256 ? throw std::invalid_argument("") : LOG_QUEUE_CNT_MAX;
    #elif RUN_TEST
    static constexpr int LogQueueCntMax = 32; //8;
    #else
    static constexpr int LogQueueCntMax = 512;
    #endif

    public:
    #if LOG_LOCATION_ARRAY_MAX
    static constexpr int LogLocationArrayMax = LOG_LOCATION_ARRAY_MAX < 8 ? throw std::invalid_argument("") : LOG_LOCATION_ARRAY_MAX;
    #else
    static constexpr int LogLocationArrayMax = 8;
    #endif

    public:
    #if THREADS_SEQ_NUM_MAX
    static constexpr int ThreadsSeqNumMax = THREADS_SEQ_NUM_MAX < 8 ? throw std::invalid_argument("") : THREADS_SEQ_NUM_MAX;
    #else
    static constexpr int ThreadsSeqNumMax = 16;
    #endif

    public:
    #if THREAD_POOL_SIZE_MAX
    static constexpr int ThreadPoolSizeMax = THREAD_POOL_SIZE_MAX > ThreadsSeqNumMax ? throw std::invalid_argument("") : THREAD_POOL_SIZE_MAX < 1 ? throw std::invalid_argument("") : THREAD_POOL_SIZE_MAX;
    #else
    static constexpr int ThreadPoolSizeMax = ThreadsSeqNumMax / 4;
    #endif

    public:
    static constexpr const char (&BuildDate)[sizeof(__DATE__)] = __DATE__;
    static constexpr const char (&BuildTime)[sizeof(__TIME__)] = __TIME__;
    static constexpr const char (&BaseFile)[sizeof(__BASE_FILE__)] = __BASE_FILE__;

    public:
    static constexpr bool Is64Bit = sizeof(void *) == 8 ? true : throw std::invalid_argument("");
    static constexpr int CharMin = std::numeric_limits<char>::min() == 0 ? 0 : throw std::invalid_argument(""); //-128 ~ 127 //-fsigned-char
    static constexpr int CharMax = std::numeric_limits<char>::max() == 255 ? 255 : throw std::invalid_argument(""); //0 ~ 255 //-funsigned-char
};

    constexpr int CompileArgs::ThreadsSeqNumMax;

class CA: public CompileArgs
{
};
}

#pragma pack()

#endif