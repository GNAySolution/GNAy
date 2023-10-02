#ifndef _STATIC_TOOLS_ENVIRONMENT_H
#define _STATIC_TOOLS_ENVIRONMENT_H

#include "../MetaTools/Functions.h"

using namespace MetaTools;

#pragma pack(1)

namespace StaticTools
{
class Environment
{
    public:
    static constexpr const char *FilePath = __FILE__;

    public:
    static constexpr const char *FileName = _FILE_NAME_;

    public:
    static constexpr int CharMin = std::numeric_limits<char>::min();
    static constexpr int CharMax = std::numeric_limits<char>::max();

    protected:
    static const bool ChkBigEndian()
    {
        const int value = 0x12345678;
        const char *ptr = (char *)&value;

        // return ptr[0] == 0x12 && ptr[1] == 0x34 && ptr[2] == 0x56 && ptr[3] == 0x78;
        return ptr[0] == 0x12;
    }

    public:
    static const bool IsBigEndian;
};

    const bool Environment::IsBigEndian = Environment::ChkBigEndian();
}

#pragma pack()

#endif