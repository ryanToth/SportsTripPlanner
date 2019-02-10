using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public class NhlSchedule : Schedule
    {
        private NhlSchedule(string yearCode)
        {
            if (yearCode == null)
            {
                yearCode = this.GetCurrentSeasonScheduleYear();
            }

            this.ValidateConstructorArguments(yearCode);

            this.yearCode = yearCode;
        }

        public static async Task<NhlSchedule> GetScheduleAsync(string yearCode)
        {
            NhlSchedule schedule = new NhlSchedule(yearCode);
            await schedule.InitializeAsync();
            return schedule;
        }

        private async Task InitializeAsync()
        {
            string jsonSchedule = await NetworkUtilities.GetJsonFromApiAsync($"http://live.nhl.com/GameData/SeasonSchedule-{this.yearCode}.json");

            IEnumerable<RawNhlGameInfo> rawGameInfo = JsonConvert.DeserializeObject<List<RawNhlGameInfo>>(jsonSchedule);

            if (rawGameInfo != null)
            {
                foreach (RawNhlGameInfo info in rawGameInfo)
                {
                    this.Add(await Game.CreateGameAsync(info.h, info.a, info.est, League.NHL));
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
