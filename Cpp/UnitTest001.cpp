#include <cstring>
#include <stdio.h>
#include <unistd.h>

int main(int argc, char *argv[]) {

    char inputBuf1[8];
    memset(inputBuf1, 0, sizeof(inputBuf1));

    const char iB1Format[] = {'%', sizeof(inputBuf1) - 1 + '0', 's', 0};

    printf("Hello World!\r\n");

    scanf(iB1Format, inputBuf1);
    printf("%s\r\n", inputBuf1);

    memset(inputBuf1, 0, sizeof(inputBuf1));
    scanf(iB1Format, inputBuf1);

    usleep(3 * 1000 * 1000);
    return 0;
}