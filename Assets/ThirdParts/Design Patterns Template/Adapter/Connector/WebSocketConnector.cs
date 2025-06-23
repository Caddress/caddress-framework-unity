using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Caddress.Template.Adapter {
    public class WebSocketConnector : IMessageBase {

        private string _connectionURI;
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;
        private Task _receiveTask;

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;

        public WebSocketConnector(string uri) {
            _connectionURI = uri;
        }

        public async void Connect() {
            await ConnectAsync(_connectionURI);
        }
        public async Task ConnectAsync(string uri) {
            if (_socket?.State == WebSocketState.Open)
                return;

            _socket = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            try {
                await _socket.ConnectAsync(new Uri(uri), _cts.Token);
                OnConnected?.Invoke();

                _receiveTask = Task.Run(ReceiveLoop);
            }
            catch (Exception ex) {
                OnError?.Invoke(ex.Message);
            }
        }

        public async void Send(string message) {
            await SendAsync(message);
        }
        public async Task SendAsync(string message) {
            if (_socket?.State != WebSocketState.Open) return;

            var bytes = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(bytes);

            try {
                await _socket.SendAsync(segment, WebSocketMessageType.Text, true, _cts.Token);
            }
            catch (Exception ex) {
                OnError?.Invoke(ex.Message);
            }
        }

        public async void Close() {
            await CloseAsync();
        }
        public async Task CloseAsync() {
            try {
                _cts?.Cancel();
                if (_socket?.State == WebSocketState.Open) {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    OnDisconnected?.Invoke();
                }
            }
            catch (Exception ex) {
                OnError?.Invoke(ex.Message);
            }
        }

        private async Task ReceiveLoop() {
            var buffer = new byte[2048];

            while (!_cts.Token.IsCancellationRequested) {
                try {
                    var messageBuffer = new List<byte>();
                    WebSocketReceiveResult result;
                    do {
                        result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        if (result.MessageType == WebSocketMessageType.Close) {
                            await CloseAsync();
                            return;
                        }
                        messageBuffer.AddRange(buffer.Take(result.Count));
                    }
                    while (!result.EndOfMessage);
                    string message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                    OnMessageReceived?.Invoke(message);
                }
                catch (Exception ex) {
                    OnError?.Invoke(ex.Message);
                    break;
                }
            }
        }

        public bool IsConnected => _socket?.State == WebSocketState.Open;
    }
}