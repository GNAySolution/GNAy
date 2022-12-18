#include <cstring>
#include <stdio.h>
#include <unistd.h>

int main(const int argc, const char *argv[]) {

    char inputBuf1[8];
    memset(inputBuf1, 0, sizeof(inputBuf1));

    //"%7s"
    const char iB1Fmt[] = {'%', sizeof(inputBuf1) - 1 + '0', 's', 0};

    int scanfResult = -1;
    int fflushResult = -1;

    printf("Hello World!\r\n");

    printf("\r\nPress any key to continue.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    fflushResult = fflush(stdin);
    printf("scanfResult=%i|fflushResult=%i|len=%li|%s|\r\n", scanfResult, fflushResult, strlen(inputBuf1), inputBuf1);

    printf("\r\nPress any key to exit.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    fflushResult = fflush(stdin);
    printf("scanfResult=%i|fflushResult=%i|len=%li|%s|\r\n", scanfResult, fflushResult, strlen(inputBuf1), inputBuf1);

    usleep(8 * 1000 * 1000);
    return 0;
}