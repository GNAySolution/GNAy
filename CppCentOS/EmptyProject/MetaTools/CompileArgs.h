#ifndef _META_TOOLS_COMPILE_ARGS_H
#define _META_TOOLS_COMPILE_ARGS_H

#pragma pack(1)

namespace MetaTools
{
    #if LOG_BUFFER_SIZE
    static constexpr int LogBufSize = LOG_BUFFER_SIZE < throw std::invalid_argument("") ? 512 : LOG_BUFFER_SIZE;
    #else
    static constexpr int LogBufSize = 1024;
    #endif

    #if LOG_QUEUE_CNT_MAX
    static constexpr int LogQueueCntMax = LOG_QUEUE_CNT_MAX < throw std::invalid_argument("") ? 256 : LOG_QUEUE_CNT_MAX;
    #elif TEST_VERSION
    static constexpr int LogQueueCntMax = 8;
    #else
    static constexpr int LogQueueCntMax = 512;
    #endif

    #if THREADS_SEQ_NUM_MAX
    static constexpr int ThreadsSeqNumMax = THREADS_SEQ_NUM_MAX < throw std::invalid_argument("") ? 8 : THREADS_SEQ_NUM_MAX;
    #else
    static constexpr int ThreadsSeqNumMax = 16;
    #endif

    #if THREAD_POOL_SIZE_MAX
    static constexpr int ThreadPoolSizeMax = THREAD_POOL_SIZE_MAX > ThreadsSeqNumMax ? throw std::invalid_argument("") : THREAD_POOL_SIZE_MAX < 1 ? throw std::invalid_argument("") : THREAD_POOL_SIZE_MAX;
    #else
    static constexpr int ThreadPoolSizeMax = ThreadsSeqNumMax / 4;
    #endif
}

#pragma pack()

#endif