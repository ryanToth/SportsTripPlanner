using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static AsyncLazy<IEnumerable<City>> Cities = new AsyncLazy<IEnumerable<City>>(async () => 
        {
            List<City> cities = new List<City>();
            using (StreamReader r = new StreamReader(@".\Data\nhl-arena-locations.json"))
            {
                string json = r.ReadToEnd();
                IEnumerable<RawCityInfo> rawCityInfoList = JsonConvert.DeserializeObject<IEnumerable<RawCityInfo>>(json);
                cities.AddRange(rawCityInfoList.Select(x => new City(x.team, x.code, x.arena, x.longitude, x.latitude, x.timezone)));
            }

            return cities;
        });
    }
}
