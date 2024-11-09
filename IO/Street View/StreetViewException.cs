using Castrimaris.Core.Monitoring;

namespace Castrimaris.IO.GoogleStreetView {

    public class StreetViewException : System.Exception {
        public StreetViewException(string Message) : base(Message) {
            Log.E(this);
        }
    }

}