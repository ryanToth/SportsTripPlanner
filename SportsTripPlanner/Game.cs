using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    internal class Game
    {
        public City HomeTeam { get; }
        public City AwayTeam { get; }
        public DateTime Date { get; }

        public Game(string homeTeamCode, string awayTeamCode, string dateCode)
        {
            this.HomeTeam = new City(homeTeamCode);
            this.AwayTeam = new City(awayTeamCode);

            // All times from the API are in eastern standard time
            // TODO: This assumption may not be true for all APIs for all sports
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            this.Date = Utilities.ConvertRawDateToReadableString(dateCode, tz, this.HomeTeam.TimeZone);
        }
    }
}
