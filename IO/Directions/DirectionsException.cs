using Castrimaris.Core.Monitoring;

namespace Castrimaris.IO.GoogleDirections {

    public class DirectionsException : System.Exception {
        public DirectionsException(string Message) : base(Message) {
            Log.E(this);
        }
    }

}