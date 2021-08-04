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
            int len = list.Length;
            if (front > back)
                len = front;
            for (int i = back; i < len; i++)
            {
                outlist.Add(list[i]);
                back = i + 1;
                if (back >= list.Length)
                    back = GetIndex(back);
            }
            for (int i = back; i < front; i++)
            {
                outlist.Add(list[i]);
                back = i + 1;
                if (back >= list.Length)
                    back = GetIndex(back);
            }
            return true;
        }

        public bool Get(out List<T> outlist, int max)
        {
            outlist = null;
            if (back == front)
                return false;

            outlist = new List<T>();
            int len = 0;
            if (front < back)
                len = front + list.Length - back;
            else
                len = front - back;
            int newfront = GetIndex(back + max);
            if (len < max)
            {
                newfront = front;
            }
            int arraylen = list.Length;
            if (newfront >= back)
            {
                arraylen = newfront;
            }
            for (int i = back; i < arraylen; i++)
            {
                outlist.Add(list[i]);
                back = i + 1;
                if (back >= list.Length)
                    back = GetIndex(back);
            }
            for (int i = back; i < newfront; i++)
            {
                outlist.Add(list[i]);
                back = i + 1;
                if (back >= list.Length)
                    back = GetIndex(back);
            }
            return true;
        }
    }

}
