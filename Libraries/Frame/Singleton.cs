using System;
using System.Collections.Generic;
using System.Text;

namespace Frame
{
    public class Singleton<T> where T: new()
    {
        static T instance = new T();
        // 锁辅助对象
        private static readonly object SynObject = new object();
        public static T Instance
        {
           get {
                if (null == instance)
                {
                    lock (SynObject)
                    {
                        if (null == instance)
                        {
                            instance = new T();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
