#include "StaticTools/Functions.h"

using namespace MetaTools;
using namespace StaticTools;

#pragma pack(1)

const int MyPID = getpid();

int main(const int argc, const char *argv[])
{
    printf("Hello World!\n\n");

    printf("MyPID=%d\n", MyPID);
    printf("MyPID=0x%08X\n\n", MyPID);

    STFunc::ChangeConst(MyPID, 1);

    printf("MyPID=%d\n", MyPID);
    printf("MyPID=0x%08X\n\n", MyPID);

    const char *argTest1 = STFunc::FindValueStr("StartedFrom", argc, argv);
    const char *argTest2 = STFunc::FindValueStr("startedFrom", argc, argv);
    const char *argTest3 = STFunc::FindValueStr("ConfigPath", argc, argv);
    const char *argTest4 = STFunc::FindValueStr("configPath", argc, argv);

    printf("%ld|StartedFrom=%s|\n", argTest1 == NULL ? 0 : strlen(argTest1), argTest1);
    printf("%ld|startedFrom=%s|\n", argTest2 == NULL ? 0 : strlen(argTest2), argTest2);
    printf("%ld|ConfigPath=%s|\n", argTest3 == NULL ? 0 : strlen(argTest3), argTest3);
    printf("%ld|configPath=%s|\n\n", argTest4 == NULL ? 0 : strlen(argTest4), argTest4);

    printf("%d|%d|%s|\n", strlen(MetaFunc::FilePath), MetaFunc::GetLength(MetaFunc::FilePath), MetaFunc::FilePath);
    printf("%d|%d|%s|\n\n", strlen(MetaFunc::FileName), MetaFunc::GetLength(MetaFunc::FileName), MetaFunc::FileName);

    printf("%d|%d|%s|\n", strlen(STFunc::FilePath), MetaFunc::GetLength(STFunc::FilePath), STFunc::FilePath);
    printf("%d|%d|%s|\n\n", strlen(STFunc::FileName), MetaFunc::GetLength(STFunc::FileName), STFunc::FileName);

    printf("Line=%d|ThisFileName=%s|%d|%d|\n\n", __LINE__, _FILE_NAME_, strlen(_FILE_NAME_), MetaFunc::GetLength(_FILE_NAME_));

    char inputBuf1[8];
    constexpr char iB1Fmt[] = {'%', sizeof(inputBuf1) - 1 + '0', 's', 0}; //"%7s"
    int scanfResult = -1;

    printf("inputBuf1=0x");
    STFunc::Print2Hexdecimal(sizeof(inputBuf1), inputBuf1, ' ');
    printf("\n\n");

    #if __GNUC__ >= 5
    constexpr bool iB1FmtLen3 = MetaFunc::GetLength(iB1Fmt) == 3;
    #else
    constexpr bool iB1FmtLen3 = MetaFunc::GetLength(iB1Fmt, 0) == 3;
    #endif
    printf("iB1Fmt=%s|%d|%d|%d|\n\n", iB1Fmt, iB1FmtLen3, strlen(iB1Fmt), MetaFunc::GetLength(iB1Fmt));

    printf("Press any key to continue.\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    STFunc::CleanStdin();
    printf("scanfResult=%d|len=%ld|input=%s|\n\n", scanfResult, strlen(inputBuf1), inputBuf1);

    printf("Press any key to exit.\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    STFunc::CleanStdin();
    printf("scanfResult=%d|len=%ld|input=%s|\n\n", scanfResult, strlen(inputBuf1), inputBuf1);

    usleep(4 * 1000 * 1000);

    return EXIT_SUCCESS;
}

#pragma pack()