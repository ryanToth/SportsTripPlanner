using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public class NbaSchedule : Schedule
    {
        private NbaSchedule(string yearCode)
        {
            if (yearCode == null)
            {
                yearCode = this.GetCurrentSeasonScheduleYear();
            }

            this.ValidateConstructorArguments(yearCode);

            this.yearCode = yearCode;
        }

        public static async Task<NbaSchedule> GetScheduleAsync(string yearCode)
        {
            NbaSchedule schedule = new NbaSchedule(yearCode);
            await schedule.InitializeAsync();
            return schedule;
        }

        private async Task InitializeAsync()
        {
            string jsonSchedule = await NetworkUtilities.GetJsonFromApiAsync($"http://data.nba.com/data/10s/v2015/json/mobile_teams/nba/{this.yearCode}/league/00_full_schedule.json");

            RawNbaGameInfo rawGameInfo = JsonConvert.DeserializeObject<RawNbaGameInfo>(jsonSchedule);

            if (rawGameInfo != null)
            {
                foreach (var lscd in rawGameInfo.lscd)
                {
                    foreach (var mscd in lscd.mscd.g)
                    {
                        this.Add(await Game.CreateGameAsync(mscd.h.ta, mscd.v.ta, mscd.etm, League.NBA));
                    }
                }
            }
        }

        private string GetCurrentSeasonScheduleYear()
        {
            string years = "";

            // If we are getting games for after July 1st
            if (DateTime.Now.Month > 6 && DateTime.Now.Day > 1)
            {
                years = DateTime.Now.Year.ToString();
            }
            else
            {
                years = (DateTime.Now.Year - 1).ToString();
            }

            return years;
        }

        private void ValidateConstructorArguments(string yearCode)
        {
            if (yearCode.Length != 8 && yearCode.Length != 4)
            {
                throw new ArgumentException($"'{nameof(yearCode)}' must be 8 or 4 characters long");
            }

            if (!int.TryParse(yearCode.Substring(0, 4), out int startingYear))
            {
                throw new ArgumentException($"'{nameof(yearCode)}' must be a valid number");
            }
        }
    }
}
