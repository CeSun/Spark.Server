using System;
using System.Collections.Generic;
using TreeLib;

namespace Frame
{
    public class Timer: Singleton<Timer>
    {
        struct RunableAction
        {
            public long Interval;
            public Action Action;
            public bool IsLoop;
        }

        private RedBlackTreeMap<long, Stack<RunableAction>> onceTimer = new RedBlackTreeMap<long, Stack<RunableAction>>();

        public void Init()
        {

        }

        /// <summary>
        /// 添加一次定时器
        /// </summary>
        /// <param name="time">间隔时间</param>
        /// <param name="fun">要执行的函数</param>
        public void SetTimeOut(long time, Action fun)
        {
            AddFun(time, fun, false);
        }

        /// <summary>
        /// 添加循环定时器
        /// </summary>
        /// <param name="time">间隔时间</param>
        /// <param name="fun">要执行的函数</param>
        public void SetInterval(long time, Action fun)
        {
            AddFun(time, fun, true);
        }

        private void AddFun(long time, Action fun, bool IsLoop)
        {
            var nextTime = TimeMngr.Instance.Timestamp + time;
            RunableAction action = new RunableAction { Action = fun, Interval = time, IsLoop = IsLoop };
            Stack<RunableAction> statck;
            if (onceTimer.TryGetValue(nextTime, out statck))
            {
                statck.Push(action);
            }
            else
            {
                statck = new Stack<RunableAction>();
                statck.Push(action);
                onceTimer.Add(nextTime, statck);
            }
        }
        
        public void Update()
        {
            var now = TimeMngr.Instance.Timestamp;
            long key;
            Stack<RunableAction> stack;
            var isFound = onceTimer.Least(out key, out stack);
            if (isFound && key < now)
            {
                while (isFound && key < now)
                {
                    onceTimer.Remove(key);
                    isFound = onceTimer.Least(out key, out stack);
                }
            }
            if (isFound && now == key)
            {
                foreach (var action in stack)
                {
                    action.Action();
                    if (action.IsLoop == true)
                    {
                        var nextTime = action.Interval;
                        AddFun(nextTime, action.Action, true);
                    }
                }
                onceTimer.Remove(key);
            }
        }

        public void Fini()
        {

        }
    }
}
