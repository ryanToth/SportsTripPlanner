# SportsTripPlanner
A command line app to help plan sports trips. Currently only looks at the NHL and NBA schedules.

  -t, --tripLength              Required. The number of days that you want your trip to be

  -g, --minNumGames             Required. The minimum number of games that you wish to see during this trip

  -y, --yearCode                The schedule year that you are interested in. eg. 20182019. Will use the current season
                                if left out

  -x, --maxTravel               (Default: 0) The maximum number of KM you are willing to travel between games

  -m, --mustSeeTeams            The teams that must be playing in at least one of the games on the road trip

  -n, --necessaryHomeTeam       At least one game in this trip must be home game for this team

  -w, --mustSpanWeekend         (Default: true) The trip must take place over a Saturday and Sunday

  -d, --mustStartOnDayOfWeek    The day of the week that the trip must start on. e.g. Sunday = 0, Saturday = 6

  -l, --leagues                 (Default: All) List of all of the leagues that you are interested in. e.g. NHL NBA

  -q, --mustIncludeLeagues      List of all of the leagues that at elast game of the trip must be.

  --help                        Display this help screen.

  --version                     Display version information.
