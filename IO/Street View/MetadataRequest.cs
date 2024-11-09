using Castrimaris.Core.Extensions;
using Castrimaris.IO.GoogleDataStructures;
using System.Threading.Tasks;

namespace Castrimaris.IO.GoogleStreetView {

    /// <summary>
    /// Request data model for metadata regarding StreetView's images.
    /// </summary>
    public class MetadataRequest : ViewRequest {

        private const string BASE_REQUEST = "https://maps.googleapis.com/maps/api/streetview/metadata";

        /// <summary>
        /// Constructor of the data model for metadata regarding StreetView's images. Location or coordinates can be used selectively
        /// </summary>
        /// <param name="latitude">Latitude of the view</param>
        /// <param name="longitude">Longitude of the view</param>
        /// <param name="location"></param>
        public MetadataRequest(float latitude = 0.0f, float longitude = 0.0f, string location = null) : base(latitude, longitude, location) { }

        /// <summary>
        /// Constructor of the data model for metadata regarding StreetView's images. Uses the <see cref="Location"/> data structure obtained from directions requests.
        /// </summary>
        /// <param name="location">The latitude and longitude of the view</param>
        public MetadataRequest(Location location) : base(location.Latitude, location.Longitude, null) { }

        /// <inheritdoc cref="ViewRequest.Build(string)"/>
        public override Task<string> Build(string key) {
            var request = $"{BASE_REQUEST}?key={key}";
            request += (Location.IsNullOrEmpty()) ? $"&location={GetCoordinates()}" : Location;
            return Task.FromResult(request);
        }
    }
}