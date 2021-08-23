using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TreeLib;

namespace Frame
{
    public class Timer: Singleton<Timer>
    {
        class RunableAction
        {
            public long Interval;
            public Action Action;
            public bool IsLoop;
            public long StartTime;
            public Stopwatch sw;
        }
        SortedDictionary<long, List<RunableAction>> onceTimer = new SortedDictionary<long, List<RunableAction>>();

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
            RunableAction action = new RunableAction { Action = fun, Interval = time, IsLoop = IsLoop, StartTime = TimeMngr.Instance.Timestamp,sw = Stopwatch.StartNew() };
            List<RunableAction> statck;
            if (onceTimer.TryGetValue(nextTime, out statck))
            {
                statck.Add(action);
            }
            else
            {
                statck = new List<RunableAction>();
                statck.Add(action);
                onceTimer.Add(nextTime, statck);
            }
        }
        
        public void Update()
        {
            var now = TimeMngr.Instance.Timestamp;
            if (onceTimer.Count == 0)
                return;
            var pair = onceTimer.FirstOrDefault();
            if (pair.Key <= now)
            {
                onceTimer.Remove(pair.Key);
                foreach (var item2 in pair.Value)
                {
                    var item = item2;
                    try
                    {
                        item.Action();
                        item.sw.Stop();
                        Console.WriteLine($"timer: start:{item.StartTime}, interval: {item.Interval}, now: {now}, detal:{now - item.StartTime}, sw: {item.sw.Elapsed.TotalMilliseconds}");
                       
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                    if (item.IsLoop == true)
                    {
                        var time = now + item.Interval;
                        var stack = onceTimer.GetValueOrDefault(time);
                        item.StartTime = now; 
                        item.sw.Restart();
                        if (stack == null)
                        {
                            stack = new List<RunableAction>();
                            onceTimer[time] = stack;
                        }
                        stack.Add(item);
                    }
                }
            }
        }

        public void Fini()
        {

        }
    }
}
