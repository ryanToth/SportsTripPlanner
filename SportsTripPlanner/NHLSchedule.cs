using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public class NhlSchedule : HashSet<Game>
    {
        private string yearCode;
        
        private NhlSchedule(string yearCode)
        {
            if (yearCode == null)
            {
                yearCode = this.GetCurrentSeasonScheduleYear();
            }

            this.ValidateConstructorArguments(yearCode);

            this.yearCode = yearCode;
        }

        public static async Task<NhlSchedule> GetNhlScheduleAsync(string yearCode)
        {
            NhlSchedule schedule = new NhlSchedule(yearCode);
            await schedule.InitializeAsync();
            return schedule;
        }

        internal IEnumerable<Trip> GetTrips(int tripLength, int minimumNumberOfGames, int maxTravel, 
            IEnumerable<string> mustSeeTeam, string necessaryHomeTeam, bool mustSpanWeekend,
            int? mustStartOnDayOfWeek)
        {
            List<Trip> trips = new List<Trip>();

            foreach (Game game in this)
            {
                var tripsToIncludeGameIn = trips.Where(x => x.CanAddGameToTrip(game));
                List<Trip> dupeTripsToAdd = new List<Trip>();

                foreach (Trip trip in tripsToIncludeGameIn)
                {
                    dupeTripsToAdd.Add(new Trip(trip));
                    trip.Add(game);
                }

                trips.AddRange(dupeTripsToAdd);
                trips.Add(new Trip(tripLength, maxTravel, game));
            }

            var potentialTrips = trips.Where(x => x.Count() >= minimumNumberOfGames && 
                                                (!mustSpanWeekend || x.SpansWeekend()) &&
                                                (mustStartOnDayOfWeek == null || (int)x.GetStartingDate().DayOfWeek == mustStartOnDayOfWeek.Value) &&
                                                (mustSeeTeam.Count() == 0 || x.Where(t => mustSeeTeam.Contains(t.AwayTeam.Code) || 
                                                                                            mustSeeTeam.Contains(t.HomeTeam.Code) ||
                                                                                            mustSeeTeam.Contains(t.AwayTeam.Code.ToLower()) ||
                                                                                            mustSeeTeam.Contains(t.HomeTeam.Code.ToLower())).Count() > 0) &&
                                                (string.IsNullOrEmpty(necessaryHomeTeam) || x.Where(t => t.HomeTeam.Code == necessaryHomeTeam).Count() > 0))
                                      .Distinct();

            // Remove trips that are subsets of other trips
            List<Trip> tripsToRemove = new List<Trip>();
            foreach (var trip in potentialTrips)
            {
                if (potentialTrips.Any(x => trip.IsProperSubsetOf(x)))
                {
                    tripsToRemove.Add(trip);
                }
            }

            return potentialTrips.Except(tripsToRemove);
        }

        private async Task InitializeAsync()
        {
            string jsonSchedule = await NetworkUtilities.GetJsonFromApiAsync($"http://live.nhl.com/GameData/SeasonSchedule-{this.yearCode}.json");

            IEnumerable<RawNhlGameInfo> rawGameInfo = JsonConvert.DeserializeObject<List<RawNhlGameInfo>>(jsonSchedule);

            if (rawGameInfo != null)
            {
                foreach (RawNhlGameInfo info in rawGameInfo)
                {
                    this.Add(await Game.CreateGameAsync(info.h, info.a, info.est));
                }
            }
        }

        private string GetCurrentSeasonScheduleYear()
        {
            string years = "";

            // If we are getting games for after July 1st
            if (DateTime.Now.Month > 6 && DateTime.Now.Day > 1)
            {
                years = DateTime.Now.Year.ToString() + (DateTime.Now.Year + 1).ToString();
            }
            else
            {
                years = (DateTime.Now.Year - 1).ToString() + (DateTime.Now.Year).ToString();
            }

            return years;
        }

        private void ValidateConstructorArguments(string yearCode)
        {
            if (yearCode.Length != 8)
            {
                throw new ArgumentException($"'{nameof(yearCode)}' must be 8 characters long");
            }

            if (int.TryParse(yearCode.Substring(0, 4), out int startingYear))
            {
                if (int.TryParse(yearCode.Substring(4), out int endingYear))
                {
                    if (endingYear - startingYear != 1)
                    {
                        throw new ArgumentException($"The first half of '{nameof(yearCode)}' must be exactly 1 less than the second half");
                    }
                }
                else
                {
                    throw new ArgumentException($"The second half of '{nameof(yearCode)}' must be a valid number");
                }
            }
            else
            {
                throw new ArgumentException($"The second half of '{nameof(yearCode)}' must be a valid number");
            }
        }
    }
}
