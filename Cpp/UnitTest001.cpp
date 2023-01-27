#include <stdlib.h>
#include <unistd.h>

#include "Tools/Functions.h"

const int MyPID = getpid();
char MyPIDStr[11];
const int MyPIDStrLen = sprintf(MyPIDStr, "%d", MyPID);

thread_local uint64_t ThreadID = 0;
thread_local char ThreadIDStr[21];
thread_local int ThreadIDStrLen = -1;

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

    printf("sizeof(char)=%ld\r\n", sizeof(char));
    printf("sizeof(short)=%ld\r\n", sizeof(short));
    printf("sizeof(int)=%ld\r\n", sizeof(int));
    printf("sizeof(long)=%ld\r\n", sizeof(long));
    printf("sizeof(long long)=%ld\r\n", sizeof(long long));
    printf("sizeof(float)=%ld\r\n", sizeof(float));
    printf("sizeof(double)=%ld\r\n", sizeof(double));
    printf("sizeof(void *)=%ld\r\n", sizeof(void *));
    printf("sizeof(uint64_t)=%ld\r\n", sizeof(uint64_t));
    printf("sizeof(size_t)=%ld\r\n", sizeof(size_t));
    printf("sizeof(struct tm)=%ld\r\n", sizeof(struct tm));
    printf("sizeof(struct timeval)=%ld\r\n", sizeof(struct timeval));

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
