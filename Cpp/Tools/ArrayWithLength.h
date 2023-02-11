#ifndef  _TOOLS_ARRAY_WITH_LENGTH_H
#define  _TOOLS_ARRAY_WITH_LENGTH_H

#include <cstring>
#include <stdio.h>

#pragma pack(1)

namespace Tools
{
    template<typename T>
    class ArrayWithLength
    {
        protected:
            bool _onlyForInside;
            int _len;
            T *_body;

        public:
            const bool OnlyForInside() const { return _onlyForInside; }
            const int Length() const { return _len; }
            const T *Body() const { return _body; }

            ArrayWithLength(): _onlyForInside(false), _len(0), _body( NULL ) {}

            ArrayWithLength(const T *src, const int& length, const bool& onlyForInside = false)
            {
                _onlyForInside = onlyForInside;
                _len = length;

                if (onlyForInside)
                {
                    _body = (T *)src;
                }
                else
                {
                    _body = new T[length];
                    memcpy(_body, src, sizeof(T) * length);
                }
            }

            ~ArrayWithLength()
            {
                if(!_onlyForInside && _body != NULL)
                {
                    printf("len=%d|msg=%s|Tools::ArrayWithLength::~ArrayWithLength|\r\n", _len, _body);

                    _len = 0;
                    delete [] _body;
                }
                
                _len = 0;
            }
    };

    class CharArray : public ArrayWithLength<char>
    {
        public:
            CharArray(const char *src, const bool& onlyForInside = false)
            {
                _onlyForInside = onlyForInside;
                _len = strlen(src);

                const int _lenWithEnd = _len + 1;

                if (onlyForInside)
                {
                    _body = (char *)src;
                }
                else
                {
                    _body = new char[_lenWithEnd];
                    memcpy(_body, src, _lenWithEnd);
                }
            }
    };
}

#pragma pack()

#endif
