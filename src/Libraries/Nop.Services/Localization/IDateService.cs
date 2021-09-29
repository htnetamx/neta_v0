using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Localization
{
    static class IDateService
    {
        public static DateTime ChangeTime(this DateTime dateTime, int hours, int minutes=0, int seconds=0, int milliseconds=0)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }
        public static DateTime ChangeUTCToMX(DateTime dateTime)
        {

            TimeZoneInfo mxTimeZone;
            try
            {
                mxTimeZone=TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            }
            catch {
                mxTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
            }
            
            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc, mxTimeZone);

        }
        public static DateTime ChangeMXToUTC(DateTime dateTime)
        {
            TimeZoneInfo mxTimeZone;
            try
            {
                mxTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            }
            catch
            {
                mxTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
            }
            return TimeZoneInfo.ConvertTime(dateTime, mxTimeZone, TimeZoneInfo.Utc);

        }
    }
}
