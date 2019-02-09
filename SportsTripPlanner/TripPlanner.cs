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
        private AsyncLazy<NhlSchedule> nhlSchedule;

        public TripPlanner(string yearCode)
        {
            this.yearCode = yearCode;
            nhlSchedule = new AsyncLazy<NhlSchedule>(this.InitializeNhlSchedule);
        }

        public async Task<IEnumerable<Trip>> PlanTripAsync(int tripLength, int minimumNumberOfGames, int maxTravel,
            IEnumerable<string> mustSeeTeam, string necessaryHomeTeam, bool mustSpanWeekend,
            int? mustStartOnDayOfWeek)
        {
            NhlSchedule schedule = await this.nhlSchedule.GetValueAsync();
            return schedule.GetTrips(tripLength, minimumNumberOfGames, maxTravel,
                mustSeeTeam, necessaryHomeTeam, mustSpanWeekend, mustStartOnDayOfWeek);
        }

        private async Task<NhlSchedule> InitializeNhlSchedule()
        {
            return await NhlSchedule.GetNhlScheduleAsync(this.yearCode);
        }
    }
}
