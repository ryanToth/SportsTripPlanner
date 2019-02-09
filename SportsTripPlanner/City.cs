using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    internal class City
    {
        private bool isInitialized;
        public string Code { get; }
        public GeoCoordinate Coordinate { get; private set; }
        public string Name { get; private set; }
        public string Arena { get; private set; }

        // Default TimeZone to EST
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        public City(string code)
        {
            this.Code = code;
            this.InitializeAsync().Wait();
        }

        public City(string name, string code, string arena, string longitude, string latitude, string timezone)
        {
            this.Name = name;
            this.Code = code;
            this.Arena = arena;
            this.Coordinate = new GeoCoordinate(double.Parse(latitude), double.Parse(longitude));
            this.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            this.isInitialized = true;
        }

        public double GetDistanceToInKm(City city)
        {
            if (!this.IsUnknown && !city.IsUnknown)
            {
                // GetDistanceTo returns distance in meters
                return this.Coordinate.GetDistanceTo(city.Coordinate) / 1000;
            }

            return double.MaxValue;
        }

        public double GetDistanceToInMiles(City city)
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
            if (obj is City city && city.Code == this.Code)
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

            City thisCity = (await Utilities.Cities.GetValueAsync()).SingleOrDefault(x => this.Code == x.Code);

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
