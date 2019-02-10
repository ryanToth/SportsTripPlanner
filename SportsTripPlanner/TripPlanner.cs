using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public class TripPlanner
    {
        private string yearCode;
        private League leagues;
        private League mustIncludeLeagues;
        private List<AsyncLazy<Schedule>> schedules = new List<AsyncLazy<Schedule>>();

        public TripPlanner(string yearCode, IEnumerable<string> leagues, IEnumerable<string> mustIncludeLeagues)
        {
            this.yearCode = yearCode;
            this.leagues = this.ParseLeagues(leagues);
            this.mustIncludeLeagues = this.ParseLeagues(mustIncludeLeagues);

            if ((int)(this.leagues & League.NHL) != 0)
            {
                schedules.Add(new AsyncLazy<Schedule>(this.InitializeNhlSchedule));
            }

            if ((int)(this.leagues & League.NBA) != 0)
            {
                schedules.Add(new AsyncLazy<Schedule>(this.InitializeNbaSchedule));
            }
        }

        public async Task<IEnumerable<Trip>> PlanTripAsync(int tripLength, int minimumNumberOfGames, int maxTravel,
            IEnumerable<string> mustSeeTeam, string necessaryHomeTeam, bool mustSpanWeekend,
            int? mustStartOnDayOfWeek)
        {
            List<Trip> trips = new List<Trip>();
            List<Game> allGames = new List<Game>();
            
            // Add all games from all schedules to a single list and sort them
            foreach (AsyncLazy<Schedule> schedule in this.schedules)
            {
                allGames.AddRange(await schedule.GetValueAsync());
            }

            DayOfWeek tripEndDayOfWeekMax = (DayOfWeek)((mustStartOnDayOfWeek.Value + tripLength - 1) % 7);

            foreach (Game game in allGames.OrderBy(x => x.Date))
            {
                // Check that this game takes place on a valid day of the week before considering it
                if (!mustStartOnDayOfWeek.HasValue ||
                    ((Utilities.InBetweenDaysInclusive(game.Date, (DayOfWeek)mustStartOnDayOfWeek.Value, tripEndDayOfWeekMax))))
                {
                    List<Trip> dupeTripsToAdd = new List<Trip>();
                    var tripsToIncludeGameIn = trips.Where(x => x.CanAddGameToTrip(game));

                    foreach (Trip trip in tripsToIncludeGameIn)
                    {
                        dupeTripsToAdd.Add(new Trip(trip));
                        trip.Add(game);
                    }

                    trips.AddRange(dupeTripsToAdd);
                    trips.Add(new Trip(tripLength, maxTravel, game));
                }
            }

            var potentialTrips = trips.Where(x => x.Count() >= minimumNumberOfGames &&
                                                    (mustIncludeLeagues == League.UNK || x.ContainsLeagues(mustIncludeLeagues)) &&
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

            return potentialTrips.Except(tripsToRemove).ToList();
        }

        private League ParseLeagues(IEnumerable<string> leagues)
        {
            League ret = 0;
            foreach (string league in leagues)
            {
                if (Enum.TryParse<League>(league.ToUpper(), out League leagueEnum))
                {
                    ret |= leagueEnum;
                }
                else
                {
                    throw new ArgumentException($"'{league}' is not a valid value for a league");
                }
            }

            return ret;
        }

        private async Task<Schedule> InitializeNhlSchedule()
        {
            return await NhlSchedule.GetScheduleAsync(this.yearCode);
        }

        private async Task<Schedule> InitializeNbaSchedule()
        {
            return await NbaSchedule.GetScheduleAsync(this.yearCode);
        }
    }
}
