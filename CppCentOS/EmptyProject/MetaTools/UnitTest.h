#ifndef GNAY_META_TOOLS_UNITTEST_H_
#define GNAY_META_TOOLS_UNITTEST_H_

#include "Functions.h"

#pragma pack(1)

namespace MetaTools
{
#if RUN_TEST
class FileShare
{
    public:
    enum Enum: unsigned char
    {
        None = 0, //0
        Read = None + 1 == 1 ? 1 : throw std::invalid_argument(""), //1
        Write = Read + 1 == 1 << 1 ? 1 << 1 : throw std::invalid_argument(""), //2
        ReadWrite = (Read | Write) == 3 ? 3 : throw std::invalid_argument(""), //3
        Delete = Write + 2 == 1 << 2 ? 1 << 2 : throw std::invalid_argument(""), //4
        Inheritable = Delete + 12 == 1 << 4 ? 1 << 4 : throw std::invalid_argument(""), //16
    };

    public:
    static constexpr const char (&NoneStr)[sizeof("None")] = "None";
    static constexpr const char (&ReadStr)[sizeof("Read")] = "Read";
    static constexpr const char (&WriteStr)[sizeof("Write")] = "Write";
    static constexpr const char (&ReadWriteStr)[sizeof("ReadWrite")] = "ReadWrite";
    static constexpr const char (&DeleteStr)[sizeof("Delete")] = "Delete";
    static constexpr const char (&InheritableStr)[sizeof("Inheritable")] = "Inheritable";

    public:
    static constexpr Enum All[] = {None, Read, Write, ReadWrite, Delete, Inheritable};
    static constexpr const char *StrCollection[] = {NoneStr, ReadStr, WriteStr, ReadWriteStr, DeleteStr, InheritableStr};

    public:
    static constexpr int EnumFirst = None == 0 ? 0 : throw std::invalid_argument("");
    static constexpr int EnumCnt = sizeof(All) / sizeof(Enum) == 6 ? 6 : throw std::logic_error("");

    protected:
    static constexpr bool _unitTestResult1 = Write == 2 && Delete == 4 && Inheritable == 16 ? true : throw std::invalid_argument("");
    static constexpr bool _unitTestResult2 = sizeof(StrCollection) / sizeof(char *) == EnumCnt ? true : throw std::logic_error("");
    static constexpr bool _unitTestResult3 = All[EnumFirst] == None ? true : throw std::invalid_argument("");
    static constexpr bool _unitTestResult4 = All[EnumCnt - 1] == Inheritable ? true : throw std::invalid_argument("");
    static constexpr bool _unitTestResult5 = StrCollection[EnumFirst] == NoneStr ? true : throw std::invalid_argument("");
    static constexpr bool _unitTestResult6 = StrCollection[EnumCnt - 1] == InheritableStr ? true : throw std::invalid_argument("");
};

    constexpr FileShare::Enum FileShare::All[];
    constexpr const char *FileShare::StrCollection[];

class SizeofTArray
{
    protected:
    template<unsigned int N>
    struct EmptyRec
    {
        static constexpr int Value[N] = {0};

        const int GetValue() const { return Value[0]; }
    };

    struct EmptyRec5: EmptyRec<5>
    {
        virtual const int GetValue2() const { return Value[0]; }
    };

    protected:
    static struct EmptyRec<10> _e10;
    static int _intArrA5[5];
    static struct TArray<long long, 3> _llArrB3;
    static struct FNameArr _fNameRec;
    static struct ConstTArray<int, 1, 2, 3, 4, 5> _intArrD5;

    protected:
    static constexpr int _memorySizeEmpty10 = (sizeof(EmptyRec<10>) == 1 && sizeof(_e10) == 1) ? 1 : throw std::logic_error("");
    static constexpr int _memorySizeEmpty5 = sizeof(EmptyRec5) == 8 ? 8 : throw std::logic_error("");

    protected:
    static constexpr int _typeSizeArrA5 = Functions::GetTypeSize(_intArrA5) == sizeof(int) ? Functions::GetTypeSize(_intArrA5) : throw std::logic_error("");
    static constexpr int _arrSizeArrA5 = Functions::GetArrSize(_intArrA5) == sizeof(_intArrA5) / sizeof(int) ? Functions::GetArrSize(_intArrA5) : throw std::logic_error("");
    static constexpr int _memorySizeArrA5 = Functions::GetMemorySize(_intArrA5) == sizeof(_intArrA5) ? Functions::GetMemorySize(_intArrA5) : throw std::logic_error("");

    protected:
    static constexpr int _typeSizeArrB3 = (_llArrB3.DTSize == sizeof(long long) && Functions::GetTypeSize(_llArrB3.Data) == sizeof(long long)) ? _llArrB3.DTSize : throw std::logic_error("");
    static constexpr int _arrSizeArrB3 = (_llArrB3.DASize == sizeof(_llArrB3) / sizeof(long long) && Functions::GetArrSize(_llArrB3.Data) == sizeof(_llArrB3) / sizeof(long long)) ? _llArrB3.DASize : throw std::logic_error("");
    static constexpr int _memorySizeArrB3 = (_llArrB3.DMSize == sizeof(_llArrB3) && sizeof(_llArrB3.Data) == sizeof(_llArrB3) && Functions::GetMemorySize(_llArrB3.Data) == sizeof(_llArrB3)) ? _llArrB3.DMSize : throw std::logic_error("");

