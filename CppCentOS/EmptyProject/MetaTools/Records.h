#ifndef _META_TOOLS_RECORDS_H
#define _META_TOOLS_RECORDS_H

#include "BaseInclude.h"
#include "CompileArgs.h"

#pragma pack(1)

namespace MetaTools
{
    #define STR(N) #N
    #define XSTR(N) STR(N)

    #define _LINE_STR_ XSTR(__LINE__)

class LogLevel
{
    public:
    enum Enum
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
    };

    public:
    static constexpr int EnumCnt = Error + 1 == 5 ? 5 : throw std::logic_error("");

    protected:
    #define _traceStr "TRACE"
    #define _debugStr "DEBUG"
    #define _infoStr "INFO"
    #define _warnStr "WARN"
    #define _errorStr "ERROR"

    public:
    static constexpr const char (&TraceStr)[sizeof(_traceStr)] = _traceStr;
    static constexpr const char (&DebugStr)[sizeof(_debugStr)] = _debugStr;
    static constexpr const char (&InfoStr)[sizeof(_infoStr)] = _infoStr;
    static constexpr const char (&WarnStr)[sizeof(_warnStr)] = _warnStr;
    static constexpr const char (&ErrorStr)[sizeof(_errorStr)] = _errorStr;

    public:
    static constexpr const char *EnumStr[EnumCnt] = {TraceStr, DebugStr, InfoStr, WarnStr, ErrorStr}; //Error + 1
};

    constexpr const char *LogLevel::EnumStr[];

template<typename T, long long N = std::numeric_limits<T>::max()>
struct TNumericWidthMax
{
    enum
    {
        StrLength = TNumericWidthMax<T, N / 10>::StrLength + (N / 10 == 0 && std::numeric_limits<T>::min() >= 0 ? 0 : 1),
        CArrSize = TNumericWidthMax<T, N / 10>::CArrSize + (N / 10 == 0 && std::numeric_limits<T>::min() >= 0 ? 0 : 1),
    };
};

template<typename T>
struct TNumericWidthMax<T, 0>
{
    enum
    {
        StrLength = 1,
        CArrSize = 1 + 1,
    };
};

template<long long N>
struct NumericWidth
{
    static constexpr long long RawValue = N;

    enum
    {
        StrLength = TNumericWidthMax<long long, N>::StrLength + (N > 0 ? -1 : 0),
        CArrSize = TNumericWidthMax<long long, N>::CArrSize + (N > 0 ? -1 : 0) + 1,
    };
};

class NumericWidthMax
{
    public:
    static constexpr int BoolStrLength = TNumericWidthMax<bool>::StrLength == 1 ? 1 : throw std::logic_error(""); //0 ~ 1
    static constexpr int BoolCArrSize = TNumericWidthMax<bool>::CArrSize == 2 ? 2 : throw std::logic_error("");
    static constexpr int CharStrLength = TNumericWidthMax<signed char>::StrLength == 4 ? 4 : throw std::logic_error(""); //-128 ~ 127
    static constexpr int CharCArrSize = TNumericWidthMax<signed char>::CArrSize == 5 ? 5 : throw std::logic_error("");
    static constexpr int ShortStrLength = TNumericWidthMax<short>::StrLength == 6 ? 6 : throw std::logic_error(""); //-32768 ~ 32767
    static constexpr int ShortCArrSize = TNumericWidthMax<short>::CArrSize == 7 ? 7 : throw std::logic_error("");
    static constexpr int IntStrLength = TNumericWidthMax<int>::StrLength == 11 ? 11 : throw std::logic_error(""); //-2147483648 ~ 2147483647
    static constexpr int IntCArrSize = TNumericWidthMax<int>::CArrSize == 12 ? 12 : throw std::logic_error("");
    static constexpr int LongStrLength = TNumericWidthMax<long long>::StrLength == 20 ? 20 : throw std::logic_error(""); //-9223372036854775808 ~ 9223372036854775807
    static constexpr int LongCArrSize = TNumericWidthMax<long long>::CArrSize == 21 ? 21 : throw std::logic_error("");

