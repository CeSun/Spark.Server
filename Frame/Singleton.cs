using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public class Singleton<TSub> where TSub : new ()
    {
        static TSub instance = new TSub();
        public static TSub Instance { get => instance; }

        protected Singleton() { }
    }
}
