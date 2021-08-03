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
            int len = outlist.Count;
            if (front > back)
                len = front;
            for (int i = back; i < len; i++)
            {
                outlist.Add(list[i]);
                back = i + 1;
                if (back >= list.Length)
                    back = GetIndex(back);
            }
            if (front < back)
            {
                for (int i = 0; i < len; i++)
                {
                    outlist.Add(list[i]);
                    back = i + 1;
                }
            }
            return true;
        }
    }

}
