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
    enum Enum
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
    static constexpr int EnumCnt = sizeof(All) / sizeof(int) == 6 ? 6 : throw std::logic_error("");

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
                                            MetaFunc::StrEqual(CRC32CARR(_cArrEmpty)::Data, "0x00000000");

    protected:
    static constexpr bool _chkCArrSpace = CRC32CARR(_cArrSpace)::DataArrSize == _cArrSize &&
                                            CRC32CARR(_cArrSpace)::DataStrLength == _strLen &&
                                            MetaFunc::StrEqual(CRC32CARR(_cArrSpace)::Data, "0xE96CCF45");

    protected:
    static constexpr bool _chkCArr0 = CRC32CARR(_cArr0)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr0)::DataStrLength == _strLen &&
                                        MetaFunc::StrEqual(CRC32CARR(_cArr0)::Data, "0xF4DBDF21");

    protected:
    static constexpr bool _chkCArr1 = CRC32CARR(_cArr1)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr1)::DataStrLength == _strLen &&
                                        MetaFunc::StrEqual(CRC32CARR(_cArr1)::Data, "0x83DCEFB7");

    protected:
    static constexpr bool _chkCArr2 = CRC32CARR(_cArr2)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr2)::DataStrLength == _strLen &&
                                        MetaFunc::StrEqual(CRC32CARR(_cArr2)::Data, "0x1AD5BE0D");

    protected:
    static constexpr bool _chkCArr9 = CRC32CARR(_cArr9)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr9)::DataStrLength == _strLen &&
                                        MetaFunc::StrEqual(CRC32CARR(_cArr9)::Data, "0x8D076785");

    protected:
    static constexpr bool _chkCArr10 = CRC32CARR(_cArr10)::DataArrSize == _cArrSize &&
                                        CRC32CARR(_cArr10)::DataStrLength == _strLen &&
                                        MetaFunc::StrEqual(CRC32CARR(_cArr10)::Data, "0xA15D25E1");

    protected:
    static constexpr bool _chkCArr123456789 = CRC32CARR(_cArr123456789)::DataArrSize == _cArrSize &&
                                                CRC32CARR(_cArr123456789)::DataStrLength == _strLen &&
                                                MetaFunc::StrEqual(CRC32CARR(_cArr123456789)::Data, "0xCBF43926");

    protected:
    static constexpr bool _chkCArrCrcVal01 = CRC32CARR(_cArrCrcVal01)::DataArrSize == _cArrSize &&
                                                CRC32CARR(_cArrCrcVal01)::DataStrLength == _strLen &&
                                                MetaFunc::StrEqual(CRC32CARR(_cArrCrcVal01)::Data, "0xCC3B0811");

    protected:
    static constexpr bool _chkCArrCrcVal02 = CRC32CARR(_cArrCrcVal02)::DataArrSize == _cArrSize &&
                                                CRC32CARR(_cArrCrcVal02)::DataStrLength == _strLen &&
                                                MetaFunc::StrEqual(CRC32CARR(_cArrCrcVal02)::Data, "0x553259AB");

    // protected:
    // static constexpr bool _chkCArrFile = CRC32CARR(_cArrFile)::DataArrSize == _cArrSize &&
    //                                         CRC32CARR(_cArrFile)::DataStrLength == _strLen &&
    //                                         MetaFunc::StrEqual(CRC32CARR(_cArrFile)::Data, "0x20C04CA9");
    //C:\MegaSolutions\Cpp11UnitTest01\MetaTools\UnitTest.h
    //0x4700785B
    //C:\MegaSolutions\Cpp11UnitTest01\MetaTools/UnitTest.h
    //0x20C04CA9

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