    protected:
    static constexpr int _arrSizeFName = _fNameRec.DASize == _fNameRec.DMSize / _fNameRec.DTSize ? _fNameRec.DASize : throw std::logic_error("");
    static constexpr int _memorySizeFName = _fNameRec.DMSize + 8 + 4 == sizeof(_fNameRec) ? _fNameRec.DMSize + 8 + 4 : throw std::logic_error("");

    protected:
    static constexpr int _typeSizeArrD5 = (_intArrD5.DTSize == sizeof(int) && Functions::GetTypeSize(_intArrD5.Data) == sizeof(int)) ? _intArrD5.DTSize : throw std::logic_error("");
    static constexpr int _arrSizeArrD5 = (_intArrD5.DASize == sizeof(_intArrD5) / sizeof(int) && Functions::GetArrSize(_intArrD5.Data) == sizeof(_intArrD5) / sizeof(int)) ? _intArrD5.DASize : throw std::logic_error("");
    static constexpr int _memorySizeArrD5 = (_intArrD5.DMSize == sizeof(_intArrD5) && sizeof(_intArrD5.Data) == sizeof(_intArrD5) && Functions::GetMemorySize(_intArrD5.Data) == sizeof(_intArrD5)) ? _intArrD5.DMSize : throw std::logic_error("");
};

class CRC32CompileTimeTest
{
    protected:
    class Hexdecimal0: public HexdecimalCharArray<0>
    {
        friend class CRC32CompileTimeTest;
    };

    protected:
    static constexpr int _cArrSize = Hexdecimal0::_cArrSize;
    static constexpr int _strLen = Hexdecimal0::_strLen;

    protected:
    #define _cArrEmpty ""
    #define _cArrSpace " "
    #define _cArr0 "0"
    #define _cArr1 "1"
    #define _cArr2 "2"
    #define _cArr9 "9"
    #define _cArr10 "10"
    #define _cArr123456789 "123456789"
    #define _cArrCrcVal01 "CrcVal01"
    #define _cArrCrcVal02 "CrcVal02"
    // #define _cArrFile __FILE__

    protected:
    static constexpr bool _chkCArrEmpty = CRC32CARR(_cArrEmpty)::DataArrSize == _cArrSize &&
                                            CRC32CARR(_cArrEmpty)::DataStrLength == _strLen &&
                                            Functions::StrEqual(CRC32CARR(_cArrEmpty)::Data, "0x00000000");

    protected:
    static constexpr bool _chkCArrSpace = CRC32CARR(_cArrSpace)::DataArrSize == _cArrSize &&
                                            CRC32CARR(_cArrSpace)::DataStrLength == _strLen &&
                                            Functions::StrEqual(CRC32CARR(_cArrSpace)::Data, "0xE96CCF45");

    protected:
    static constexpr bool _chkCArr0 = CRC32CARR(_cArr0)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr0)::DataStrLength == _strLen &&
                                        Functions::StrEqual(CRC32CARR(_cArr0)::Data, "0xF4DBDF21");

    protected:
    static constexpr bool _chkCArr1 = CRC32CARR(_cArr1)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr1)::DataStrLength == _strLen &&
                                        Functions::StrEqual(CRC32CARR(_cArr1)::Data, "0x83DCEFB7");

    protected:
    static constexpr bool _chkCArr2 = CRC32CARR(_cArr2)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr2)::DataStrLength == _strLen &&
                                        Functions::StrEqual(CRC32CARR(_cArr2)::Data, "0x1AD5BE0D");

    protected:
    static constexpr bool _chkCArr9 = CRC32CARR(_cArr9)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr9)::DataStrLength == _strLen &&
                                        Functions::StrEqual(CRC32CARR(_cArr9)::Data, "0x8D076785");

    protected:
    static constexpr bool _chkCArr10 = CRC32CARR(_cArr10)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr10)::DataStrLength == _strLen &&
                                        Functions::StrEqual(CRC32CARR(_cArr10)::Data, "0xA15D25E1");

    protected:
    static constexpr bool _chkCArr123456789 = CRC32CARR(_cArr123456789)::DataArrSize == _cArrSize &&
                                                CRC32CARR(_cArr123456789)::DataStrLength == _strLen &&
                                                Functions::StrEqual(CRC32CARR(_cArr123456789)::Data, "0xCBF43926");

    protected:
    static constexpr bool _chkCArrCrcVal01 = CRC32CARR(_cArrCrcVal01)::DataArrSize == _cArrSize &&
                                                CRC32CARR(_cArrCrcVal01)::DataStrLength == _strLen &&
                                                Functions::StrEqual(CRC32CARR(_cArrCrcVal01)::Data, "0xCC3B0811");

    protected:
    static constexpr bool _chkCArrCrcVal02 = CRC32CARR(_cArrCrcVal02)::DataArrSize == _cArrSize &&
                                                CRC32CARR(_cArrCrcVal02)::DataStrLength == _strLen &&
                                                Functions::StrEqual(CRC32CARR(_cArrCrcVal02)::Data, "0x553259AB");

    protected:
    static constexpr bool _unitTestResult = _chkCArrEmpty &&
                                            _chkCArrSpace &&
                                            _chkCArr0 &&
                                            _chkCArr1 &&
                                            _chkCArr2 &&
                                            _chkCArr9 &&
                                            _chkCArr10 &&
                                            _chkCArr123456789 &&
                                            _chkCArrCrcVal01 &&
                                            _chkCArrCrcVal02; //&&
                                            // _chkCArrFile;

    protected:
    static constexpr bool _chkUnitTest = _unitTestResult ? true : throw std::logic_error("");
};
#endif
}

#pragma pack()

#endif