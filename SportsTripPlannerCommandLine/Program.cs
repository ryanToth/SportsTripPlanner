using CommandLine;
using SportsTripPlanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsTripPlannerCommandLine
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts =>
                {
                    List<string> leagues = new List<string>();
                    leagues.AddRange(opts.Leagues);
                    leagues.AddRange(opts.MustIncludeLeagues);
                    TripPlanner planner = new TripPlanner(opts.YearCode, leagues, opts.MustIncludeLeagues);

                    IEnumerable<Trip> trips = Task.Run<IEnumerable<Trip>>(async () => {
                        return await planner.PlanTripAsync(opts.TripLength,
                            opts.MinimumNumberOfGames, opts.MaxTravel, opts.MustSeeTeams,
                            opts.NecessaryHomeTeam, opts.MustSpanWeekend, opts.DayOfWeek, opts.AfterTodayOnly);
                    }).Result;

                    Console.Out.WriteLine($"{Environment.NewLine}");
                    Console.Out.WriteLine(string.Join($"{Environment.NewLine}{Environment.NewLine}", trips));
                    Console.Out.WriteLine($"{Environment.NewLine}");
                    Console.Out.WriteLine($"Found {trips.Count()} trip(s).{Environment.NewLine}");
                })
                .WithNotParsed<Options>((errs) => { });
            }
            catch(Exception ex)
            {
                List<string> messagesToPrint = new List<string>(new string[] { "Error Encountered:" });
                
                while (ex != null)
                {
                    messagesToPrint.Add($"{Environment.NewLine}----- {ex.Message} ------{Environment.NewLine}{ex.StackTrace}");
                    ex = ex.InnerException;
                }

                Console.Out.WriteLine(string.Join(Environment.NewLine, messagesToPrint));
            }
        }
    }

    internal class Options
    {
        [Option('t', "tripLength", Required = true, HelpText = "The number of days that you want your trip to be")]
        public int TripLength { get; set; }

        [Option('g', "minNumGames", Required = true, HelpText = "The minimum number of games that you wish to see during this trip")]
        public int MinimumNumberOfGames { get; set; }

        [Option('y', "yearCode", Required = false, Default = null, HelpText = "The schedule year that you are interested in. eg. 20182019. Will use the current season if left out")]
        public string YearCode { get; set; }

        [Option('x', "maxTravel", Required = false, Default = 0, HelpText = "The maximum number of KM you are willing to travel between games")]
        public int MaxTravel { get; set; }

        [Option('m', "mustSeeTeams", Required = false, HelpText = "The teams that must be playing in at least one of the games on the road trip")]
        public IEnumerable<string> MustSeeTeams { get; set; }

        [Option('n', "necessaryHomeTeam", Required = false, HelpText = "At least one game in this trip must be home game for this team")]
        public string NecessaryHomeTeam { get; set; }

        [Option('w', "mustSpanWeekend", Required = false, Default = true, HelpText = "The trip must take place over a Saturday and Sunday")]
        public bool MustSpanWeekend { get; set; }

        [Option('d', "mustStartOnDayOfWeek", Required = false, HelpText = "The day of the week that the trip must start on. e.g. Sunday = 0, Saturday = 6")]
        public int? DayOfWeek { get; set; }

        [Option('l', "leagues", Required = false, Default = "All", HelpText = "List of all of the leagues that you are interested in. e.g. NHL NBA")]
        public IEnumerable<string> Leagues { get; set; }

        [Option('q', "mustIncludeLeagues", Required = false, HelpText = "List of all of the leagues that at elast game of the trip must be")]
        public IEnumerable<string> MustIncludeLeagues { get; set; }

        [Option('a', "afterTodayOnly", Required = false, Default = true, HelpText = "Only consider trips that would take place prior to today's date")]
        public bool AfterTodayOnly { get; set; }
    }
}
