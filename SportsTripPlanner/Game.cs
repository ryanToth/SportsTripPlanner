using System;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public class Game
    {
        public City HomeTeam { get; private set; }
        public City AwayTeam { get; private set; }
        public DateTime Date { get;  private set; }

        public static async Task<Game> CreateGameAsync(string homeTeamCode, string awayTeamCode, string dateCode)
        {
            Game game = new Game();
            await game.InitializeAsync(homeTeamCode, awayTeamCode, dateCode);
            return game;
        }

        private Game() { }

        private async Task InitializeAsync(string homeTeamCode, string awayTeamCode, string dateCode)
        {
            this.HomeTeam = await City.GetCityAsync(homeTeamCode);
            this.AwayTeam = await City.GetCityAsync(awayTeamCode);

            // All times from the API are in eastern standard time
            // TODO: This assumption may not be true for all APIs for all sports
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            this.Date = Utilities.ConvertRawDateToReadableString(dateCode, tz, this.HomeTeam.TimeZone);
        }
    }
}
