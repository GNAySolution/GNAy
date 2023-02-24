#ifndef _TOOLS_THREAD_POOL_H
#define _TOOLS_THREAD_POOL_H

#include <condition_variable>
#include <functional>
#include <future>
#include <memory>
#include <mutex>
#include <queue>
#include <stdexcept>
#include <thread>
#include <vector>

#pragma pack(1)

namespace Tools
{
    class ThreadPool
    {
        protected:
            // need to keep track of threads so we can join them
            std::vector<std::thread> _workers;
            // the task queue
            std::queue<std::function<void()>> _tasks;

            // synchronization
            std::mutex _queue_mutex;
            std::condition_variable _condition;

            bool _stop;

        public:
            ThreadPool(): _stop(false) {}

            ~ThreadPool();

            const int GetSizeMax()
            {
                return _workers.size();
            }

            const bool SetSizeMax(size_t);

            const int QueueCount()
            {
                return _tasks.size();
            }

            template<class F, class... Args>
            auto Enqueue(F&& f, Args&&... args)
                -> std::future<typename std::result_of<F(Args...)>::type>;
    };

    // the constructor just launches some amount of workers
    const bool ThreadPool::SetSizeMax(size_t threadsMax)
    {
        if (GetSizeMax() > 0)
        {
            return false;
        }

        for (size_t i = 0; i < threadsMax; ++i)
        {
            _workers.emplace_back([this]
            {
                for (;;)
                {
                    std::function<void()> task;

                    {
                        std::unique_lock<std::mutex> lock(this->_queue_mutex);

                        this->_condition.wait(lock, [this] { return this->_stop || !this->_tasks.empty(); });

                        if (this->_stop && this->_tasks.empty())
                            return;

                        task = std::move(this->_tasks.front());

                        this->_tasks.pop();
                    }

                    task();
                }
            } );
        }

        return GetSizeMax() > 0;
    }

    // the destructor joins all threads
    ThreadPool::~ThreadPool()
    {
        printf("_stop=%d|SizeMax=%d|QueueCount=%d|ThreadPool::~ThreadPool|\r\n", _stop, GetSizeMax(), QueueCount());

        {
            std::unique_lock<std::mutex> lock(_queue_mutex);
            _stop = true;
        }
        _condition.notify_all();

        for (std::thread &worker: _workers)
            worker.join();
    }

    // add new work item to the pool
    template<class F, class... Args>
    auto ThreadPool::Enqueue(F&& f, Args&&... args)
        -> std::future<typename std::result_of<F(Args...)>::type>
    {
        using return_type = typename std::result_of<F(Args...)>::type;

        auto task = std::make_shared<std::packaged_task<return_type()>> (
            std::bind(std::forward<F>(f), std::forward<Args>(args)...)
        );

        std::future<return_type> res = task->get_future();
        {
            std::unique_lock<std::mutex> lock(_queue_mutex);

            // don't allow enqueueing after stopping the pool
            if (_stop)
                throw std::runtime_error("enqueue on stopped ThreadPool");

            _tasks.emplace([task]() { (*task)(); });
        }
        _condition.notify_one();

        return res;
    }
}

#pragma pack()

#endif
