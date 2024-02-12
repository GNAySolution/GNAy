#ifndef GNAY_META_TOOLS_UNITTEST_H_
#define GNAY_META_TOOLS_UNITTEST_H_

#include "Functions.h"

#pragma pack(1)

namespace MetaTools
{
#if RUN_TEST
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