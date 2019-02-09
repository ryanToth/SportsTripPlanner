using System;
using System.Collections.Generic;
using System.Linq;

namespace SportsTripPlanner
{
    public class Trip : HashSet<Game>
    {
        private int tripLength;
        private int maxTravel;

        public Trip(int tripLength, int maxTravel, params Game[] games)
        {
            this.tripLength = tripLength;
            this.maxTravel = maxTravel;
            this.UnionWith(games);
        }

        public Trip(Trip trip) : this(trip.tripLength, trip.maxTravel, trip.ToArray())
        { }

        // To be able to be added to the trip the game must
        //  1. Be within the trip length (counting the beginning of a day at midnight on the first day, not when th first game starts)
        //  2. There must be at least 8 hours between ea ch game
        //  3. It must be within the distance threshold from the last game
        internal bool CanAddGameToTrip(Game game)
        {
            return this.GameFitsWithinTripLength(game) &&
                   !this.GameConflictsWithExistingGameInTrip(game) &&
                   this.GetEndingCity().GetDistanceToInKm(game.HomeTeam) <= this.maxTravel;
        }

        internal bool GameFitsWithinTripLength(Game game)
        {
            return this.GetStartingDate().Date.AddDays(this.tripLength) > game.Date;
        }

        internal bool GameConflictsWithExistingGameInTrip(Game game)
        {
            return this.GetConflictingGame(game) != null;
        }

        internal bool SpansWeekend()
        {
            // If the number of days plus the length of the trip is over or equal to 7, the trip will end on or after Sunday
            return (int)this.GetStartingDate().DayOfWeek + this.NumberOfDays >= 7;
        }

        internal City GetStartingCity()
        {
            return this.StartingGame.HomeTeam;
        }

        internal City GetEndingCity()
        {
            return this.EndingGame.HomeTeam;
        }

        internal DateTime GetStartingDate()
        {
            return this.StartingGame.Date;
        }

        internal DateTime GetEndingDate()
        {
            return this.EndingGame.Date;
        }

        internal string GetHomeTeams()
        {
            return string.Join(", ", this.Select(x => x.HomeTeam.Name).Distinct());
        }

        internal string GetAllTeams()
        {
            List<string> teams = new List<string>(this.Select(x => x.HomeTeam.Name));
            teams.AddRange(this.Select(x => x.AwayTeam.Name));
            return string.Join(", ", teams.Distinct());
        }

        internal string GetGames()
        {
            return string.Join(", ", this.Select(x => $"{x.AwayTeam.Name} at {x.HomeTeam.Name} {x.Date}"));
        }

        internal string GetDates()
        {
            return $"{this.GetStartingDate()} - {this.GetEndingDate()}";
        }

        internal string GetDays()
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

        public override bool Equals(object obj)
        {
            return obj is Trip trip && this.SetEquals(trip);
        }

        internal Game StartingGame
        {
            get
            {
                return this.OrderBy(x => x.Date).First();
            }
        }

        private double NumberOfDays
        {
            get
            {
                return (this.GetEndingDate() - this.GetStartingDate()).TotalDays;
            }
        }

        internal Game EndingGame
        {
            get
            {
                return this.OrderByDescending(x => x.Date).First();
            }
        }

        private Game GetConflictingGame(Game game)
        {
            return this.Where(x => x.Date.AddHours(8).CompareTo(game.Date) > 0 && x.Date.CompareTo(game.Date.AddHours(8)) < 0).FirstOrDefault();
        }
    }
}
