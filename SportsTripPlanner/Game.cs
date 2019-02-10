using System;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public class Game
    {
        public Team HomeTeam { get; private set; }
        public Team AwayTeam { get; private set; }
        public DateTime Date { get;  private set; }
        public League League { get; private set; }

        public static async Task<Game> CreateGameAsync(string homeTeamCode, string awayTeamCode, string dateCode, League league)
        {
            Game game = new Game(league);
            await game.InitializeAsync(homeTeamCode, awayTeamCode, dateCode);
            return game;
        }

        public static async Task<Game> CreateGameAsync(string homeTeamCode, string awayTeamCode, DateTime gameTime, League league)
        {
            Game game = new Game(league);
            await game.InitializeAsync(homeTeamCode, awayTeamCode, gameTime);
            return game;
        }

        private Game(League league)
        {
            this.League = league;
        }

        private async Task InitializeAsync(string homeTeamCode, string awayTeamCode, string dateCode)
        {
            await this.InitializeTeams(homeTeamCode, awayTeamCode);
            // All times from the API are in eastern standard time
            // TODO: This assumption may not be true for all APIs for all sports
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            this.Date = Utilities.ConvertRawDateToReadableString(dateCode, tz, this.HomeTeam.TimeZone);
        }

        private async Task InitializeAsync(string homeTeamCode, string awayTeamCode, DateTime gameTime)
        {
            await this.InitializeTeams(homeTeamCode, awayTeamCode);
            this.Date = gameTime;
        }

        private async Task InitializeTeams(string homeTeamCode, string awayTeamCode)
        {
            this.HomeTeam = await Team.GetCityAsync(homeTeamCode, this.League);
            this.AwayTeam = await Team.GetCityAsync(awayTeamCode, this.League);
        }

        public override bool Equals(object obj)
        {
            return obj is Game game && this.HomeTeam.Equals(game.HomeTeam)
                && this.AwayTeam.Equals(game.AwayTeam) && this.Date == game.Date;
        }
    }
}
