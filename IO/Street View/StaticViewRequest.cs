using Castrimaris.Core.Extensions;
using System.Threading.Tasks;

namespace Castrimaris.IO.GoogleStreetView {

    /// <summary>
    /// General request for static views from Google's Street View services.
    /// </summary>
    public class StaticViewRequest : ViewRequest {
        public int Width;
        public int Height;
        public float Pitch;
        public float Heading;
        public float Fov;
        public bool OnlyOutdoor;

        private const string BASE_REQUEST = "https://maps.googleapis.com/maps/api/streetview";

        public StaticViewRequest(
                float Latitude = 0f,
                float Longitude = 0f,
                string Location = null,
                int Width = 1000,
                int Height = 1000,
                float Pitch = 0,
                float Heading = 0,
                bool OnlyOutdoor = true,
                float Fov = 90.0f) : base(Latitude, Longitude, Location) {
            this.Width = Width;
            this.Height = Height;
            this.Pitch = Pitch;
            this.Heading = Heading;
            this.OnlyOutdoor = OnlyOutdoor;
            this.Fov = (Fov < 10 || Fov > 120) ? 90 : Fov;
        }

        public override Task<string> Build(string Key) {
            var request = $"{BASE_REQUEST}?key={Key}&size={Width}x{Height}&heading={Heading.ToStringAmericanStyle()}&pitch={Pitch.ToStringAmericanStyle()}&fov={Fov}&return_error_code=true";
            request += (Location.IsNullOrEmpty()) ? $"&location={Latitude.ToStringAmericanStyle()},{Longitude.ToStringAmericanStyle()}" : $"&location={Location}";
            request += (OnlyOutdoor) ? "&source=outdoor" : "";
            return Task.FromResult(request);
        }
    }

}