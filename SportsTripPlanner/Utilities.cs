using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SportsTripPlanner
{
    internal static class Utilities
    {
        /// <summary>
        /// Converts a string from time zone <paramref name="rawDataTimeZone"/> to a DateTime in time zone <paramref name="targetTimeZone"/>
        /// </summary>
        /// <param name="rawDate">The string time data</param>
        /// <param name="rawDataTimeZone">The time zone that the string time data is from</param>
        /// /// <param name="targetTimeZone">The time zone that the converted time data should be in</param>
        /// <returns>The DateTime of the input string in the target time zone</returns>
        public static DateTime ConvertRawDateToReadableString(string rawDate, TimeZoneInfo rawDataTimeZone, TimeZoneInfo targetTimeZone)
        {
            DateTime rawDataTime = DateTime.ParseExact(rawDate, "yyyyMMdd HH:mm:ss", CultureInfo.CurrentCulture);
            DateTime UTCTime = TimeZoneInfo.ConvertTimeToUtc(rawDataTime, rawDataTimeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(UTCTime, targetTimeZone);
        }

        public static bool InBetweenDaysInclusive(DateTime date, DayOfWeek start, DayOfWeek end)
        {
            DayOfWeek curDay = date.DayOfWeek;

            if (start <= end)
            {
                return (start <= curDay && curDay <= end);
            }
            else
            {
                return (start <= curDay || curDay <= end);
            }
        }

        public static AsyncLazy<IEnumerable<Team>> NhlCities = new AsyncLazy<IEnumerable<Team>>(async () => 
        {
            List<Team> cities = new List<Team>();
            using (StreamReader r = new StreamReader(@".\Data\nhl-arena-locations.json"))
            {
                string json = r.ReadToEnd();
                IEnumerable<RawCityInfo> rawCityInfoList = JsonConvert.DeserializeObject<IEnumerable<RawCityInfo>>(json);
                cities.AddRange(rawCityInfoList.Select(x => new Team(x.team, x.code, x.arena, x.longitude, x.latitude, x.timezone)));
            }

            return cities;
        });

        public static AsyncLazy<IEnumerable<Team>> NbaCities = new AsyncLazy<IEnumerable<Team>>(async () =>
        {
            List<Team> cities = new List<Team>();
            using (StreamReader r = new StreamReader(@".\Data\nba-stadium-locations.json"))
            {
                string json = r.ReadToEnd();
                IEnumerable<RawCityInfo> rawCityInfoList = JsonConvert.DeserializeObject<IEnumerable<RawCityInfo>>(json);
                cities.AddRange(rawCityInfoList.Select(x => new Team(x.team, x.code, x.arena, x.longitude, x.latitude, x.timezone)));
            }

            return cities;
        });
    }
}
