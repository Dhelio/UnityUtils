using Castrimaris.Core.Extensions;
using Castrimaris.IO.Contracts;
using System.Threading.Tasks;

namespace Castrimaris.IO.GoogleStreetView {

    /// <summary>
    /// Base view request for Google Street View's API.
    /// </summary>
    public abstract class ViewRequest : IViewRequest {
        public float Latitude;
        public float Longitude;
        public string Location;

        public ViewRequest(float Latitude, float Longitude, string Location)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            this.Location = (Location.IsNullOrEmpty()) ? null : Location.Replace(" ", "+");
        }

        public ViewRequest(double Latitude, double Longitude, string Location)
        {
            this.Latitude = (float)Latitude;
            this.Longitude = (float)Longitude;
            this.Location = (Location.IsNullOrEmpty()) ? null : Location.Replace(" ", "+");
        }

        /// <summary>
        /// Builds the request
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public abstract Task<string> Build(string Key);

        /// <summary>
        /// Returns the coordinates of the request in an URL friendly format.
        /// </summary>
        public virtual string GetCoordinates() => $"{Latitude.ToStringAmericanStyle()},{Longitude.ToStringAmericanStyle()}";
    }

}