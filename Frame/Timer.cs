using System;
using System.Collections.Generic;
using System.Collections.Generic.RedBlack;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public class Timer
    {
        // 只执行一次的
        public RedBlackTree<ulong, Stack<Action>> onceTimer = new RedBlackTree<ulong, Stack<Action>>();
        public RedBlackTree<ulong, List<Action>> loopTimer = new RedBlackTree<ulong, List<Action>>();


        public void Init()
        {

        }

        public void Update()
        {
            
        }

        public void Fini()
        {

        }
    }
}
