using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame
{
    public class TimeMngr: Singleton<TimeMngr>
    {
        public int Zone => zone;

        private long offset;
        public long Timestamp { get; private set; }
        public long UtcTimestamp { get; private set; }
        public long RealTimestamp { get; private set; }
        public long RealUtcTimestamp { get; private set; }

        private int zone;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="zone"></param>
        public void Init(int zone)
        {
            this.zone = zone; 
            Update();
        }

        DateTime initTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// 设置时间偏移 (用于GM改时间)
        /// </summary>
        /// <param name="offset"></param>
        public void SetTimeOffset(long offset)
        {
            this.offset = offset;
        }

        /// <summary>
        /// 每帧调用 用于更新时间
        /// </summary>
        public void Update()
        {
            RealUtcTimestamp = (long)(DateTime.UtcNow - initTime).TotalMilliseconds;
            RealTimestamp = RealUtcTimestamp + (zone * 60 * 60 * 1000);
            UtcTimestamp = RealUtcTimestamp + offset;
            Timestamp = RealTimestamp + offset;
        }
        public bool IsSameDay(long time1, long time2)
        {
            throw new NotImplementedException();
        }
        public bool IsSameWeek(long time1, long time2)
        {
            throw new NotImplementedException();
        }
        public bool IsSameMonth(long time1, long time2)
        {
            throw new NotImplementedException();
        }
        public bool IsSameYear(long time1, long time2)
        {
            throw new NotImplementedException();
        }


    }
}
