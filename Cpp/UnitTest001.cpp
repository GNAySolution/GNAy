#include <stdlib.h>
#include <unistd.h>
#include <vector>

#include "Tools/ArrayWithLength.h"
#include "Tools/Functions.h"

const size_t SizeOfTimeval = sizeof(struct timeval);

thread_local size_t StrLenTemp = -1;

const int LogBufCntMax = 64;
const int LogBufSize = 1024;

char LogBuf[LogBufCntMax][LogBufSize];
std::vector<const char *> LogList;

const int MyPID = getpid();
char MyPIDStr[11];
const int MyPIDStrLen = sprintf(MyPIDStr, "%d", MyPID);

thread_local uint64_t ThreadID = 0;
thread_local char ThreadIDStr[21];
thread_local int ThreadIDStrLen = -1;

void PrintMessagesA(const std::vector<const char *>& msgs)
{
    int idx = -1;

    for (const char *msg : msgs)
    {
        printf("idx=%d|len=%ld|msg=%s|\r\n", ++idx, strlen(msg), msg);
    }
}

void PrintMessagesB(const std::vector<const char *>& msgs)
{
    const int timeBufSize = 32;

    int idx = -1;

    for (const char *msg : msgs)
    {
        const struct timeval *rawTime = (const struct timeval *)&msg[0];

        char timeBuf[timeBufSize];
        Tools::GetTimeWithMicroseconds(timeBuf, timeBufSize, *rawTime, "%H:%M:%S.");

        printf("idx=%d|time=%s|len=%ld|msg=%s|\r\n", ++idx, timeBuf, strlen(&msg[SizeOfTimeval]), &msg[SizeOfTimeval]);
    }
}

