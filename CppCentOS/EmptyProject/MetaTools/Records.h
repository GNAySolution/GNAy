#ifndef GNAY_META_TOOLS_RECORDS_H_
#define GNAY_META_TOOLS_RECORDS_H_

#include "CompileArgs.h"

#pragma pack(1)

namespace MetaTools
{
    // #define UNUSED(x) (void)(x)

    #ifndef NAME_MAX
    #define NAME_MAX 255
    #endif

    #define STR(N) #N
    #define XSTR(N) STR(N)

    #define _LINE_STR_ XSTR(__LINE__)

class LogLevel
{
    public:
    enum Enum
    {
        Trace = 0,
        Debug = Trace + 1 == 1 ? 1 : throw std::invalid_argument(""),
        Info = Debug + 1 == 2 ? 2 : throw std::invalid_argument(""),
        Warn = Info + 1 == 3 ? 3 : throw std::invalid_argument(""),
        Error = Warn + 1 == 4 ? 4 : throw std::invalid_argument(""),
    };

    public:
    static constexpr const char (&TraceStr)[sizeof("TRACE")] = "TRACE";
    static constexpr const char (&DebugStr)[sizeof("DEBUG")] = "DEBUG";
    static constexpr const char (&InfoStr)[sizeof("INFO")] = "INFO";
    static constexpr const char (&WarnStr)[sizeof("WARN")] = "WARN";
    static constexpr const char (&ErrorStr)[sizeof("ERROR")] = "ERROR";

    public:
    static constexpr Enum All[] = {Trace, Debug, Info, Warn, Error};
    static constexpr const char *StrCollection[] = {TraceStr, DebugStr, InfoStr, WarnStr, ErrorStr};

    public:
    static constexpr int EnumFirst = Trace == 0 ? 0 : throw std::invalid_argument("");
    static constexpr int EnumCnt = sizeof(All) / sizeof(int) == 5 ? 5 : throw std::logic_error("");

    protected:
    static constexpr bool _unitTestResult1 = sizeof(StrCollection) / sizeof(char *) == EnumCnt ? true : throw std::logic_error("");
    static constexpr bool _unitTestResult2 = All[EnumFirst] == Trace ? true : throw std::invalid_argument("");
    static constexpr bool _unitTestResult3 = All[EnumCnt - 1] == Error ? true : throw std::invalid_argument("");
    static constexpr bool _unitTestResult4 = StrCollection[EnumFirst] == TraceStr ? true : throw std::invalid_argument("");
    static constexpr bool _unitTestResult5 = StrCollection[EnumCnt - 1] == ErrorStr ? true : throw std::invalid_argument("");
};

    constexpr LogLevel::Enum LogLevel::All[];
    constexpr const char *LogLevel::StrCollection[];

template<typename T, long long N = std::numeric_limits<T>::max()>
struct TNumericWidthMax
{
    enum
    {
        StrLength = TNumericWidthMax<T, N / 10>::StrLength + ((N / 10 == 0 && std::numeric_limits<T>::min() >= 0) ? 0 : 1),
        CArrSize = TNumericWidthMax<T, N / 10>::CArrSize + ((N / 10 == 0 && std::numeric_limits<T>::min() >= 0) ? 0 : 1),
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
    static constexpr bool _chkUnitTest = _unitTestResult ? true : throw std::logic_error("");
};

template<typename T, unsigned int N>
struct TArray
{
    static constexpr unsigned int DTSize = sizeof(T);
    static constexpr unsigned int DASize = N;
    static constexpr unsigned int DMSize = sizeof(T) * N;

    T Data[N] = {0};
};

    template<typename T, unsigned int N>
    constexpr unsigned int TArray<T, N>::DASize;

template<typename T, unsigned int N>
struct TArrModel: TArray<T, N>
{
    virtual T *Reset(const int& value = 0)
    {
        return (T *)memset(this->Data, value, N);
    }
};

template<unsigned int N>
struct CharArray: TArrModel<char, N>
{
    int Length = -1;

    virtual char *Reset(const int& value = 0) override
    {
        Length = -1;

        return (char *)memset(this->Data, value, N);
    }
};

struct FPathArr: CharArray<PATH_MAX>
{
};

struct FNameArr: CharArray<NAME_MAX>
{
};

struct HostNameArr: CharArray<128>
{
};

template<typename T>
struct TNumericStrRecord
{
    T Value;
    struct CharArray<TNumericWidthMax<T>::CArrSize> ValueStr;

    TNumericStrRecord() = default;
    TNumericStrRecord(const T& value, const char *format = "%lld")
    {
        Value = value;
        ValueStr.Length = snprintf(ValueStr.Data, ValueStr.DASize, format, value);
    }
};

struct BoolStrRecord: TNumericStrRecord<bool>
{
    BoolStrRecord() = default;
    BoolStrRecord(const bool& value, const char *format = "%d"): TNumericStrRecord<bool>(value, format) {}
};

struct ByteStrRecord: TNumericStrRecord<unsigned char>
{
    ByteStrRecord() = default;
    ByteStrRecord(const unsigned char& value, const char *format = "%c"): TNumericStrRecord<unsigned char>(value, format) {}
};

struct ShortStrRecord: TNumericStrRecord<short>
{
    ShortStrRecord() = default;
    ShortStrRecord(const short& value, const char *format = "%h"): TNumericStrRecord<short>(value, format) {}
};

struct IntStrRecord: TNumericStrRecord<int>
{
    IntStrRecord() = default;
    IntStrRecord(const int& value, const char *format = "%d"): TNumericStrRecord<int>(value, format) {}
};

struct LongStrRecord: TNumericStrRecord<long long>
{
    LongStrRecord() = default;
    LongStrRecord(const long long& value, const char *format = "%lld"): TNumericStrRecord<long long>(value, format) {}
};

struct LogTimeBuffer: CharArray<sizeof("HH:mm:ss.ffffff")> //"%H:%M:%S."
{
};

struct LogRecord
{
    struct timeval CreatedTime;
    LogLevel::Enum Level = LogLevel::Error;
    struct LongStrRecord ThreadID;
    struct IntStrRecord ThreadSeq;
    unsigned int LogSeq = 0;

    // fprintf(_fptr[i], "%06d|%s|+%06ld|%5s|%02d|ML=%d|%s\n", record->LogSeq, TimeExt::GetHHmmssffffff(record->CreatedTime), elu, LogLevel::StrCollection[record->Level], record->ThreadSeq.Value, record->MsgLength, record->Msg);
    static constexpr int MsgArrSize = CompileArgs::LogBufSize - LogTimeBuffer::DASize * 2 - sizeof(LogLevel::Enum) - sizeof(struct LongStrRecord) - sizeof(struct IntStrRecord) - sizeof(unsigned int) - sizeof(int);
    struct CharArray<MsgArrSize> Msg;
};

template<typename T, T... Args>
struct ConstTArray
{
    static constexpr unsigned int DTSize = sizeof(T);
    static constexpr unsigned int DASize = (sizeof... (Args));
    static constexpr unsigned int DMSize = sizeof(T) * (sizeof... (Args));

    const T Data[sizeof... (Args)] = {Args...};
};

    template<typename T, T... Args>
    constexpr unsigned int ConstTArray<T, Args...>::DASize;

template<char... Args>
struct ConstCharArray: ConstTArray<char, Args...>
{
    int Length = (sizeof... (Args)) - 1;
};

    constexpr char UpperCase[26] = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
    constexpr char LowerCase[26] = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};
    constexpr char LowerUpperDistance = 'a' - 'A';

    constexpr char HexdecimalCharacters[16] = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
}

#pragma pack()

#endif
