#ifndef _META_TOOLS_RECORDS_H
#define _META_TOOLS_RECORDS_H

#include "BaseInclude.h"

#pragma pack(1)

namespace MetaTools
{
class LogLevel
{
    public:
    enum
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
    };

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
    static constexpr const char *EnumStr[] = {TraceStr, DebugStr, InfoStr, WarnStr, ErrorStr}; //Error + 1
};

    constexpr const char *LogLevel::EnumStr[];

template<typename T, long long N = std::numeric_limits<T>::max()>
class TNumericWidthMax
{
    public:
    enum
    {
        StrLength = TNumericWidthMax<T, N / 10>::StrLength + (N / 10 == 0 && std::numeric_limits<T>::min() >= 0 ? 0 : 1),
        ArraySize = TNumericWidthMax<T, N / 10>::ArraySize + (N / 10 == 0 && std::numeric_limits<T>::min() >= 0 ? 0 : 1),
    };
};

template<typename T>
class TNumericWidthMax<T, 0>
{
    public:
    enum
    {
        StrLength = 1,
        ArraySize = 1 + 1,
    };
};

template<long long N>
class TNumericWidth
{
    public:
    static constexpr long long RawValue = N;

    public:
    enum
    {
        StrLength = TNumericWidthMax<long long, N>::StrLength + (N > 0 ? -1 : 0),
        ArraySize = TNumericWidthMax<long long, N>::ArraySize + (N > 0 ? -1 : 0) + 1,
    };
};

class NumericWidthMax
{
    public:
    static constexpr int BoolStrLength = TNumericWidthMax<bool>::StrLength == 1 ? 1 : -1; //0 ~ 1
    static constexpr int BoolArraySize = TNumericWidthMax<bool>::ArraySize == 2 ? 2 : -1;
    static constexpr int CharStrLength = TNumericWidthMax<signed char>::StrLength == 4 ? 4 : -1; //-128 ~ 127
    static constexpr int CharArraySize = TNumericWidthMax<signed char>::ArraySize == 5 ? 5 : -1;
    static constexpr int ShortStrLength = TNumericWidthMax<short>::StrLength == 6 ? 6 : -1; //-32768 ~ 32767
    static constexpr int ShortArraySize = TNumericWidthMax<short>::ArraySize == 7 ? 7 : -1;
    static constexpr int IntStrLength = TNumericWidthMax<int>::StrLength == 11 ? 11 : -1; //-2147483648 ~ 2147483647
    static constexpr int IntArraySize = TNumericWidthMax<int>::ArraySize == 12 ? 12 : -1;
    static constexpr int LongStrLength = TNumericWidthMax<long long>::StrLength == 20 ? 20 : -1; //-9223372036854775808 ~ 9223372036854775807
    static constexpr int LongArraySize = TNumericWidthMax<long long>::ArraySize == 21 ? 21 : -1;

    protected:
    static constexpr TNumericWidth<-123456789> _nm123456789 {};
    static constexpr TNumericWidth<-10> _nm10 {};
    static constexpr TNumericWidth<-9> _nm9 {};
    static constexpr TNumericWidth<-1> _nm1 {};
    static constexpr TNumericWidth<0> _n0 {};
    static constexpr TNumericWidth<1> _n1 {};
    static constexpr TNumericWidth<9> _n9 {};
    static constexpr TNumericWidth<10> _n10 {};
    static constexpr TNumericWidth<123456789> _n123456789 {};

    public:
    static constexpr bool UnitTestResult = BoolStrLength > 0 &&
                                            BoolArraySize > 0 &&
                                            CharStrLength > 0 &&
                                            CharArraySize > 0 &&
                                            ShortStrLength > 0 &&
                                            ShortArraySize > 0 &&
                                            IntStrLength > 0 &&
                                            IntArraySize > 0 &&
                                            LongStrLength > 0 &&
                                            LongArraySize > 0 &&
                                            _nm123456789.StrLength == 10 &&
                                            _nm10.StrLength == 3 &&
                                            _nm9.StrLength == 2 &&
                                            _nm1.StrLength == 2 &&
                                            _n0.StrLength == 1 &&
                                            _n1.StrLength == 1 &&
                                            _n9.StrLength == 1 &&
                                            _n10.StrLength == 2 &&
                                            _n123456789.StrLength == 9;

