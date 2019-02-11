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
            int? mustStartOnDayOfWeek, bool afterTodayOnly)
        {
            List<Trip> trips = new List<Trip>();
            List<Game> allGames = new List<Game>();
            
            // Add all games from all schedules to a single list and sort them
            foreach (AsyncLazy<Schedule> schedule in this.schedules)
            {
                allGames.AddRange(await schedule.GetValueAsync());
            }

            DayOfWeek tripEndDayOfWeekMax = (DayOfWeek)((mustStartOnDayOfWeek != null ? mustStartOnDayOfWeek.Value : 0 + tripLength - 1) % 7);

            foreach (Game game in allGames.OrderBy(x => x.Date))
            {
                // Check that this game takes place on a valid day of the week before considering it
                if ((!afterTodayOnly || game.Date > DateTime.Now.Date) &&
                    (!mustStartOnDayOfWeek.HasValue ||
                    ((Utilities.InBetweenDaysInclusive(game.Date, (DayOfWeek)mustStartOnDayOfWeek.Value, tripEndDayOfWeekMax)))))
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
                                                    (!mustSeeTeam.Any() || SatisfiesMustSeeTeams(x, mustSeeTeam)) &&
                                                    (string.IsNullOrEmpty(necessaryHomeTeam) || x.Where(t => t.HomeTeam.Code == necessaryHomeTeam).Count() > 0))
                                          .Distinct().ToList();

            // Remove trips that are subsets of other trips
            List<Trip> tripsToRemove = new List<Trip>();
            for (int i = potentialTrips.Count() - 1; i >= 0; i--)
            {
                if (potentialTrips.Any(x => potentialTrips[i].IsProperSubsetOf(x)))
                {
                    potentialTrips.Remove(potentialTrips[i]);
                }
            }

            return potentialTrips;
        }

        private static bool SatisfiesMustSeeTeams(Trip trip, IEnumerable<string> mustSeeTeams)
        {
            bool ret = true;
            foreach (string team in mustSeeTeams)
            {
                ret &= trip.Any(x => (x.AwayTeam.Code == team.ToUpper() || x.HomeTeam.Code == team.ToUpper()));
                if (!ret)
                {
                    break;
                }
            }

            return ret;
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
