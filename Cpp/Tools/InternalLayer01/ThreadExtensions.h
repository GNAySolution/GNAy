#ifndef _TOOLS_IL01_THREAD_EXTENSIONS_H
#define _TOOLS_IL01_THREAD_EXTENSIONS_H

#include <cstring>
#include <iostream>
#include <sstream>
#include <thread>

#pragma pack(1)

namespace Tools
{
    class IL01ThreadExtensions
    {
        protected:
            static const long long GetIDValue()
            {
                const std::thread::id& _tID = std::this_thread::get_id();

                std::stringstream ss;
                ss << _tID;

                const std::string str = ss.str();

                printf("TRACE|Thread %s is initialized.\r\n", str.c_str());

                return std::stoll(str);
            }

        public:
            static thread_local const long long IDValue;
    };

    thread_local const long long IL01ThreadExtensions::IDValue = IL01ThreadExtensions::GetIDValue();
}

#pragma pack()

#endif
