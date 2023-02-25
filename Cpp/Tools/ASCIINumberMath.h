#ifndef _TOOLS_ASCII_NUMBER_MATH_H
#define _TOOLS_ASCII_NUMBER_MATH_H

#include <cstdint>
#include <cstdio>
#include <cstring>

#pragma pack(1)

namespace Tools
{
    const int ASCIINumberMax = '9';
    const int ASCIINumberCntMax = ASCIINumberMax + 1;

    struct ASCIICharMathResult
    {
        char Result;
        bool HasNext;
    };

    class ASCIINumberMath
    {
        friend class ANMathStaticCons;

        protected:
            static struct ASCIICharMathResult CharPlusResults[ASCIINumberCntMax][ASCIINumberCntMax];
            static struct ASCIICharMathResult CharMinusResults[ASCIINumberCntMax][ASCIINumberCntMax];

            static void Reset(struct ASCIICharMathResult& rs)
            {
                rs.Result = ' ';
                rs.HasNext = false;
            }

            static bool ResetCharPlusResults()
            {
                const struct ASCIICharMathResult *rs = &CharPlusResults['5']['6']; //11=5+6

                if (rs->Result == '1' && rs->HasNext)
                {
                    return false;
                }

                for (int i = 0; i < ASCIINumberCntMax; ++i)
                {
                    for (int j = 0; j < ASCIINumberCntMax; ++j)
                    {
                        Reset(CharPlusResults[i][j]);

                        if (i >= '0' && i <= '9' && j >= '0' && j <= '9')
                        {
                            CharPlusResults[i][j].HasNext = Plus(i, j, CharPlusResults[i][j].Result);
                        }
                    }
                }

                return true;
            }

            static bool ResetCharMinusResults()
            {
                const struct ASCIICharMathResult *rs = &CharMinusResults['5']['6']; //9=5+10-6

                if (rs->Result == '9' && rs->HasNext)
                {
                    return false;
                }

                for (int i = 0; i < ASCIINumberCntMax; ++i)
                {
                    for (int j = 0; j < ASCIINumberCntMax; ++j)
                    {
                        Reset(CharMinusResults[i][j]);

                        if (i >= '0' && i <= '9' && j >= '0' && j <= '9')
                        {
                            CharMinusResults[i][j].HasNext = Minus(i, j, CharMinusResults[i][j].Result);
                        }
                    }
                }

                return true;
            }

            static const int IsLeftBigger(const char *unsignedValue1, const int& length1, const char *unsignedValue2, const int& length2)
            {
                if (length1 > length2)
                {
                    return 1;
                }
                else if (length1 < length2)
                {
                    return -1;
                }

                for (int i = 0; i < length1; ++i)
                {
                    if (unsignedValue1[i] > unsignedValue2[i])
                    {
                        return 1;
                    }
                    else if (unsignedValue1[i] < unsignedValue2[i])
                    {
                        return -1;
                    }
                }

                return 0; //The same.
            }

            static void TrimLeft(char *resultChars, const int& resultSize, int& pos, const bool& unsigned1, const bool& unsigned2, const int& isLeftBigger)
            {
                if (pos < 1)
                {
                    return;
                }

                for (int i = pos; i < resultSize - 2; ++i)
                {
                    if (resultChars[i] == '0')
                    {
                        resultChars[i] = ' ';
                        pos = i - 1;
                    }
                    else
                    {
                        pos = i;

                        break;
                    }
                }

                if (resultChars[pos] == '0' && pos == resultSize - 2)
                {
                    memset(resultChars, ' ', pos);

                    return;
                }
                else if (!unsigned1 && !unsigned2)
                {
                    if (pos > 0)
                    {
                        --pos;
                        resultChars[pos] = '-';
                    }
                }
                else if (!unsigned1 || !unsigned2)
                {
                    if (isLeftBigger == 1)
                    {
                        // if (unsigned1 && !unsigned2)
                        // {
                        // }
                        // else
                        if (!unsigned1 && unsigned2)
                        {
                            if (pos > 0)
                            {
                                --pos;
                                resultChars[pos] = '-';
                            }
                        }
                    }
                    else if (isLeftBigger == -1)
                    {
                        if (unsigned1 && !unsigned2)
                        {
                            if (pos > 0)
                            {
                                --pos;
                                resultChars[pos] = '-';
                            }
                        }
                        // else
                        // {
                        // }
                    }
                    // else //if (isLeftBigger == 0)
                    // {
                    // }
                }

                memset(resultChars, ' ', pos);
            }