    protected:
    static constexpr NumericWidth<-123456789> _nm123456789 {};
    static constexpr NumericWidth<-10> _nm10 {};
    static constexpr NumericWidth<-9> _nm9 {};
    static constexpr NumericWidth<-1> _nm1 {};
    static constexpr NumericWidth<0> _n0 {};
    static constexpr NumericWidth<1> _n1 {};
    static constexpr NumericWidth<9> _n9 {};
    static constexpr NumericWidth<10> _n10 {};
    static constexpr NumericWidth<123456789> _n123456789 {};

    protected:
    static constexpr bool _unitTestResult = BoolStrLength > 0 &&
                                            BoolCArrSize > 0 &&
                                            CharStrLength > 0 &&
                                            CharCArrSize > 0 &&
                                            ShortStrLength > 0 &&
                                            ShortCArrSize > 0 &&
                                            IntStrLength > 0 &&
                                            IntCArrSize > 0 &&
                                            LongStrLength > 0 &&
                                            LongCArrSize > 0 &&
                                            _nm123456789.StrLength == 10 &&
                                            _nm10.StrLength == 3 &&
                                            _nm9.StrLength == 2 &&
                                            _nm1.StrLength == 2 &&
                                            _n0.StrLength == 1 &&
                                            _n1.StrLength == 1 &&
                                            _n9.StrLength == 1 &&
                                            _n10.StrLength == 2 &&
                                            _n123456789.StrLength == 9;

    protected:
    static constexpr bool _chkUnitTest = _unitTestResult ? _unitTestResult : throw std::logic_error("");
};

template<typename T>
struct TNumericStrRecord
{
    T Value;
    char ValueStr[TNumericWidthMax<T>::CArrSize] = {0};
    int ValueStrLen = -1;

    TNumericStrRecord() {}

    TNumericStrRecord(const T& value, const char *format = "%lld")
    {
        Value = value;
        ValueStrLen = sprintf(ValueStr, format, value);
    }
};

struct BoolStrRecord: TNumericStrRecord<bool>
{
    BoolStrRecord() {}
    BoolStrRecord(const bool& value, const char *format = "%lld"): TNumericStrRecord<bool>(value, format) {}
};

struct ByteStrRecord: TNumericStrRecord<unsigned char>
{
    ByteStrRecord() {}
    ByteStrRecord(const unsigned char& value, const char *format = "%lld"): TNumericStrRecord<unsigned char>(value, format) {}
};

struct ShortStrRecord: TNumericStrRecord<short>
{
    ShortStrRecord() {}
    ShortStrRecord(const short& value, const char *format = "%lld"): TNumericStrRecord<short>(value, format) {}
};

struct IntStrRecord: TNumericStrRecord<int>
{
    IntStrRecord() {}
    IntStrRecord(const int& value, const char *format = "%lld"): TNumericStrRecord<int>(value, format) {}
};

struct LongStrRecord: TNumericStrRecord<long long>
{
    LongStrRecord() {}
    LongStrRecord(const long long& value, const char *format = "%lld"): TNumericStrRecord<long long>(value, format) {}
};

template<typename T, unsigned int N>
struct TArray
{
    static constexpr int ArrSize = N;

    T Data[N] = {0};
};

template<unsigned int N>
struct CharArray: TArray<char, N>
{
    int Length = -1;
};

struct HostNameArr: CharArray<128>
{
};

struct LogRecord
{
    struct timeval CreatedTime;
    LogLevel::Enum Level = LogLevel::Error;
    struct LongStrRecord ThreadID;
    struct IntStrRecord ThreadSeq;
    unsigned int LogSeq = 0;

    int MsgLength = -1;
    static constexpr int MsgArrSize = LogBufSize - sizeof("HH:mm:ss.ffffff") * 2 - sizeof(LogLevel::Enum) - sizeof(struct LongStrRecord) - sizeof(struct IntStrRecord) - sizeof(unsigned int) - sizeof(int);
    char Msg[MsgArrSize] = {0};
};

template<typename T, T... Args>
struct ConstTArray
{
    const int ArrSize = (sizeof... (Args));
    const T Data[sizeof... (Args)] = {Args...};
};

template<char... Args>
struct ConstCharArray: ConstTArray<char, Args...>
{
    int Length = (sizeof... (Args)) - 1;
};

    constexpr char UpperCase[26] = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
    constexpr char LowerCase[26] = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};
    constexpr char HexdecimalCharacters[16] = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
}

#pragma pack()

#endif