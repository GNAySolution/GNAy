#include <cstdlib>
#include <cstring>
#include <stdio.h>

int main(int argc, char *argv[]) {

    char inputBuf[128];
    memset(inputBuf, 0, sizeof(inputBuf));

    printf("Hello World!\r\n");

    scanf("%127s", inputBuf);
    printf("%s\r\n", inputBuf);

    memset(inputBuf, 0, sizeof(inputBuf));
    scanf("%127s", inputBuf);

    exit(0);
    return 0;
}