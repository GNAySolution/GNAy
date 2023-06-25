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
            static const std::string GetIDStr()
            {
                const std::thread::id& _tID = std::this_thread::get_id();

                std::stringstream ss;
                ss << _tID;

                return ss.str();
            }

        public:
            static thread_local const std::string IDStr;
            static thread_local const long long IDValue;
    };

    thread_local const std::string IL01ThreadExtensions::IDStr = IL01ThreadExtensions::GetIDStr();
    thread_local const long long IL01ThreadExtensions::IDValue = std::stoll(IL01ThreadExtensions::IDStr);
}

#pragma pack()

#endif
