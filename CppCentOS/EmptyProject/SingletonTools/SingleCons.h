#ifndef _SINGLETON_TOOLS_SINGLE_CONS_H
#define _SINGLETON_TOOLS_SINGLE_CONS_H

#include "../MetaTools/Functions.h"

#pragma pack(1)

namespace SingletonTools
{
class SingleCons
{
    public:
    static constexpr const char *FilePath = __FILE__;

    public:
    static constexpr const char *FileName = _FILE_NAME_;

    protected:
    struct Constructor
    {
        Constructor()
        {
            printf("SingleCons is initialized.|%s|%s|%d|%s|%s|\n\n", __DATE__, __TIME__, __LINE__, __FUNCTION__, __BASE_FILE__);
        }
    };

    protected:
    static const Constructor Cons;
};

    const struct SingleCons::Constructor SingleCons::Cons;
}

#pragma pack()

#endif