using System;
using System.Collections.Generic;

namespace Frame
{
    /// <summary>
    /// 无锁队列
    /// </summary>
    /// <typeparam name="T">要存放的数据类型</typeparam>
    class LockFreeQueue<T>
    {
        T[] list;
        int front;
        int back;

        public LockFreeQueue() : this(1024)
        {

        }

        public LockFreeQueue(int size)
        {
            list = new T[size + 1];
            front = 0;
            back = 0;
        }

        public bool Add(T value)
        {
            if (GetIndex(front + 1) == back)
            {
                return false;
            }
            list[front] = value;
            front = GetIndex(front + 1);
            return true;
        }
        private int GetIndex(int index)
        {
            return index % list.Length;
        }

        public bool TryGetAll(out List<T> outlist)
        {
            outlist = null;
            if (back == front)
                return false;
            outlist = new List<T>();
            T t = default;
            while(TryGet(out t))
            {
                outlist.Add(t);
            }
            return true;
        }
        public bool TryGet(out T value)
        {
            value = default;
            if (back == front)
                return false;

            value = list[back];
            back = GetIndex(back + 1);
            return true;
        }
        public bool Get(out List<T> outlist, int max)
        {
            outlist = null;
            if (back == front)
                return false;
            outlist = new List<T>();
            T t = default;
            int i = 0;
            while (TryGet(out t))
            {
                outlist.Add(t);
                ++i;
                if (i >= max)
                    break;
            }
            return true;

        }
    }

}
