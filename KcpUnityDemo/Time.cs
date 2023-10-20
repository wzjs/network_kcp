using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    internal static class Time
    {
        public static DateTime dt1970 = new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc); 
        public static long NowUTC()
        {
            return (DateTime.UtcNow.Ticks - dt1970.Ticks) / 10000;
        }

        //todo 当前时区时间
    }
}