    public:
    static void CheckEachUnitTest(const bool& errorTest = false)
    {
        if (UnitTestResult && !errorTest)
        {
            return;
        }

        if (BoolStrLength <= 0 || errorTest)
        {
            printf("%s|BoolStrLength=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<bool>::StrLength);
        }

        if (BoolArraySize <= 0 || errorTest)
        {
            printf("%s|BoolArraySize=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<bool>::ArraySize);
        }

        if (CharStrLength <= 0 || errorTest)
        {
            printf("%s|CharStrLength=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<signed char>::StrLength);
        }

        if (CharArraySize <= 0 || errorTest)
        {
            printf("%s|CharArraySize=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<signed char>::ArraySize);
        }

        if (ShortStrLength <= 0 || errorTest)
        {
            printf("%s|ShortStrLength=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<short>::StrLength);
        }

        if (ShortArraySize <= 0 || errorTest)
        {
            printf("%s|ShortArraySize=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<short>::ArraySize);
        }

        if (IntStrLength <= 0 || errorTest)
        {
            printf("%s|IntStrLength=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<int>::StrLength);
        }

        if (IntArraySize <= 0 || errorTest)
        {
            printf("%s|IntArraySize=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<int>::ArraySize);
        }

        if (LongStrLength <= 0 || errorTest)
        {
            printf("%s|LongStrLength=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<long long>::StrLength);
        }

        if (LongArraySize <= 0 || errorTest)
        {
            printf("%s|LongArraySize=%d|\n", LogLevel::ErrorStr, TNumericWidthMax<long long>::ArraySize);
        }

        if (_nm123456789.StrLength != 10 || errorTest)
        {
            printf("%s|_nm123456789.StrLength=%d|\n", LogLevel::ErrorStr, _nm123456789.StrLength);
        }

        if (_nm10.StrLength != 3 || errorTest)
        {
            printf("%s|_nm10.StrLength=%d|\n", LogLevel::ErrorStr, _nm10.StrLength);
        }

        if (_nm9.StrLength != 2 || errorTest)
        {
            printf("%s|_nm9.StrLength=%d|\n", LogLevel::ErrorStr, _nm9.StrLength);
        }

        if (_nm1.StrLength != 2 || errorTest)
        {
            printf("%s|_nm1.StrLength=%d|\n", LogLevel::ErrorStr, _nm1.StrLength);
        }

        if (_n0.StrLength != 1 || errorTest)
        {
            printf("%s|_n0.StrLength=%d|\n", LogLevel::ErrorStr, _n0.StrLength);
        }

        if (_n1.StrLength != 1 || errorTest)
        {
            printf("%s|_n1.StrLength=%d|\n", LogLevel::ErrorStr, _n1.StrLength);
        }

        if (_n9.StrLength != 1 || errorTest)
        {
            printf("%s|_n9.StrLength=%d|\n", LogLevel::ErrorStr, _n9.StrLength);
        }

        if (_n10.StrLength != 2 || errorTest)
        {
            printf("%s|_n10.StrLength=%d|\n", LogLevel::ErrorStr, _n10.StrLength);
        }

        if (_n123456789.StrLength != 9 || errorTest)
        {
            printf("%s|_n123456789.StrLength=%d|\n", LogLevel::ErrorStr, _n123456789.StrLength);
        }
    }
};

template<typename T, unsigned int N>
struct TArray
{
    static constexpr int Size = N;

    T Data[N];
};

template<unsigned int N>
struct TCharArray: TArray<char, N>
{
    char Data[N] = {0};
};

template<typename T, T... Args>
struct TConstArray
{
    const int Size = (sizeof... (Args));
    const T Data[sizeof... (Args)] = {Args...};
};

template<char... Args>
struct TString: TConstArray<char, Args...>
{
    int Length = (sizeof... (Args)) - 1;
};

    constexpr char HexdecimalCharacters[] = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
}

#pragma pack()

#endif