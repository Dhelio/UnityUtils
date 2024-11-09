

using Unity.Netcode.Transports.UTP;

namespace Castrimaris.Network {

    /// <summary>
    /// A UnityTransport implementation with an added interface for easier decoupling
    /// </summary>
    public class CastrimarisTransport : UnityTransport, ITransport {
        public string GetAddress() {
            return this.ConnectionData.Address;
        }

        public ushort GetPort() {
            return this.ConnectionData.Port;
        }

        public string GetServerListenAddress() {
            return this.ConnectionData.ServerListenAddress;
        }

        public void SetAddress(string Address) {
            this.ConnectionData.Address = Address;
        }

        public void SetPort(ushort Port) {
            this.ConnectionData.Port = Port;
        }

        public void SetServerListenAddress(string ServerListenAddress) {
            this.ConnectionData.ServerListenAddress = ServerListenAddress;
        }

        public new void SetConnectionData(string Address, ushort Port, string ServerListenAddress) {
            base.SetConnectionData(Address, Port, ServerListenAddress);
        }
    }

}