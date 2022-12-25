#include <cstring>
#include <stdio.h>
#include <unistd.h>

void CleanStdin()
{
    int c;

    do {
        c = getchar();
    } while (c != '\n' && c != EOF);
}

int main(const int argc, const char *argv[])
{
    printf("Hello World!\r\n");

    printf("sizeof(char)=%i\r\n", sizeof(char));
    printf("sizeof(short)=%i\r\n", sizeof(short));
    printf("sizeof(int)=%i\r\n", sizeof(int));
    printf("sizeof(long)=%i\r\n", sizeof(long));
    printf("sizeof(long long)=%i\r\n", sizeof(long long));
    printf("sizeof(float)=%i\r\n", sizeof(float));
    printf("sizeof(double)=%i\r\n", sizeof(double));
    printf("sizeof(void *)=%i\r\n", sizeof(void *));

    char inputBuf1[8];
    memset(inputBuf1, 0, sizeof(inputBuf1));

    //"%7s"
    const char iB1Fmt[] = {'%', sizeof(inputBuf1) - 1 + '0', 's', 0};

    int scanfResult = -1;

    printf("\r\nPress any key to continue.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    CleanStdin();
    printf("scanfResult=%i|len=%li|input=%s|\r\n", scanfResult, strlen(inputBuf1), inputBuf1);

    printf("\r\nPress any key to exit.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanfResult = scanf(iB1Fmt, inputBuf1);
    CleanStdin();
    printf("scanfResult=%i|len=%li|input=%s|\r\n", scanfResult, strlen(inputBuf1), inputBuf1);

    usleep(8 * 1000 * 1000);
    return 0;
}