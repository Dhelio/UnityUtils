using Castrimaris.Core.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Castrimaris.IO.GoogleDataStructures {

    /// <summary>
    /// Response received from Google's Directions API service. Class has been generated through <see cref="https://json2csharp.com/">a JSON to C# generator</see>.
    /// </summary>
    [System.Serializable]
    public class DirectionsResponse {
        [JsonProperty("geocoded_waypoints")]
        public List<GeocodedWaypoint> GeocodedWaypoints;

        [JsonProperty("routes")]
        public List<Route> Routes;

        [JsonProperty("status")]
        public string Status;
    }

    [System.Serializable]
    public class Agency {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("url")]
        public string Url;
    }

    [System.Serializable]
    public class ArrivalStop {
        [JsonProperty("location")]
        public Location Location;

        [JsonProperty("name")]
        public string Name;
    }

    [System.Serializable]
    public class ArrivalTime {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("time_zone")]
        public string TimeZone;

        [JsonProperty("value")]
        public int Value;
    }

    [System.Serializable]
    public class Bounds {
        [JsonProperty("northeast")]
        public Location Northeast;

        [JsonProperty("southwest")]
        public Location Southwest;
    }

    [System.Serializable]
    public class DepartureStop {
        [JsonProperty("location")]
        public Location Location;

        [JsonProperty("name")]
        public string Name;
    }

    [System.Serializable]
    public class DepartureTime {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("time_zone")]
        public string TimeZone;

        [JsonProperty("value")]
        public int Value;
    }

    [System.Serializable]
    public class Distance {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("value")]
        public int Value;
    }

    [System.Serializable]
    public class Duration {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("value")]
        public int Value;
    }

    [System.Serializable]
    public class Location {

        [JsonProperty("lat")]
        public double Latitude;

        [JsonProperty("lng")]
        public double Longitude;

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString() => $"{Latitude.ToAmericanStyle()},{Longitude.ToAmericanStyle()}"; //Overridden for convenience when generating the DirectionsRequests
    }

    [System.Serializable]
    public class GeocodedWaypoint {
        [JsonProperty("geocoder_status")]
        public string GeocoderStatus;

        [JsonProperty("place_id")]
        public string PlaceId;

        [JsonProperty("types")]
        public List<string> Types;
    }

    [System.Serializable]
    public class Leg {
        [JsonProperty("arrival_time")]
        public ArrivalTime ArrivalTime;

        [JsonProperty("departure_time")]
        public DepartureTime DepartureTime;

        [JsonProperty("distance")]
        public Distance Distance;

        [JsonProperty("duration")]
        public Duration Duration;

        [JsonProperty("end_address")]
        public string EndAddress;

        [JsonProperty("end_location")]
        public Location EndLocation;

        [JsonProperty("start_address")]
        public string StartAddress;

        [JsonProperty("start_location")]
        public Location StartLocation;

        [JsonProperty("steps")]
        public List<Step> Steps;

        [JsonProperty("traffic_speed_entry")]
        public List<object> TrafficSpeedEntry;

        [JsonProperty("via_waypoint")]
        public List<object> ViaWaypoint;
    }

    [System.Serializable]
    public class Line {
        [JsonProperty("agencies")]
        public List<Agency> Agencies;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("short_name")]
        public string ShortName;

        [JsonProperty("vehicle")]
        public Vehicle Vehicle;
    }

    [System.Serializable]
    public class OverviewPolyline {
        [JsonProperty("points")]
        public string Points;
    }

    [System.Serializable]
    public class Polyline {
        [JsonProperty("points")]
        public string Points;
    }

    [System.Serializable]
    public class Route {
        [JsonProperty("bounds")]
        public Bounds Bounds;

        [JsonProperty("copyrights")]
        public string Copyrights;

        [JsonProperty("legs")]
        public List<Leg> Legs;

        [JsonProperty("overview_polyline")]
        public OverviewPolyline OverviewPolyline;

        [JsonProperty("summary")]
        public string Summary;

        [JsonProperty("warnings")]
        public List<string> Warnings;

        [JsonProperty("waypoint_order")]
        public List<object> WaypointOrder;
    }

    [System.Serializable]
    public class Step {
        [JsonProperty("distance")]
        public Distance Distance;

        [JsonProperty("duration")]
        public Duration Duration;

        [JsonProperty("end_location")]
        public Location EndLocation;

        [JsonProperty("html_instructions")]
        public string HtmlInstructions;

        [JsonProperty("polyline")]
        public Polyline Polyline;

        [JsonProperty("start_location")]
        public Location StartLocation;

        [JsonProperty("steps")]
        public List<Step> Steps;

        [JsonProperty("travel_mode")]
        public string TravelMode;

        [JsonProperty("transit_details")]
        public TransitDetails TransitDetails;

        [JsonProperty("maneuver")]
        public string Maneuver;
    }

    [System.Serializable]
    public class TransitDetails {
        [JsonProperty("arrival_stop")]
        public ArrivalStop ArrivalStop;

        [JsonProperty("arrival_time")]
        public ArrivalTime ArrivalTime;

        [JsonProperty("departure_stop")]
        public DepartureStop DepartureStop;

        [JsonProperty("departure_time")]
        public DepartureTime DepartureTime;

        [JsonProperty("headsign")]
        public string Headsign;

        [JsonProperty("line")]
        public Line Line;

        [JsonProperty("num_stops")]
        public int NumStops;
    }

    [System.Serializable]
    public class Vehicle {
        [JsonProperty("icon")]
        public string Icon;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("type")]
        public string Type;
    }



}