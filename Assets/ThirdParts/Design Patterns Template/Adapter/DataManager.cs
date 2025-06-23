using Caddress.Template.FactoryMethod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using UMySql;
using Unity.VisualScripting;
using UnityEngine;

namespace Caddress.Template.Adapter {
    public enum ConnectorType {
        WebSocket = 0,
        SQLite = 1,
        MQTT = 2,
        MySQL = 3
    }

    public class DataManager : MonoBehaviour {

        private static readonly Dictionary<ConnectorType, Func<string, IDataConnector>> _connectorMap = new Dictionary<ConnectorType, Func<string, IDataConnector>>{
            { ConnectorType.WebSocket, (uri) => new WebSocketConnector(uri) },
            { ConnectorType.SQLite, (uri) => new SQLiteConnector(uri) },
            { ConnectorType.MQTT, (uri) => new MQTTConnector(uri) },
            { ConnectorType.MySQL, (uri) => new MySQLConnector(uri) }
        };

        private readonly Dictionary<ConnectorType, Dictionary<string, IDataConnector>> _connectors =
        new Dictionary<ConnectorType, Dictionary<string, IDataConnector>>();

        public void RegisterConnector(ConnectorType type, string uri) {

            IDataConnector connector = _connectorMap.TryGetValue(type, out var createFunc) ? createFunc(uri) : throw new NotSupportedException($"Unsupported type: {type}");

            if (!_connectors.ContainsKey(type)) {
                _connectors[type] = new Dictionary<string, IDataConnector>();
            }

            _connectors[type][uri] = connector;
        }

        public T GetConnector<T>(ConnectorType type) where T : class, IDataConnector {
            if (_connectors.TryGetValue(type, out var map) && map.Count > 0) {
                return map.Values.First() as T;
            }
            return null;
        }

        public T GetConnector<T>(ConnectorType type, string uri) where T : class, IDataConnector {
            if (_connectors.TryGetValue(type, out var map) && map.TryGetValue(uri, out var connector)) {
                return connector as T;
            }
            return null;
        }

        public void ConnectAll() {
            foreach (var typeMap in _connectors.Values) {
                foreach (var connector in typeMap.Values) {
                    connector.Connect();
                }
            }
                
        }

        public void DisconnectAll() {
            foreach (var typeMap in _connectors.Values) {
                foreach (var connector in typeMap.Values) {
                    connector.Close();
                }
            }
        }
    }
}