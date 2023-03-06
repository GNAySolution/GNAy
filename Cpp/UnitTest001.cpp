#include <stdlib.h>
#include <unistd.h>
#include <vector>

#include "Tools/ArrayWithLength.h"
#include "Tools/ASCIINumberMath.h"
#include "Tools/FileLogger.h"
#include "Tools/Functions.h"
#include "Tools/NumberWidthMax.h"
#include "Tools/NumberWithString.h"
#include "Tools/ThreadHelper.h"
#include "Tools/ThreadPool.h"
#include "Tools/TimeHelper.h"

using namespace Tools;

#pragma pack(1)

const int SizeOfTimeval = sizeof(struct timeval);

const int LogBufCntMax = 128;
const int LogBufSize = 1024;

ThreadPool TPool;

char LogBuf[LogBufCntMax][LogBufSize];
std::vector<const char *> LogList;

const struct IntWithStr MyPID = NumberWithString::CreateIntWithStr(getpid());

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

        TimeHelper::GetTimeWithMicroseconds(timeBuf, timeBufSize, *rawTime, "%H:%M:%S.");

        printf("idx=%d|time=%s|len=%ld|msg=%s|\r\n", ++idx, timeBuf, strlen(&msg[SizeOfTimeval]), &msg[SizeOfTimeval]);
    }
}

