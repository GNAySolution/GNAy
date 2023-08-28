#ifndef _META_TOOLS_FUNCTIONS_H
#define _META_TOOLS_FUNCTIONS_H

#pragma pack(1)

namespace MetaTools
{
class Functions
{
    public:
    static constexpr int GetLength(const char *str)
    {
        return *str ? 1 + GetLength(str + 1) : 0;
    }

    protected:
    static constexpr const char *GetFileName(const char *fullPath, const int& pos)
    {
        return pos <= 0 ? fullPath : (fullPath[pos] == '/' || fullPath[pos] == '\\') ? fullPath + pos + 1 : GetFileName(fullPath, pos - 1);
    }

    public:
    static constexpr const char *GetFileName(const char *fullPath)
    {
        return GetFileName(fullPath, GetLength(fullPath) - 1);
    }
};

class MetaFunc: public Functions
{
    public:
    static constexpr const char *FilePath = __FILE__;

    public:
    static constexpr const char *FileName = GetFileName(__FILE__);
};

    #define _FILE_NAME_ MetaFunc::GetFileName(__FILE__)
}

#pragma pack()

#endif