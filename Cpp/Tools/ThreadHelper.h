#ifndef _TOOLS_THREAD_HELPER_H
#define _TOOLS_THREAD_HELPER_H

#include <cstring>
#include <iostream>
#include <sstream>
#include <thread>

#include "NumberWithString.h"

#pragma pack(1)

namespace Tools
{
    class ThreadHelper
    {
        protected:
            static const struct LongWithStr GetID()
            {
                struct LongWithStr tID;

                GetID(tID);

                return tID;
            }

        public:
            static thread_local const struct LongWithStr ThreadID;

            static const bool GetID(struct LongWithStr& tID)
            {
                if (tID.ValueStrLen > 0)
                {
                    return true;
                }

                const std::thread::id& _tID = std::this_thread::get_id();

                std::stringstream ss;
                ss << _tID;

                const std::string str = ss.str();

                tID.Value = std::stoll(str);
                tID.ValueStrLen = sprintf(tID.ValueStr, "%s", str.c_str());

                return tID.Value != 0;
            }
    };

    thread_local const struct LongWithStr ThreadHelper::ThreadID = ThreadHelper::GetID();
}

#pragma pack()

#endif