void TestArgumentsA(const int& argc, const char *argv[])
{
    for (int i = 0; i < argc; ++i)
    {
        char idxStr[11];
        size_t pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[i][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[i][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[i][pos], ThreadIDStr, ThreadIDStrLen);
        pos += ThreadIDStrLen;
        StrLenTemp = sprintf(idxStr, "|argv[%d]=", i);
        memcpy(&LogBuf[i][pos], idxStr, StrLenTemp);
        pos += StrLenTemp;
        memcpy(&LogBuf[i][pos], argv[i], StrLenTemp = strlen(argv[i]));
        pos += StrLenTemp;
        LogBuf[i][pos] = 0;

        LogList.push_back(LogBuf[i]);
    }

    {
        size_t pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[argc][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[argc][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[argc][pos], ThreadIDStr, ThreadIDStrLen);
        pos += ThreadIDStrLen;
        LogBuf[argc][pos] = 0;

        LogList.push_back(LogBuf[argc]);
    }
}

void TestArgumentsB(const int& argc, const char *argv[])
{
    for (int i = 0; i < argc; ++i)
    {
        char idxStr[11];
        size_t pos = 0;

        const struct timeval timeV = Tools::GetTimeNowWithMicroseconds();
        memcpy(&LogBuf[i][pos], &timeV, SizeOfTimeval);
        pos += SizeOfTimeval;

        memcpy(&LogBuf[i][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[i][pos], ThreadIDStr, ThreadIDStrLen);
        pos += ThreadIDStrLen;
        StrLenTemp = sprintf(idxStr, "|argv[%d]=", i);
        memcpy(&LogBuf[i][pos], idxStr, StrLenTemp);
        pos += StrLenTemp;
        memcpy(&LogBuf[i][pos], argv[i], StrLenTemp = strlen(argv[i]));
        pos += StrLenTemp;
        LogBuf[i][pos] = 0;

        LogList.push_back(LogBuf[i]);
    }

    {
        size_t pos = 0;

        const struct timeval timeV = Tools::GetTimeNowWithMicroseconds();
        memcpy(&LogBuf[argc][pos], &timeV, SizeOfTimeval);
        pos += SizeOfTimeval;

        memcpy(&LogBuf[argc][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[argc][pos], ThreadIDStr, ThreadIDStrLen);
        pos += ThreadIDStrLen;
        LogBuf[argc][pos] = 0;

        LogList.push_back(LogBuf[argc]);
    }
}

void TestSizeOfA(const int& index, const char *typeMsg, const size_t& size)
{
    char sizeStr[21];
    size_t pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[index][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[index][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[index][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[index][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[index][pos], typeMsg, StrLenTemp = strlen(typeMsg));
    pos += StrLenTemp;
    StrLenTemp = sprintf(sizeStr, "%ld", size);
    memcpy(&LogBuf[index][pos], sizeStr, StrLenTemp);
    pos += StrLenTemp;
    LogBuf[index][pos] = 0;

    LogList.push_back(LogBuf[index]);
}

void TestSizeOfB(const int& index, const char *typeMsg, const size_t& size)
{
    char sizeStr[21];
    size_t pos = 0;

    const struct timeval timeV = Tools::GetTimeNowWithMicroseconds();
    memcpy(&LogBuf[index][pos], &timeV, SizeOfTimeval);
    pos += SizeOfTimeval;

    memcpy(&LogBuf[index][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[index][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[index][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[index][pos], typeMsg, StrLenTemp = strlen(typeMsg));
    pos += StrLenTemp;
    StrLenTemp = sprintf(sizeStr, "%ld", size);
    memcpy(&LogBuf[index][pos], sizeStr, StrLenTemp);
    pos += StrLenTemp;
    LogBuf[index][pos] = 0;

    LogList.push_back(LogBuf[index]);
}

void TestTimevalA()
{
    size_t pos = 0;

    const struct timeval timeV = Tools::GetTimeNowWithMicroseconds();
    memcpy(&LogBuf[0][pos], &timeV, SizeOfTimeval);
    pos += SizeOfTimeval;

    char secStr[21];

    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[0][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    StrLenTemp = sprintf(secStr, "|%ld|%d", timeV.tv_sec, timeV.tv_usec);
    memcpy(&LogBuf[0][pos], secStr, StrLenTemp);
    pos += StrLenTemp;
    LogBuf[0][pos] = 0;

    LogList.push_back(LogBuf[0]);
}

void TestTimevalB()
{
    size_t pos = 0;

    struct timeval timeV = Tools::GetTimeNowWithMicroseconds();
    memcpy(&LogBuf[0][pos], &timeV, SizeOfTimeval);
    pos += SizeOfTimeval;

    timeV.tv_sec = 99;

    char secStr[21];

    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[0][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    pos += Tools::GetTimeWithMicroseconds(&LogBuf[0][pos], LogBufSize, timeV, "%Y/%m/%d %H:%M:%S.");
    StrLenTemp = sprintf(secStr, "|%ld|%d", timeV.tv_sec, timeV.tv_usec);
    memcpy(&LogBuf[0][pos], secStr, StrLenTemp);
    pos += StrLenTemp;
    LogBuf[0][pos] = 0;

    LogList.push_back(LogBuf[0]);
}

void TestTimevalC()
{
    size_t pos = 0;

    struct timeval timeV = Tools::GetTimeNowWithMicroseconds();
    memcpy(&LogBuf[0][pos], &timeV, SizeOfTimeval);
    pos += SizeOfTimeval;

    timeV.tv_sec = 2100000099;

    char secStr[21];

    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[0][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    pos += Tools::GetTimeWithMicroseconds(&LogBuf[0][pos], LogBufSize, timeV, "%Y/%m/%d %H:%M:%S.");
    StrLenTemp = sprintf(secStr, "|%ld|%d", timeV.tv_sec, timeV.tv_usec);
    memcpy(&LogBuf[0][pos], secStr, StrLenTemp);
    pos += StrLenTemp;
    LogBuf[0][pos] = 0;

    LogList.push_back(LogBuf[0]);
}

void TestTime2CharArrayA()
{
    int idx = 0;

    size_t pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += Tools::GetTimeNow(&LogBuf[idx][pos], LogBufSize);
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += Tools::GetTimeNow(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S.");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    long usec = -1;
    struct tm timePast = *(struct tm *)Tools::GetTimeNowWithMicroseconds(usec);
    timePast.tm_sec -= 120;
    pos += Tools::GetTimeWithMicroseconds(&LogBuf[idx][pos], LogBufSize, &timePast, usec, "%Y/%m/%d %H:%M:%S.");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    usec = -1;
    struct tm timeFuture = *(struct tm *)Tools::GetTimeNowWithMicroseconds(usec);
    timeFuture.tm_sec += 120;
    pos += Tools::GetTimeWithMicroseconds(&LogBuf[idx][pos], LogBufSize, &timeFuture, usec, "%Y/%m/%d %H:%M:%S.");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    // ++idx;
}

void TestDate2CharArrayA()
{
    int idx = 0;

    size_t pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += Tools::GetDateToday(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += Tools::GetDateYesterday(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadIDStr, ThreadIDStrLen);
    pos += ThreadIDStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += Tools::GetDateTomorrow(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    // ++idx;
}

int main(const int argc, const char *argv[])
{
    printf("Hello World!\r\n\r\n");

    char buildInfo[128];
    int buildInfoLen = -1;
    buildInfoLen = Tools::GetBuildDate(buildInfo);
    printf("%d|%ld|BuildDate=%s\r\n", buildInfoLen, strlen(buildInfo), buildInfo);
    buildInfoLen = Tools::GetBuildTime(buildInfo);
    printf("%d|%ld|BuildTime=%s\r\n", buildInfoLen, strlen(buildInfo), buildInfo);
    buildInfoLen = Tools::GetFilePath(buildInfo);
    printf("%d|%ld|FilePath=%s\r\n", buildInfoLen, strlen(buildInfo), buildInfo);
    buildInfoLen = Tools::GetLineNumber(buildInfo);
    printf("%d|%ld|LineNumber=%s\r\n\r\n", buildInfoLen, strlen(buildInfo), buildInfo);

    ThreadIDStrLen = Tools::GetThreadIDAndStr(ThreadID, ThreadIDStr, ThreadIDStrLen);

    printf("MyPID=%d|MyPIDStrLen=%d|MyPIDStr=%s\r\n", MyPID, MyPIDStrLen, MyPIDStr);
    printf("ThreadID=%ld|ThreadIDStrLen=%d|ThreadIDStr=%s\r\n\r\n", ThreadID, ThreadIDStrLen, ThreadIDStr);

    int64_t elapsed = 0;

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&]
    {
        const char *helloWorld = " Hello World! ";

        size_t pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadIDStr, ThreadIDStrLen);
        pos += ThreadIDStrLen;
        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], helloWorld, StrLenTemp = strlen(helloWorld));
        pos += StrLenTemp;
        LogBuf[0][pos] = 0;
        
        LogList.push_back(LogBuf[0]);
    } );
    printf("\r\nTestHelloWorld1 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
    printf("TestHelloWorld1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&]
    {
        size_t pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadIDStr, ThreadIDStrLen);
        pos += ThreadIDStrLen;
        memcpy(&LogBuf[0][pos], "|PID=", StrLenTemp = strlen("|PID="));
        pos += StrLenTemp;
        memcpy(&LogBuf[0][pos], MyPIDStr, StrLenTemp = strlen(MyPIDStr));
        pos += StrLenTemp;
        LogBuf[0][pos] = 0;
        
        LogList.push_back(LogBuf[0]);
    } );
    printf("\r\nTestPID1 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
    printf("TestPID1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&]
    {
        size_t pos = 0;

        const struct timeval timeV = Tools::GetTimeNowWithMicroseconds();
        memcpy(&LogBuf[0][pos], &timeV, SizeOfTimeval);
        pos += SizeOfTimeval;

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadIDStr, ThreadIDStrLen);
        pos += ThreadIDStrLen;
        memcpy(&LogBuf[0][pos], "|PID=", StrLenTemp = strlen("|PID="));
        pos += StrLenTemp;
        memcpy(&LogBuf[0][pos], MyPIDStr, StrLenTemp = strlen(MyPIDStr));
        pos += StrLenTemp;
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    } );
    printf("\r\nTestPID2 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); } );
    printf("TestPID2 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { TestArgumentsA(argc, argv); } );
    printf("\r\nTestArguments1 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
    printf("TestArguments1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    std::thread t2([&]
    {
        ThreadIDStrLen = Tools::GetThreadIDAndStr(ThreadID, ThreadIDStr, ThreadIDStrLen);

        elapsed = Tools::GetTimeElapsedInMicroseconds([&] { TestArgumentsA(argc, argv); } );
        printf("\r\nTestArguments2 run elapsed: %ld us\r\n", elapsed);
        elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
        printf("TestArguments2 printed elapsed: %ld us\r\n", elapsed);
    });
    if (t2.joinable())
    {
        t2.join();
    }

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&]
    {
        std::thread t3([&]
        {
            ThreadIDStrLen = Tools::GetThreadIDAndStr(ThreadID, ThreadIDStr, ThreadIDStrLen);

            TestArgumentsA(argc, argv);
        });
        if (t3.joinable())
        {
            t3.join();
        }
    } );
    printf("\r\nTestArguments3 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
    printf("TestArguments3 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { TestArgumentsB(argc, argv); } );
    printf("\r\nTestArguments4 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); } );
    printf("TestArguments4 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] {
        TestSizeOfA(0, "sizeof(char)=", sizeof(char));
        TestSizeOfA(1, "sizeof(short)=", sizeof(short));
        TestSizeOfA(2, "sizeof(int)=", sizeof(int));
        TestSizeOfA(3, "sizeof(long)=", sizeof(long));
        TestSizeOfA(4, "sizeof(long long)=", sizeof(long long));
        TestSizeOfA(5, "sizeof(float)=", sizeof(float));
        TestSizeOfA(6, "sizeof(double)=", sizeof(double));
        TestSizeOfA(7, "sizeof(void *)=", sizeof(void *));
        TestSizeOfA(8, "sizeof(uint64_t)=", sizeof(uint64_t));
        TestSizeOfA(9, "sizeof(size_t)=", sizeof(size_t));
        TestSizeOfA(10, "sizeof(struct tm)=", sizeof(struct tm));
        TestSizeOfA(11, "sizeof(struct timeval)=", sizeof(struct timeval));
    } );
    printf("\r\nTestSizeOf1 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
    printf("TestSizeOf1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] {
        TestSizeOfB(0, "sizeof(char)=", sizeof(char));
        TestSizeOfB(1, "sizeof(short)=", sizeof(short));
        TestSizeOfB(2, "sizeof(int)=", sizeof(int));
        TestSizeOfB(3, "sizeof(long)=", sizeof(long));
        TestSizeOfB(4, "sizeof(long long)=", sizeof(long long));
        TestSizeOfB(5, "sizeof(float)=", sizeof(float));
        TestSizeOfB(6, "sizeof(double)=", sizeof(double));
        TestSizeOfB(7, "sizeof(void *)=", sizeof(void *));
        TestSizeOfB(8, "sizeof(uint64_t)=", sizeof(uint64_t));
        TestSizeOfB(9, "sizeof(size_t)=", sizeof(size_t));
        TestSizeOfB(10, "sizeof(struct tm)=", sizeof(struct tm));
        TestSizeOfB(11, "sizeof(struct timeval)=", sizeof(struct timeval));
    } );
    printf("\r\nTestSizeOf2 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); } );
    printf("TestSizeOf2 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { TestTimevalA(); } );
    printf("\r\nTestTimeval1 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); } );
    printf("TestTimeval1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { TestTimevalB(); } );
    printf("\r\nTestTimeval2 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); } );
    printf("TestTimeval2 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { TestTimevalC(); } );
    printf("\r\nTestTimeval3 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); } );
    printf("TestTimeval3 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { TestTime2CharArrayA(); } );
    printf("\r\nTestTime2CharArray1 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
    printf("TestTime2CharArray1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { TestDate2CharArrayA(); } );
    printf("\r\nTestDate2CharArray1 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
    printf("TestDate2CharArray1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = Tools::GetTimeElapsedInMicroseconds([&]
    {
        Tools::CharArray helloWorld = Tools::CharArray(" Hello C++ World from VS Code! ");
        char lenBuf[11];
        size_t pos = Tools::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadIDStr, ThreadIDStrLen);
        pos += ThreadIDStrLen;
        StrLenTemp = sprintf(lenBuf, "|%d|", helloWorld.Length());
        memcpy(&LogBuf[0][pos], lenBuf, StrLenTemp);
        pos += StrLenTemp;
        memcpy(&LogBuf[0][pos], helloWorld.Body(), helloWorld.Length());
        pos += helloWorld.Length();
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    } );
    printf("\r\nTestCharArray1 run elapsed: %ld us\r\n", elapsed);
    elapsed = Tools::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); } );
    printf("TestCharArray1 printed elapsed: %ld us\r\n", elapsed);

    char inputBuf1[8];
    memset(inputBuf1, 0, sizeof(inputBuf1));

    //"%7s"
    const char iB1Fmt[] = {'%', sizeof(inputBuf1) - 1 + '0', 's', 0};

    int scanfResult = -1;

    printf("\r\nPress any key to continue.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    Tools::CleanStdin();
    printf("scanfResult=%d|len=%ld|input=%s|\r\n", scanfResult, strlen(inputBuf1), inputBuf1);

    printf("\r\nPress any key to exit.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    Tools::CleanStdin();
    printf("scanfResult=%d|len=%ld|input=%s|\r\n", scanfResult, strlen(inputBuf1), inputBuf1);

    usleep(8 * 1000 * 1000);
    return 0;
}
