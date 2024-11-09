using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using System.Threading.Tasks;

namespace Castrimaris.IO.GoogleStreetView {

    /// <summary>
    /// Request model for Google's equirectangular images from StreetView WebAPIs
    /// </summary>
    // The existence of this class is kind-of an hack; normally the images downloaded from Google Image aren't made to be aligned side-by-side perfectly; as such, when doing just that
    // one can see visible distortions, especially for >45° angles. Just try to download those images via StaticViewRequest and put them side by side to see.
    // Anyway, when capturing the images for StreetView, Google does a bit of math to stitch these images together correctly, and makes them available from its web apis for common users.
    // We can download that stitched image in full using these web apis, however Google's term of use state clearly that the use of those images in commercial products is illegal.
    // As long as this application remains for internal use or demoing use we're golden tho.
    public class EquirectangularViewRequest : ViewRequest {
        public string PanoId;
        public int Zoom;
        public int X;
        public int Y;

        private const string BASE_REQUEST = "https://streetviewpixels-pa.googleapis.com/v1/tile?cb_client=maps_sv.tactile";

        //example request model: https://streetviewpixels-pa.googleapis.com/v1/tile?cb_client=maps_sv.tactile&panoid=cUC4KgxNdgcdhMtG5uD40w&x=2&y=0&zoom=3

        public EquirectangularViewRequest(
                float Latitude = 0.0f,
                float Longitude = 0.0f,
                string Location = null,
                string PanoId = null,
                int Zoom = 4,
                int X = 0,
                int Y = 0) : base(Latitude, Longitude, Location) {
            this.PanoId = PanoId;
            this.Zoom = Zoom;
            this.X = X;
            this.Y = Y;
            //TODO X&Y clamping (zoom=4 -> xCount = 15, yCount = 7)
        }

        public EquirectangularViewRequest(
                double Latitude = 0.0f,
                double Longitude = 0.0f,
                string Location = null,
                string PanoId = null,
                int Zoom = 4,
                int X = 0,
                int Y = 0) : base(Latitude, Longitude, Location) {
            this.PanoId = PanoId;
            this.Zoom = Zoom;
            this.X = X;
            this.Y = Y;
            //TODO X&Y clamping (zoom=4 -> xCount = 15, yCount = 7)
        }

        public override async Task<string> Build(string key) {
            //Request PanoId if necessary
            if (PanoId.IsNullOrEmpty()) {
                PanoId = await StreetView.RequestPanoId(new MetadataRequest(Latitude, Longitude));
            }

            //Build request
            var request = $"{BASE_REQUEST}&panoid={PanoId}&x={X}&y={Y}&zoom={Zoom}";
            return request;
        }
    }

}