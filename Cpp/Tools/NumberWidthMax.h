#ifndef _TOOLS_NUMBER_WIDTH_MAX_H
#define _TOOLS_NUMBER_WIDTH_MAX_H

#include <limits>

#pragma pack(1)

namespace Tools
{
    template<typename T, long long N = std::numeric_limits<T>::max()>
    class NumberWidthMax
    {
        public:
            enum
            {
                ArraySize = NumberWidthMax<T, N / 10>::ArraySize + 1,
                StringLength = NumberWidthMax<T, N / 10>::StringLength + 1,
            };
    };

    template<typename T>
    class NumberWidthMax<T, 0>
    {
        public:
            enum
            {
                ArraySize = 1 + 1,
                StringLength = 1,
            };
    };

    const int BoolMaxArraySize = NumberWidthMax<bool>::ArraySize;
    const int BoolMaxStringLength = NumberWidthMax<bool>::StringLength;
    const int CharMaxArraySize = NumberWidthMax<char>::ArraySize;
    const int CharMaxStringLength = NumberWidthMax<char>::StringLength;
    const int ShortMaxArraySize = NumberWidthMax<short>::ArraySize;
    const int ShortMaxStringLength = NumberWidthMax<short>::StringLength;
    const int IntMaxArraySize = NumberWidthMax<int>::ArraySize;
    const int IntMaxStringLength = NumberWidthMax<int>::StringLength;
    const int LongMaxArraySize = NumberWidthMax<long long>::ArraySize;
    const int LongMaxStringLength = NumberWidthMax<long long>::StringLength;
}

#pragma pack()

#endif
