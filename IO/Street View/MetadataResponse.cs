using Castrimaris.IO.GoogleDataStructures;

namespace Castrimaris.IO.GoogleStreetView {

    /// <summary>
    /// Metadata data returned from Google's StreetView APIs
    /// </summary>
    [System.Serializable]
    public class MetadataResponse {
        public string copyright;
        public string date;
        public Location location;
        public string pano_id;
        public string status;
    }
}