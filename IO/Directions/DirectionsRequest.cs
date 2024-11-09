using Castrimaris.Core;
using Castrimaris.Core.Extensions;
using Castrimaris.IO.Contracts;
using Castrimaris.IO.GoogleDataStructures;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Castrimaris.IO.GoogleDirections {

    /// <summary>
    /// Class for making requests to Google Directions API in order to receive a <see cref="DirectionsResponse"/>.
    /// </summary>
    [HelpURL("https://developers.google.com/maps/documentation/directions/?hl=it")]
    public class DirectionsRequest : IDirectionsRequest {

        #region Private Fields

        private const string BASE_REQUEST = "https://maps.googleapis.com/maps/api/directions/json?";
        private Location origin;
        private Location destination;
        private TransitModes transitMode;
        private Languages language;
        private DateTime departureTime;

        #endregion Private Fields

        #region Public Constructors

        public DirectionsRequest(Location origin, Location destination, TransitModes transitMode = TransitModes.TRANSIT, Languages language = Languages.Inglese, DateTime departureTime = default) {
            this.origin = origin;
            this.destination = destination;
            this.transitMode = transitMode;
            this.language = language;
            this.departureTime = departureTime;
        }

        #endregion Public Constructors

        #region Public Methods

        public Task<string> Build(string key) {
            var departureTime = (this.departureTime < DateTime.Now) ? "now" : ((DateTimeOffset)this.departureTime).ToUnixTimeSeconds().ToString();
            return Task.FromResult($"{BASE_REQUEST}key={key}&origin={origin}&destination={destination}&mode={transitMode.AsString()}&language={language.AsString()}&departure_time={departureTime}");
        }

        #endregion Public Methods
    }
}