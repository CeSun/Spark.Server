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
            int _front = front;
            outlist = null;
            if (back == _front)
                return false;
            outlist = new List<T>();
            for (; back != _front; back = GetIndex(back + 1))
            {
                outlist.Add(list[back]);
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
            int _front = front;
            outlist = null;
            if (back == _front)
                return false;
            outlist = new List<T>();
            for (int i = 0; i < max && back != _front; i++,back = GetIndex(back+ 1))
            {
                outlist.Add(list[back]);
            }
            return true;

        }
    }

}
