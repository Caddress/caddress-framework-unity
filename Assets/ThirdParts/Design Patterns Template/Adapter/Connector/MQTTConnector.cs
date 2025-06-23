using System;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using LitJson;

namespace Caddress.Template.Adapter {
    public class MQTTConnector : IMessageBase{

        private string _connectionURI;
        private MqttClient _client;
        private Task _receiveTask;

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;

        public MQTTConnector(string uri) {
            _connectionURI = uri;
        }

        public bool IsConnected => _client?.IsConnected ?? false;

        public async void Connect() {
            if (_client != null && _client.IsConnected) return;

            try {
                _client = new MqttClient(_connectionURI, 1883, false, null);
                _client.MqttMsgPublishReceived += OnMqttMessageReceived;

                string clientId = Guid.NewGuid().ToString();
                _client.Connect(clientId, "admin", "123");

                if (_client.IsConnected) {
                    OnConnected?.Invoke();
                }
            }
            catch (Exception ex) {
                OnError?.Invoke($"[Connect] {ex.Message}");
            }
        }

        public void Close() {
            try {
                if (_client?.IsConnected ?? false) {
                    _client.Disconnect();
                    OnDisconnected?.Invoke();
                }
            }
            catch (Exception ex) {
                OnError?.Invoke($"[Disconnect] {ex.Message}");
            }
        }

        public void Send(string message) {
            if (_client.IsConnected) {
                JsonData msgJD = JsonMapper.ToObject(message);
                var op = msgJD["op"].ToString();
                var topic = msgJD["Topic"].ToString();
                var msg = msgJD["Message"].ToString();
                byte qos = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;

                if(op == "publish") {
                    bool retain = false;
                    var payload = Encoding.UTF8.GetBytes(msg);
                    _client.Publish(topic, payload, qos, retain);
                }else if(op == "subscribe") {
                    _client.Subscribe(new string[] { topic }, new byte[] { qos });
                }
            }
        }

        private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs e) {
            string msg = Encoding.UTF8.GetString(e.Message);
            OnMessageReceived?.Invoke(msg);
        }

        private void OnConnectionClosed(object sender, EventArgs e) {
            OnDisconnected?.Invoke();
        }
    }
}