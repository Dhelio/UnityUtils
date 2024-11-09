namespace Castrimaris.Network.Data {
    [System.Serializable]
    public struct ConnectionAddressData {
        public string ServerListenAddress;
        public ushort? Port;
        public string Address;
    }
}