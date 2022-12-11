#include <cstring>
#include <stdio.h>
#include <unistd.h>

int main(const int argc, const char *argv[]) {

    char inputBuf1[8];
    memset(inputBuf1, 0, sizeof(inputBuf1));

    const char ib1fmt[] = {'%', sizeof(inputBuf1) - 1 + '0', 's', 0};

    printf("Hello World!\r\n");

    scanf(ib1fmt, inputBuf1);
    printf("%s\r\n", inputBuf1);

    printf("\r\nPress any key to exit.\r\n");
    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanf(ib1fmt, inputBuf1);
    printf("%s\r\n", inputBuf1);

    usleep(3 * 1000 * 1000);
    return 0;
}