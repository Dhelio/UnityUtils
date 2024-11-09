using Castrimaris.Core.Monitoring;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Castrimaris.Network {

    /// <summary>
    /// A <see cref="WebSocket"/> implementation that loosely follows RFC standards and provides automatic callbacks for general events tied to the socket.
    /// </summary>
    public class ManagedWebSocket {

        #region Private Variables

        private ClientWebSocket socket;
        private Dictionary<string, string> headers;
        private string endpoint;
        private List<string> messageBuffer;
        private Task receiveTask;

        #endregion

        #region Events

        public Action OnOpen;
        public Action OnClose;
        public Action OnError;
        public Action<string> OnMessage;
        public Action<string> OnPartialMessage;

        #endregion

        #region Properties

        public Dictionary<string, string> Headers => headers;

        #endregion

        #region Constructors

        public ManagedWebSocket(string endpoint, Dictionary<string, string> headers) {
            this.headers = headers;
            this.endpoint = endpoint;

            messageBuffer = new List<string>();
            headers = new Dictionary<string, string>();

            socket = new ClientWebSocket();
        }

        #endregion

        #region Public Methods

        public bool AddHeader(string name, string value) {
            if (headers.ContainsKey(name))
                return false;
            headers.Add(name, value);
            return true;
        }

        public void ClearHeaders() => headers.Clear();

        public async Task Open(CancellationToken cancellationToken = default) {


            foreach (var header in headers) {
                socket.Options.SetRequestHeader(header.Key, header.Value);
            }

            var uri = new Uri(endpoint);
            await socket.ConnectAsync(uri, cancellationToken);

            Task.Run(Receive);

            OnOpen?.Invoke();
        }

        public async Task Send(string message) {
            if (socket.State != WebSocketState.Open && socket.State != WebSocketState.CloseReceived) {  //Can only send data when the socket is open or the server is waiting for a close confirm
                Log.E($"Tried to send a message, but the WebSocket is not Open or CloseReceived! State: {socket.State}");
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(bytes);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, default);
        }

        public async Task Close(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure) {
            if (socket.State == WebSocketState.Closed)
                return;

            await socket.CloseAsync(closeStatus, closeStatus.ToString(), default);
            OnClose?.Invoke();
        }

        #endregion

        #region Private Methods

        private async Task Receive() {
            while (socket.State == WebSocketState.Open) {
                var buffer = new byte[1024 * 4];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                switch (result.MessageType) {
                    case WebSocketMessageType.Close:
                        Log.W($"Received close message from server.");
                        await Close(closeStatus: WebSocketCloseStatus.EndpointUnavailable);
                        break;
                    case WebSocketMessageType.Text:
                        try {
                            var message = Encoding.UTF8.GetString(buffer);
                            Log.D($"Received message: {message}");
                            //OnPartialMessage?.Invoke(message);
                            messageBuffer.Add(message);
                            if (result.EndOfMessage) {
                                OnMessage?.Invoke(string.Join("", messageBuffer));
                                messageBuffer.Clear();
                            }
                        } catch (Exception e) {
                            Log.E($"Something went wrong when receiving: {e}");
                        }
                        break;
                    case WebSocketMessageType.Binary:
                        var binaryMessage = Encoding.UTF8.GetString(buffer);
                        Log.D($"Received binary message: {binaryMessage}");
                        break;
                    default:
                        Log.E($"No such message type!");
                        break;
                }
            }
            Log.D($"Stopping receive because socket is closing...");
            await Task.CompletedTask;
        }
        #endregion
    }
}