void TestArgumentsA(const int& argc, const char *argv[])
{
    for (int i = 0; i < argc; ++i)
    {
        char idxStr[IntMaxArraySize];
        int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[i][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[i][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[i][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        FileLogger::MsgLen = sprintf(idxStr, "|argv[%d]=", i);
        memcpy(&LogBuf[i][pos], idxStr, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[i][pos], argv[i], FileLogger::MsgLen = strlen(argv[i]));
        pos += FileLogger::MsgLen;
        LogBuf[i][pos] = 0;

        LogList.push_back(LogBuf[i]);
    }

    {
        int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[argc][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[argc][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[argc][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        LogBuf[argc][pos] = 0;

        LogList.push_back(LogBuf[argc]);
    }
}

void TestArgumentsB(const int& argc, const char *argv[])
{
    for (int i = 0; i < argc; ++i)
    {
        char idxStr[IntMaxArraySize];
        int pos = 0;
        const struct timeval timeV = TimeHelper::GetTimeNowWithMicroseconds();

        memcpy(&LogBuf[i][pos], &timeV, SizeOfTimeval);
        pos += SizeOfTimeval;
        memcpy(&LogBuf[i][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[i][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        FileLogger::MsgLen = sprintf(idxStr, "|argv[%d]=", i);
        memcpy(&LogBuf[i][pos], idxStr, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[i][pos], argv[i], FileLogger::MsgLen = strlen(argv[i]));
        pos += FileLogger::MsgLen;
        LogBuf[i][pos] = 0;

        LogList.push_back(LogBuf[i]);
    }

    {
        int pos = 0;
        const struct timeval timeV = TimeHelper::GetTimeNowWithMicroseconds();

        memcpy(&LogBuf[argc][pos], &timeV, SizeOfTimeval);
        pos += SizeOfTimeval;
        memcpy(&LogBuf[argc][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[argc][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        LogBuf[argc][pos] = 0;

        LogList.push_back(LogBuf[argc]);
    }
}

void TestSizeOfA(const int& index, const char *typeMsg, const int& size)
{
    char sizeStr[IntMaxArraySize]; 
    int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[index][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[index][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[index][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[index][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[index][pos], typeMsg, FileLogger::MsgLen = strlen(typeMsg));
    pos += FileLogger::MsgLen;
    FileLogger::MsgLen = sprintf(sizeStr, "%d", size);
    memcpy(&LogBuf[index][pos], sizeStr, FileLogger::MsgLen);
    pos += FileLogger::MsgLen;
    LogBuf[index][pos] = 0;

    LogList.push_back(LogBuf[index]);
}

void TestSizeOfB(const int& index, const char *typeMsg, const int& size)
{
    char sizeStr[IntMaxArraySize];
    int pos = 0;
    const struct timeval timeV = TimeHelper::GetTimeNowWithMicroseconds();

    memcpy(&LogBuf[index][pos], &timeV, SizeOfTimeval);
    pos += SizeOfTimeval;
    memcpy(&LogBuf[index][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[index][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[index][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[index][pos], typeMsg, FileLogger::MsgLen = strlen(typeMsg));
    pos += FileLogger::MsgLen;
    FileLogger::MsgLen = sprintf(sizeStr, "%d", size);
    memcpy(&LogBuf[index][pos], sizeStr, FileLogger::MsgLen);
    pos += FileLogger::MsgLen;
    LogBuf[index][pos] = 0;

    LogList.push_back(LogBuf[index]);
}

void TestTimevalA()
{
    int pos = 0;
    const struct timeval timeV = TimeHelper::GetTimeNowWithMicroseconds();
    char secStr[LongMaxArraySize + IntMaxArraySize];

    memcpy(&LogBuf[0][pos], &timeV, SizeOfTimeval);
    pos += SizeOfTimeval;
    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    FileLogger::MsgLen = sprintf(secStr, "|%ld|%d", timeV.tv_sec, timeV.tv_usec);
    memcpy(&LogBuf[0][pos], secStr, FileLogger::MsgLen);
    pos += FileLogger::MsgLen;
    LogBuf[0][pos] = 0;

    LogList.push_back(LogBuf[0]);
}

void TestTimevalB()
{
    int pos = 0;
    struct timeval timeV = TimeHelper::GetTimeNowWithMicroseconds();

    memcpy(&LogBuf[0][pos], &timeV, SizeOfTimeval);
    pos += SizeOfTimeval;

    timeV.tv_sec = 99;

    char secStr[LongMaxArraySize + IntMaxArraySize];

    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    pos += TimeHelper::GetTimeWithMicroseconds(&LogBuf[0][pos], LogBufSize, timeV, "%Y/%m/%d %H:%M:%S.");
    FileLogger::MsgLen = sprintf(secStr, "|%ld|%d", timeV.tv_sec, timeV.tv_usec);
    memcpy(&LogBuf[0][pos], secStr, FileLogger::MsgLen);
    pos += FileLogger::MsgLen;
    LogBuf[0][pos] = 0;

    LogList.push_back(LogBuf[0]);
}

void TestTimevalC()
{
    int pos = 0;
    struct timeval timeV = TimeHelper::GetTimeNowWithMicroseconds();

    memcpy(&LogBuf[0][pos], &timeV, SizeOfTimeval);
    pos += SizeOfTimeval;

    timeV.tv_sec = 2100000099;

    char secStr[LongMaxArraySize + IntMaxArraySize];

    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    pos += TimeHelper::GetTimeWithMicroseconds(&LogBuf[0][pos], LogBufSize, timeV, "%Y/%m/%d %H:%M:%S.");
    FileLogger::MsgLen = sprintf(secStr, "|%ld|%d", timeV.tv_sec, timeV.tv_usec);
    memcpy(&LogBuf[0][pos], secStr, FileLogger::MsgLen);
    pos += FileLogger::MsgLen;
    LogBuf[0][pos] = 0;

    LogList.push_back(LogBuf[0]);
}

void TestTime2CharArrayA()
{
    int idx = 0;
    int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += TimeHelper::GetTimeNow(&LogBuf[idx][pos], LogBufSize);
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += TimeHelper::GetTimeNow(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S.");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;

    long usec = -1;

    struct tm timePast = *(struct tm *)TimeHelper::GetTimeNowWithMicroseconds(usec);
    timePast.tm_sec -= 120;

    pos += TimeHelper::GetTimeWithMicroseconds(&LogBuf[idx][pos], LogBufSize, &timePast, usec, "%Y/%m/%d %H:%M:%S.");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;

    usec = -1;

    struct tm timeFuture = *(struct tm *)TimeHelper::GetTimeNowWithMicroseconds(usec);
    timeFuture.tm_sec += 120;

    pos += TimeHelper::GetTimeWithMicroseconds(&LogBuf[idx][pos], LogBufSize, &timeFuture, usec, "%Y/%m/%d %H:%M:%S.");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    // ++idx;
}

void TestDate2CharArrayA()
{
    int idx = 0;
    int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += TimeHelper::GetDateToday(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += TimeHelper::GetDateYesterday(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    ++idx;

    pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[idx][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[idx][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[idx][pos], "|", 1);
    ++pos;
    pos += TimeHelper::GetDateTomorrow(&LogBuf[idx][pos], LogBufSize, "%Y/%m/%d %H:%M:%S");
    LogBuf[idx][pos] = 0;
    LogList.push_back(LogBuf[idx]);
    // ++idx;
}

void TestASCIINumberCalculateA(const char *argv)
{
    FileLogger::MsgLen = strlen(argv);

    char buf[64];
    int bufPos = 0;
    char bufPosStr[IntMaxArraySize];

    bufPos = ASCIINumberMath::Calculate(argv, FileLogger::MsgLen, buf, sizeof(buf));

    int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
    pos += ThreadHelper::ThreadID.ValueStrLen;
    memcpy(&LogBuf[0][pos], "|", 1);
    ++pos;
    memcpy(&LogBuf[0][pos], argv, FileLogger::MsgLen);
    pos += FileLogger::MsgLen;
    FileLogger::MsgLen = sprintf(bufPosStr, "|%d|%ld|", bufPos, strlen(buf) - bufPos);
    memcpy(&LogBuf[0][pos], bufPosStr, FileLogger::MsgLen);
    pos += FileLogger::MsgLen;
    memcpy(&LogBuf[0][pos], buf, FileLogger::MsgLen = strlen(buf));
    pos += FileLogger::MsgLen;
    LogBuf[0][pos] = 0;

    LogList.push_back(LogBuf[0]);
}

int main(const int argc, const char *argv[])
{
    printf("%s|%s|%s| Hello World! |%d|%s|%s|\r\n\r\n", TimeHelper::GetHHmmssffffff(), LogLevel::Trace, ThreadHelper::ThreadID.ValueStr, __LINE__, __FUNCTION__, __FILE__);

    const char *valueStr = Functions::FindValueStr("ThreadPoolSizeMax", argc, argv);
    TPool.SetSizeMax(valueStr == NULL ? 0 : std::stoull(valueStr));

    printf("%s|%s|%s|MyPID=%d|MyPIDStrLen=%d|MyPIDStr=%s|%d|%s|%s|\r\n", TimeHelper::GetHHmmssffffff(), LogLevel::Trace, ThreadHelper::ThreadID.ValueStr, MyPID.Value, MyPID.ValueStrLen, MyPID.ValueStr, __LINE__, __FUNCTION__, __FILE__);

    {
        char arr01[BoolMaxArraySize];
        char arr02[BoolMaxStringLength];
        char arr03[BoolMaxArraySize + BoolMaxStringLength];
        char arr04[BoolMaxArraySize * 2 + BoolMaxStringLength];
        char arr05[CharMaxArraySize];
        char arr06[CharMaxStringLength];
        char arr07[ShortMaxArraySize];
        char arr08[ShortMaxStringLength];
        char arr09[IntMaxArraySize];
        char arr10[IntMaxStringLength];
        char arr11[LongMaxArraySize];
        char arr12[LongMaxStringLength];

        printf("BoolMaxArraySize = %ld\r\n", sizeof(arr01));
        printf("BoolMaxStringLength = %ld\r\n", sizeof(arr02));
        printf("BoolMaxArraySize + BoolMaxStringLength = %ld\r\n", sizeof(arr03));
        printf("BoolMaxArraySize * 2 + BoolMaxStringLength = %ld\r\n", sizeof(arr04));
        printf("CharMaxArraySize = %ld\r\n", sizeof(arr05));
        printf("CharMaxStringLength = %ld\r\n", sizeof(arr06));
        printf("ShortMaxArraySize = %ld\r\n", sizeof(arr07));
        printf("ShortMaxStringLength = %ld\r\n", sizeof(arr08));
        printf("IntMaxArraySize = %ld\r\n", sizeof(arr09));
        printf("IntMaxStringLength = %ld\r\n", sizeof(arr10));
        printf("LongMaxArraySize = %ld\r\n", sizeof(arr11));
        printf("LongMaxStringLength = %ld\r\n", sizeof(arr12));
    }

    int64_t elapsed = 0;

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        const char *helloWorld = " Hello World! ";
        int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], helloWorld, FileLogger::MsgLen = strlen(helloWorld));
        pos += FileLogger::MsgLen;
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    });
    printf("\r\nTestHelloWorld1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestHelloWorld1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        const char *helloWorld = " Hello World! ";
        int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], helloWorld, FileLogger::MsgLen = strlen(helloWorld));
        pos += FileLogger::MsgLen;
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    });
    printf("\r\nTestHelloWorld2 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestHelloWorld2 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        char buildInfo[256];
        int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        FileLogger::MsgLen = sprintf(buildInfo, "|BuildDate=%s", __DATE__);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = sprintf(buildInfo, "|BuildTime=%s", __TIME__);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = sprintf(buildInfo, "|LineNumber=%d", __LINE__);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = sprintf(buildInfo, "|FunctionName=%s", __FUNCTION__);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = sprintf(buildInfo, "|FilePath=%s", __FILE__);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    });
    printf("\r\nTestBuildInfo1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestBuildInfo1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        char buildInfo[256];
        int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        memcpy(&LogBuf[0][pos], "|BuildDate=", FileLogger::MsgLen = strlen("|BuildDate="));
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = Functions::GetBuildDate(buildInfo);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[0][pos], "|BuildTime=", FileLogger::MsgLen = strlen("|BuildTime="));
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = Functions::GetBuildTime(buildInfo);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[0][pos], "|LineNumber=", FileLogger::MsgLen = strlen("|LineNumber="));
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = Functions::GetLineNumber(buildInfo);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[0][pos], "|FunctionName=", FileLogger::MsgLen = strlen("|FunctionName="));
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = Functions::GetFunctionName(buildInfo);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[0][pos], "|FilePath=", FileLogger::MsgLen = strlen("|FilePath="));
        pos += FileLogger::MsgLen;
        FileLogger::MsgLen = Functions::GetFilePath(buildInfo);
        memcpy(&LogBuf[0][pos], buildInfo, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    });
    printf("\r\nTestBuildInfo2 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestBuildInfo2 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        memcpy(&LogBuf[0][pos], "|PID=", FileLogger::MsgLen = strlen("|PID="));
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[0][pos], MyPID.ValueStr, FileLogger::MsgLen = strlen(MyPID.ValueStr));
        pos += FileLogger::MsgLen;
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    });
    printf("\r\nTestPID1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestPID1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        int pos = 0;
        const struct timeval timeV = TimeHelper::GetTimeNowWithMicroseconds();

        memcpy(&LogBuf[0][pos], &timeV, SizeOfTimeval);
        pos += SizeOfTimeval;
        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        memcpy(&LogBuf[0][pos], "|PID=", FileLogger::MsgLen = strlen("|PID="));
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[0][pos], MyPID.ValueStr, FileLogger::MsgLen = strlen(MyPID.ValueStr));
        pos += FileLogger::MsgLen;
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    });
    printf("\r\nTestPID2 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); });
    printf("TestPID2 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestArgumentsA(argc, argv); });
    printf("\r\nTestArguments1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestArguments1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    std::thread t2([&]
    {
        elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestArgumentsA(argc, argv); });
        printf("\r\nTestArguments2 run elapsed: %ld us\r\n", elapsed);
        elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
        printf("TestArguments2 printed elapsed: %ld us\r\n", elapsed);
    });
    if (t2.joinable())
    {
        t2.join();
    }

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        std::thread t3([&] { TestArgumentsA(argc, argv); });
        if (t3.joinable())
        {
            t3.join();
        }
    });
    printf("\r\nTestArguments3 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestArguments3 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestArgumentsB(argc, argv); });
    printf("\r\nTestArguments4 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); });
    printf("TestArguments4 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
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
    });
    printf("\r\nTestSizeOf1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestSizeOf1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
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
    });
    printf("\r\nTestSizeOf2 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); });
    printf("TestSizeOf2 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestTimevalA(); });
    printf("\r\nTestTimeval1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); });
    printf("TestTimeval1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestTimevalB(); });
    printf("\r\nTestTimeval2 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); });
    printf("TestTimeval2 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestTimevalC(); });
    printf("\r\nTestTimeval3 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesB(LogList); });
    printf("TestTimeval3 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestTime2CharArrayA(); });
    printf("\r\nTestTime2CharArray1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestTime2CharArray1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestDate2CharArrayA(); });
    printf("\r\nTestDate2CharArray1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestDate2CharArray1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        const CharArray helloWorld = CharArray(" Hello C++ World from VS Code! ");
        char lenBuf[IntMaxArraySize];
        int pos = TimeHelper::GetTimeNowWithMicroseconds(&LogBuf[0][0], LogBufSize, "%H:%M:%S.");

        memcpy(&LogBuf[0][pos], "|", 1);
        ++pos;
        memcpy(&LogBuf[0][pos], ThreadHelper::ThreadID.ValueStr, ThreadHelper::ThreadID.ValueStrLen);
        pos += ThreadHelper::ThreadID.ValueStrLen;
        FileLogger::MsgLen = sprintf(lenBuf, "|%d|", helloWorld.Length());
        memcpy(&LogBuf[0][pos], lenBuf, FileLogger::MsgLen);
        pos += FileLogger::MsgLen;
        memcpy(&LogBuf[0][pos], helloWorld.Body(), helloWorld.Length());
        pos += helloWorld.Length();
        LogBuf[0][pos] = 0;

        LogList.push_back(LogBuf[0]);
    });
    printf("\r\nTestCharArray1 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestCharArray1 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA(""); });
    printf("\r\nTestASCIINumberCalculate01 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate01 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA(" "); });
    printf("\r\nTestASCIINumberCalculate02 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate02 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("  "); });
    printf("\r\nTestASCIINumberCalculate03 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate03 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("0"); });
    printf("\r\nTestASCIINumberCalculate04 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate04 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1"); });
    printf("\r\nTestASCIINumberCalculate05 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate05 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("9"); });
    printf("\r\nTestASCIINumberCalculate06 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate06 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("0+0"); });
    printf("\r\nTestASCIINumberCalculate07 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate07 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("0+1"); });
    printf("\r\nTestASCIINumberCalculate08 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate08 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("0+9"); });
    printf("\r\nTestASCIINumberCalculate09 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate09 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1+0"); });
    printf("\r\nTestASCIINumberCalculate10 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate10 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1+1"); });
    printf("\r\nTestASCIINumberCalculate11 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate11 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1+9"); });
    printf("\r\nTestASCIINumberCalculate12 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate12 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1112+0"); });
    printf("\r\nTestASCIINumberCalculate13 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate13 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1112+1"); });
    printf("\r\nTestASCIINumberCalculate14 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate14 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1112+9"); });
    printf("\r\nTestASCIINumberCalculate15 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate15 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1118+0"); });
    printf("\r\nTestASCIINumberCalculate16 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate16 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1118+1"); });
    printf("\r\nTestASCIINumberCalculate17 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate17 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1118+9"); });
    printf("\r\nTestASCIINumberCalculate18 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate18 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1119+0"); });
    printf("\r\nTestASCIINumberCalculate19 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate19 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1119+1"); });
    printf("\r\nTestASCIINumberCalculate20 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate20 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1119+9"); });
    printf("\r\nTestASCIINumberCalculate21 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate21 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-0"); });
    printf("\r\nTestASCIINumberCalculate22 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate22 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1"); });
    printf("\r\nTestASCIINumberCalculate23 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate23 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-9"); });
    printf("\r\nTestASCIINumberCalculate24 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate24 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-0-0"); });
    printf("\r\nTestASCIINumberCalculate25 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate25 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-0-1"); });
    printf("\r\nTestASCIINumberCalculate26 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate26 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-0-9"); });
    printf("\r\nTestASCIINumberCalculate27 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate27 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1-0"); });
    printf("\r\nTestASCIINumberCalculate28 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate28 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1-1"); });
    printf("\r\nTestASCIINumberCalculate29 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate29 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1-9"); });
    printf("\r\nTestASCIINumberCalculate30 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate30 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1112-0"); });
    printf("\r\nTestASCIINumberCalculate31 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate31 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1112-1"); });
    printf("\r\nTestASCIINumberCalculate32 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate32 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1112-9"); });
    printf("\r\nTestASCIINumberCalculate33 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate33 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1118-0"); });
    printf("\r\nTestASCIINumberCalculate34 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate34 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1118-1"); });
    printf("\r\nTestASCIINumberCalculate35 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate35 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1118-9"); });
    printf("\r\nTestASCIINumberCalculate36 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate36 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1119-0"); });
    printf("\r\nTestASCIINumberCalculate37 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate37 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1119-1"); });
    printf("\r\nTestASCIINumberCalculate38 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate38 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1119-9"); });
    printf("\r\nTestASCIINumberCalculate39 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate39 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("0-0"); });
    printf("\r\nTestASCIINumberCalculate40 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate40 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("0-1"); });
    printf("\r\nTestASCIINumberCalculate41 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate41 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("0-9"); });
    printf("\r\nTestASCIINumberCalculate42 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate42 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1-0"); });
    printf("\r\nTestASCIINumberCalculate43 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate43 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1-1"); });
    printf("\r\nTestASCIINumberCalculate44 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate44 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1-9"); });
    printf("\r\nTestASCIINumberCalculate45 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate45 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1112-0"); });
    printf("\r\nTestASCIINumberCalculate46 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate46 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1112-1"); });
    printf("\r\nTestASCIINumberCalculate47 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate47 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1112-9"); });
    printf("\r\nTestASCIINumberCalculate48 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate48 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1118-0"); });
    printf("\r\nTestASCIINumberCalculate49 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate49 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1118-1"); });
    printf("\r\nTestASCIINumberCalculate50 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate50 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1118-9"); });
    printf("\r\nTestASCIINumberCalculate51 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate51 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1119-0"); });
    printf("\r\nTestASCIINumberCalculate52 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate52 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1119-1"); });
    printf("\r\nTestASCIINumberCalculate53 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate53 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("1119-9"); });
    printf("\r\nTestASCIINumberCalculate54 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate54 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-0+0"); });
    printf("\r\nTestASCIINumberCalculate55 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate55 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-0+1"); });
    printf("\r\nTestASCIINumberCalculate56 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate56 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-0+9"); });
    printf("\r\nTestASCIINumberCalculate57 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate57 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1+0"); });
    printf("\r\nTestASCIINumberCalculate58 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate58 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1+1"); });
    printf("\r\nTestASCIINumberCalculate59 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate59 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1+9"); });
    printf("\r\nTestASCIINumberCalculate60 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate60 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1112+0"); });
    printf("\r\nTestASCIINumberCalculate61 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate61 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1112+1"); });
    printf("\r\nTestASCIINumberCalculate62 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate62 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1112+9"); });
    printf("\r\nTestASCIINumberCalculate63 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate63 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1118+0"); });
    printf("\r\nTestASCIINumberCalculate64 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate64 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1118+1"); });
    printf("\r\nTestASCIINumberCalculate65 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate65 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1118+9"); });
    printf("\r\nTestASCIINumberCalculate66 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate66 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1119+0"); });
    printf("\r\nTestASCIINumberCalculate67 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate67 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1119+1"); });
    printf("\r\nTestASCIINumberCalculate68 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate68 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { TestASCIINumberCalculateA("-1119+9"); });
    printf("\r\nTestASCIINumberCalculate69 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate69 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        const char *valueStr = Functions::FindValueStr("ASCIINumberMathCalculate1", argc, argv);
        TestASCIINumberCalculateA(valueStr == NULL ? "" : valueStr);
    });
    printf("\r\nTestASCIINumberCalculate70 run elapsed: %ld us\r\n", elapsed);
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&] { PrintMessagesA(LogList); });
    printf("TestASCIINumberCalculate70 printed elapsed: %ld us\r\n", elapsed);

    LogList.clear();
    elapsed = TimeHelper::GetTimeElapsedInMicroseconds([&]
    {
        for (int i = 0; i < 8; ++i)
        {
            TPool.Enqueue([&, i]
            {
                printf("TestThreadPool1A|i=%d|ThreadID=%lld|ThreadIDStrLen=%d|ThreadIDStr=%s\r\n", i, ThreadHelper::ThreadID.Value, ThreadHelper::ThreadID.ValueStrLen, ThreadHelper::ThreadID.ValueStr);
                std::this_thread::sleep_for(std::chrono::seconds(i + 1) / 2);
                printf("TestThreadPool1B|i=%d|ThreadID=%lld|ThreadIDStrLen=%d|ThreadIDStr=%s\r\n", i, ThreadHelper::ThreadID.Value, ThreadHelper::ThreadID.ValueStrLen, ThreadHelper::ThreadID.ValueStr);

                return i * i;
            });
        }
    });
    printf("\r\nTestThreadPool1 run elapsed: %ld us\r\n", elapsed);

    char inputBuf1[8];
    memset(inputBuf1, 0, sizeof(inputBuf1));

    const char iB1Fmt[] = {'%', sizeof(inputBuf1) - 1 + '0', 's', 0}; //"%7s"
    int scanfResult = -1;

    printf("\r\nPress any key to continue.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    Functions::CleanStdin();
    printf("scanfResult=%d|len=%ld|input=%s|\r\n", scanfResult, strlen(inputBuf1), inputBuf1);

    printf("\r\nPress any key to exit.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    Functions::CleanStdin();
    printf("scanfResult=%d|len=%ld|input=%s|\r\n", scanfResult, strlen(inputBuf1), inputBuf1);

    usleep(8 * 1000 * 1000);

    return EXIT_SUCCESS;
}

#pragma pack()
