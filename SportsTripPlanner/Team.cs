using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public class Team
    {
        private bool isInitialized;
        public string Code { get; }
        public GeoCoordinate Coordinate { get; private set; }
        public string Name { get; private set; }
        public string Arena { get; private set; }
        public League League { get; private set; }

        // Default TimeZone to EST
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        public Team(string name, string code, string arena, string longitude, string latitude, string timezone)
        {
            this.Name = name;
            this.Code = code;
            this.Arena = arena;
            this.Coordinate = new GeoCoordinate(double.Parse(latitude), double.Parse(longitude));

            if (!string.IsNullOrEmpty(timezone))
            {
                this.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            }
            
            this.isInitialized = true;
        }

        private Team(string code, League league)
        {
            this.Code = code;
            this.League = league;
        }

        public static async Task<Team> GetCityAsync(string code, League league)
        {
            Team city = new Team(code, league);
            await city.InitializeAsync();
            return city;
        }

        public double GetDistanceToInKm(Team city)
        {
            if (!this.IsUnknown && !city.IsUnknown)
            {
                // GetDistanceTo returns distance in meters
                return this.Coordinate.GetDistanceTo(city.Coordinate) / 1000;
            }

            return double.MaxValue;
        }

        public double GetDistanceToInMiles(Team city)
        {
            return this.GetDistanceToInKm(city) * 0.621371;
        }

        public bool IsUnknown
        {
            get
            {
                return this.Coordinate == null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Team city && city.Code == this.Code && city.League == this.League)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private async Task InitializeAsync()
        {
            if (this.isInitialized)
            {
                return;
            }

            IEnumerable<Team> teams = Enumerable.Empty<Team>();
            switch (this.League)
            {
                case League.NHL:
                    teams = await Utilities.NhlCities.GetValueAsync();
                    break;
                case League.NBA:
                    teams = await Utilities.NbaCities.GetValueAsync();
                    break;
            }

            Team thisCity = teams.SingleOrDefault(x => this.Code == x.Code);

            if (thisCity != null)
            {
                this.Name = thisCity.Name;
                this.Arena = thisCity.Arena;
                this.Coordinate = thisCity.Coordinate;
                this.TimeZone = thisCity.TimeZone;
            }

            this.isInitialized = true;
        }
    }
}
