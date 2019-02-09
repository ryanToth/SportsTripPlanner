using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    internal class NhlSchedule : HashSet<Game>
    {
        private string yearCode;
        private AsyncManualResetEvent initialized = new AsyncManualResetEvent();
        
        public NhlSchedule(string yearCode)
        {
            this.ValidateConstructorArguments(yearCode);

            this.yearCode = yearCode;

            Task.Run(async () => 
            {
                await Initialize();
            });
        }

        public IEnumerable<Trip> GetTrips(int tripLength, int minimumNumberOfGames, int maxTravel, IEnumerable<string> mustSeeTeam, string necessaryHomeTeam)
        {
            List<Trip> trips = new List<Trip>();
            // Make sure the schedule is initialized before querying it
            Task.Run(async () => await this.initialized.WaitAsync()).Wait();

            foreach (Game game in this)
            {
                var tripsToIncludeGameIn = trips.Where(x => x.CanAddGameToTrip(game, tripLength, maxTravel));
                if (tripsToIncludeGameIn != null)
                {
                    foreach (Trip trip in tripsToIncludeGameIn)
                    {
                        trip.Add(game);
                    }
                }

                trips.Add(new Trip(game));
            }

            return trips.Where(x => x.Count() >= minimumNumberOfGames && 
                                    (mustSeeTeam.Count() == 0 || x.Where(t => mustSeeTeam.Contains(t.AwayTeam.Code) || 
                                                                              mustSeeTeam.Contains(t.HomeTeam.Code) ||
                                                                              mustSeeTeam.Contains(t.AwayTeam.Code.ToLower()) ||
                                                                              mustSeeTeam.Contains(t.HomeTeam.Code.ToLower())).Count() > 0) &&
                                    (string.IsNullOrEmpty(necessaryHomeTeam) || x.Where(t => t.HomeTeam.Code == necessaryHomeTeam).Count() > 0));
        }

        private async Task Initialize()
        {
            string jsonSchedule = await NetworkUtilities.GetJsonFromApiAsync($"http://live.nhl.com/GameData/SeasonSchedule-{this.yearCode}.json");

            IEnumerable<RawNhlGameInfo> rawGameInfo = JsonConvert.DeserializeObject<List<RawNhlGameInfo>>(jsonSchedule);

            if (rawGameInfo != null)
            {
                this.UnionWith(rawGameInfo.Select(x => new Game(x.h, x.a, x.est)));
            }

            this.initialized.Set();
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
            if (yearCode == null)
            {
                this.yearCode = this.GetCurrentSeasonScheduleYear();
                return;
            }

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
