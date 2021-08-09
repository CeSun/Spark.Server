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
        public long Timestamp => utcTimestamp + (zone * 60 * 60) + offset;
        public long UtcTimestamp => utcTimestamp + offset;
        public long RealTimestamp => utcTimestamp + (zone * 60 * 60);
        public long RealUtcTimestamp => utcTimestamp;

        private int zone;

        private long utcTimestamp;
        public void Init(int zone)
        {
            this.zone = zone;
        }
        DateTime initTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public void SetTimeOffset(long offset)
        {
            this.offset = offset;
        }
        public void Update()
        {
            utcTimestamp = (long)(DateTime.UtcNow - initTime).TotalMilliseconds;
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