            static const bool Plus(const char& value1, const char& value2, char& result)
            {
                result = value1 - '0' + value2;

                if (result <= '9')
                {
                    return false;
                }

                result -= 10;

                return true;
            }

            static const bool Plus(const uint8_t& value1, const uint8_t& value2, char& resultChar, const bool& plus1)
            {
                const struct ASCIICharMathResult *rs = &CharPlusResults[value1][value2];

                resultChar = rs->Result + plus1;

                return rs->HasNext;
            }

            static const bool Minus(const char& value1, const char& value2, char& result)
            {
                if (value1 >= value2)
                {
                    result = value1 - value2 + '0';

                    return false;
                }

                // result = value1 + 10 - value2 + '0';
                result = value1 + (10 + '0') - value2;

                return true;
            }

            static const bool Minus(uint8_t value1, const uint8_t& value2, char& resultChar, const bool& minus1)
            {
                bool hasNext = false;

                value1 -= minus1;

                if (value1 < '0')
                {
                    value1 = '9';
                    hasNext = true;
                }

                const struct ASCIICharMathResult *rs = &CharMinusResults[value1][value2];

                resultChar = rs->Result;

                return hasNext || rs->HasNext;
            }

        public:
            static const int Plus(const char *unsignedValue1, const int& length1, const char *unsignedValue2, const int& length2, char *resultChars, const int& resultSize)
            {
                resultChars[resultSize - 1] = 0;

                int pos1 = length1 - 1;
                int pos2 = length2 - 1;
                bool plus1 = false;

                for (int i = resultSize - 2; i >= 0; --i)
                {
                    if (pos1 >= 0) //&& unsignedValue1[pos1] >= '0' && unsignedValue1[pos1] <= '9')
                    {
                        if (pos2 >= 0) //&& unsignedValue2[pos2] >= '0' && unsignedValue2[pos2] <= '9')
                        {
                            plus1 = Plus(unsignedValue1[pos1], unsignedValue2[pos2], resultChars[i], plus1);
                            --pos1;
                            --pos2;
                        }
                        else if (plus1)
                        {
                            plus1 = Plus(unsignedValue1[pos1], '0', resultChars[i], plus1);
                            --pos1;
                        }
                        else
                        {
                            resultChars[i] = unsignedValue1[pos1];
                            --pos1;
                        }
                    }
                    else if (pos2 >= 0) //&& unsignedValue2[pos2] >= '0' && unsignedValue2[pos2] <= '9')
                    {
                        if (plus1)
                        {
                            plus1 = Plus('0', unsignedValue2[pos2], resultChars[i], plus1);
                            --pos2;
                        }
                        else
                        {
                            resultChars[i] = unsignedValue2[pos2];
                            --pos2;
                        }
                    }
                    else if (plus1)
                    {
                        resultChars[i] = '1';

                        return i;
                    }
                    else
                    {
                        return i + 1;
                    }
                }

                return -1;
            }

            static const int Minus(const char *unsignedValue1, const int& length1, const char *unsignedValue2, const int& length2, char *resultChars, const int& resultSize, int& isLeftBigger)
            {
                if (isLeftBigger > 1 || isLeftBigger < -1)
                {
                    isLeftBigger = IsLeftBigger(unsignedValue1, length1, unsignedValue2, length2);

                    return Minus(unsignedValue1, length1, unsignedValue2, length2, resultChars, resultSize, isLeftBigger);
                }
                else if (isLeftBigger == -1)
                {
                    int _leftBigger = 1;

                    return Minus(unsignedValue2, length2, unsignedValue1, length1, resultChars, resultSize, _leftBigger);
                }

                resultChars[resultSize - 1] = 0;

                if (isLeftBigger == 0)
                {
                    const int pos = resultSize - 2;

                    resultChars[pos] = '0';

                    return pos;
                }

                int pos1 = length1 - 1;
                int pos2 = length2 - 1;
                bool minus1 = false;

                for (int i = resultSize - 2; i >= 0; --i)
                {
                    if (pos1 >= 0) //&& unsignedValue1[pos1] >= '0' && unsignedValue1[pos1] <= '9')
                    {
                        if (pos2 >= 0) //&& unsignedValue2[pos2] >= '0' && unsignedValue2[pos2] <= '9')
                        {
                            minus1 = Minus(unsignedValue1[pos1], unsignedValue2[pos2], resultChars[i], minus1);
                            --pos1;
                            --pos2;
                        }
                        else if (minus1)
                        {
                            minus1 = Minus(unsignedValue1[pos1], '0', resultChars[i], minus1);
                            --pos1;
                        }
                        else
                        {
                            resultChars[i] = unsignedValue1[pos1];
                            --pos1;
                        }
                    }
                    // else if (pos2 >= 0) //&& unsignedValue2[pos2] >= '0' && unsignedValue2[pos2] <= '9')
                    // {
                    //     //TODO
                    //     return -1;
                    // }
                    // else if (minus1)
                    // {
                    //     //TODO
                    //     return -1;
                    // }
                    else
                    {
                        return i + 1;
                    }
                }

                return -1;
            }

