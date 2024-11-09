namespace Castrimaris.Network {

    public interface ITransport {
        public ushort GetPort();
        public void SetPort(ushort Port);
        public string GetAddress();
        public void SetAddress(string Address);
        public string GetServerListenAddress();
        public void SetServerListenAddress(string ServerListenAddress);
        public void SetConnectionData(string Address, ushort Port, string ServerListenAddress);
    }

}