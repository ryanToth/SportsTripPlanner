using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    internal class Trip : List<Game>
    {
        public Trip(params Game[] games)
        {
            this.AddRange(games);
        }

        // To be able to be added to the trip the game must
        //  1. Be within the trip length (counting the beginning of a day at midnight on the first day, not when th first game starts)
        //  2. There must be at least 8 hours between ea ch game
        //  3. It must be within the distance threshold from the last game
        public bool CanAddGameToTrip(Game game, int tripLength, int maxTravel)
        {
            return this.GetStartingDate().Date.AddDays(tripLength) > game.Date &&
                   this.Where(x => x.Date.AddHours(8).CompareTo(game.Date) > 0 && x.Date.CompareTo(game.Date.AddHours(8)) < 0).Count() == 0 &&
                   this.GetEndingCity().GetDistanceToInKm(game.HomeTeam) <= maxTravel;
        }

        public bool SpansWeekend()
        {
            // If the number of days plus the length of the trip is over or equal to 7, the trip will end on or after Sunday
            return (int)this.GetStartingDate().DayOfWeek + this.NumberOfDays >= 7;
        }

        public City GetStartingCity()
        {
            return this.First(x => x.Date == this.GetStartingDate()).HomeTeam;
        }

        public City GetEndingCity()
        {
            return this.First(x => x.Date == this.GetEndingDate()).HomeTeam;
        }

        public DateTime GetStartingDate()
        {
            return this.OrderBy(x => x.Date).First().Date;
        }

        public DateTime GetEndingDate()
        {
            return this.OrderByDescending(x => x.Date).First().Date;
        }

        public string GetHomeTeams()
        {
            return string.Join(", ", this.Select(x => x.HomeTeam.Name).Distinct());
        }

        public string GetAllTeams()
        {
            List<string> teams = new List<string>(this.Select(x => x.HomeTeam.Name));
            teams.AddRange(this.Select(x => x.AwayTeam.Name));
            return string.Join(", ", teams.Distinct());
        }

        public string GetGames()
        {
            return string.Join(", ", this.Select(x => $"{x.AwayTeam.Name} at {x.HomeTeam.Name} {x.Date}"));
        }

        public string GetDates()
        {
            return $"{this.GetStartingDate()} - {this.GetEndingDate()}";
        }

        public string GetDays()
        {
            return $"{this.GetStartingDate().DayOfWeek} - {this.GetEndingDate().DayOfWeek}";
        }

        public override string ToString()
        {
            return $"Dates: {this.GetDates()}{Environment.NewLine}" +
                   $"Days: {this.GetDays()}{Environment.NewLine}" +
                   $"Home Teams: {this.GetHomeTeams()}{Environment.NewLine}" +
                   $"All Teams: {this.GetAllTeams()}{Environment.NewLine}" +
                   $"Games: {this.GetGames()}{Environment.NewLine}";
        }

        private double NumberOfDays
        {
            get
            {
                return (this.GetEndingDate() - this.GetStartingDate()).TotalDays;
            }
        }
    }
}