            static const int Minus(const char *unsignedValue1, const int& length1, const char *unsignedValue2, const int& length2, char *resultChars, const int& resultSize)
            {
                int isLeftBigger = 2;

                return Minus(unsignedValue1, length1, unsignedValue2, length2, resultChars, resultSize, isLeftBigger);
            }

            static const int Calculate(const char *input, const int& inputLength, char *outputBuf, const int& outputBufSize)
            {
                char *value1 = NULL;
                bool unsigned1 = true;
                int length1 = 0;
                bool end1 = false;

                char *value2 = NULL;
                bool unsigned2 = true;
                int length2 = 0;
                bool end2 = false;

                int pos = -1;

                int isLeftBigger = 2;

                for (int i = 0; i < (int)inputLength; ++i)
                {
                    if (input[i] >= '0' && input[i] <= '9')
                    {
                        if (value1 == NULL)
                        {
                            value1 = (char *)&input[i];
                            ++length1;
                        }
                        else if (!end1)
                        {
                            ++length1;
                        }
                        else if (value2 == NULL)
                        {
                            value2 = (char *)&input[i];
                            ++length2;
                        }
                        else if (!end2)
                        {
                            ++length2;
                        }
                    }
                    else if (value1 != NULL && !end1)
                    {
                        end1 = true;
                    }
                    else if (value2 != NULL && !end2)
                    {
                        end2 = true;
                    }

                    if (input[i] == '-')
                    {
                        if (value1 == NULL)
                        {
                            unsigned1 = false;
                        }
                        else if (value2 == NULL)
                        {
                            unsigned2 = false;
                        }
                    }
                }

                if ((value1 == NULL) && (value2 == NULL))
                {
                    memset(outputBuf, ' ', outputBufSize - 1);
                    outputBuf[outputBufSize - 1] = 0;

                    return pos;
                }
                else if (value1 == NULL)
                {
                    value1 = (char *)"0";
                    unsigned1 = unsigned2;
                    length1 = 1;
                }
                else if (value2 == NULL)
                {
                    value2 = (char *)"0";
                    unsigned2 = unsigned1;
                    length2 = 1;
                }

                if (unsigned1 && unsigned2)
                {
                    pos = Plus(value1, length1, value2, length2, outputBuf, outputBufSize);
                }
                else if (!unsigned1 && !unsigned2)
                {
                    pos = Plus(value1, length1, value2, length2, outputBuf, outputBufSize);
                }
                else
                {
                    pos = Minus(value1, length1, value2, length2, outputBuf, outputBufSize, isLeftBigger);
                }

                TrimLeft(outputBuf, outputBufSize, pos, unsigned1, unsigned2, isLeftBigger);

                return pos;
            }
    };

    struct ASCIICharMathResult ASCIINumberMath::CharPlusResults[ASCIINumberCntMax][ASCIINumberCntMax];
    struct ASCIICharMathResult ASCIINumberMath::CharMinusResults[ASCIINumberCntMax][ASCIINumberCntMax];

    class ANMathStaticCons //ASCIINumberMath Static Constructor
    {
        struct Constructor
        {
            Constructor()
            {
                ASCIINumberMath::ResetCharPlusResults();
                ASCIINumberMath::ResetCharMinusResults();
            }
        };

        const static Constructor Cons;
    };

    const struct ANMathStaticCons::Constructor ANMathStaticCons::Cons;
}

#pragma pack()

#endif
