﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveBlockCore.Utilities
{
    internal class TimeUtil
    {
        public static long GetTime()
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long nowTicks = DateTime.UtcNow.Ticks;
            long timeStamp = ((nowTicks - epochTicks) / TimeSpan.TicksPerSecond);
            return timeStamp;

            //returns time in ticks from Epoch Time
        }

        public static DateTime ToDateTime(long unixTime)
        {
            DateTime frDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            frDateTime = frDateTime.AddSeconds(unixTime).ToLocalTime();
            return frDateTime;
        }
    }
}
