#include <cstring>
#include <stdio.h>

int main(int argc, char *argv[]) {

    char inputBuf[8];
    memset(inputBuf, 0, sizeof(inputBuf));

    printf("Hello World!\r\n");

    scanf("%7s", inputBuf);
    printf("%s\r\n", inputBuf);

    memset(inputBuf, 0, sizeof(inputBuf));
    scanf("%7s", inputBuf);

    return 0;
}