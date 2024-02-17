using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using System.Threading.Tasks;

namespace MQTT
{
    /// <summary>
    /// A class for handling MQTT connections.
    /// 
    /// Author: Jaakko Karjalainen, jaakko.karjalainen@vtt.fi
    /// 
    /// Last update: 19.3.2018
    /// </summary>
    public class MqttConnection
    {
        private MqttClient _client;

        // MQTT connection delegates.
        public delegate void ConnectionEstablishedDelegate(bool success);
        public delegate void ConnectionDisconnectedDelegate(bool success);
        public delegate Task MessageReceivedAsyncDelegate(string topic, string message);
        public delegate void MessageReceivedDelegate(string topic, string message);

        /// <summary>
        /// On MQTT connection established event.
        /// </summary>
        public event ConnectionEstablishedDelegate OnConnectionEstablished;

        /// <summary>
        /// On MQTT connection disconnected event.
        /// </summary>
        public event ConnectionDisconnectedDelegate OnConnectionDisconnected;

        /// <summary>
        /// On MQTT message received async event.
        /// </summary>
        public event MessageReceivedAsyncDelegate OnMessageReceivedAsync;

        /// <summary>
        /// On MQTT message received event.
        /// </summary>
        public event MessageReceivedDelegate OnMessageReceived;

        /// <summary>
        /// Establish a MQTT connection.
        /// </summary>
        /// <param name="url">MQTT broker url</param>
        /// <param name="port">MQTT broker port</param>
        /// <param name="userName">Username required for this connection (optional)</param>
        /// <param name="password">Password required for this connection (optional)</param>
        /// <param name="clientId">Identifier for this client (optional)</param>
        /// <returns></returns>
        public async Task ConnectAsync(string url, int port, string userName = "", string password = "", string clientId = "MQTTClient")
        {
            try
            {
                // Launch the connection routine asynchronously.
                await Task.Run(() => Connect(url, port, userName, password, clientId));
            }
            catch
            {
                // Launch connection establishing failed event.
                OnConnectionEstablished?.Invoke(false);
                throw;
            }
        }

        /// <summary>
        /// The actual MQTT connection establishing routine.
        /// </summary>
        /// <param name="url">MQTT broker url</param>
        /// <param name="port">MQTT broker port</param>
        /// <param name="userName">Username required for this connection (optional)</param>
        /// <param name="password">Password required for this connection (optional)</param>
        /// <param name="clientId">Identifier for this client (optional)</param>
        public void Connect(string url, int port, string userName = "", string password = "", string clientId = "MQTTClient")
        {
            // Just to prevent identical MQTT client names...
            var time = DateTime.UtcNow;
            clientId = clientId + "-" + time.Hour + "-" + time.Minute + "-" + time.Second + "-" + time.Millisecond;

            // For Hololens.
            #if WINDOWS_UWP
			    _client = new MqttClient(url, port, false, MqttSslProtocols.None);

            // For all the other platforms.
            #else
                _client = new MqttClient(url, port, false, null, null, MqttSslProtocols.None);
            #endif

            // Register to callbacks
            _client.MqttMsgPublishReceived += ReceiveAsync;

            // ConnectAsync...
            try
            {
                // If user name is not set, connect without authentication
                if (string.IsNullOrEmpty(userName))
                    _client.Connect(clientId);

                // If user name is set, connect with authentication
                else
                {
                    // Username must contain also a password
                    if (string.IsNullOrEmpty(password))
                        throw new ArgumentNullException(userName + " does not contain a password.");

                    _client.Connect(clientId, userName, password);
                }

                // Launch connection established succesfully event.
                OnConnectionEstablished?.Invoke(true);
            }
            catch
            {
                // Launch connection establishing failed event.
                OnConnectionEstablished?.Invoke(false);
                throw;
            }
        }

        /// <summary>
        /// SubscribeAsync to a MQTT topic.
        /// </summary>
        /// <param name="topic">Topic to subscribe to</param>
        /// <param name="qosLevel">Quality of Service level (0 by default)</param>
        public async Task SubscribeAsync(string topic, byte qosLevel = 0)
        {
            try
            {
                // Launch the subscribe routine asynchronously.
                await Task.Run(() => Subscribe(topic, qosLevel));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// The actual MQTT subscribe routine.
        /// </summary>
        /// <param name="topic">Topic to subscribe to</param>
        /// <param name="qosLevel">Quality of Service level (0 by default)</param>
        public void Subscribe(string topic, byte qosLevel = 0)
        {
            try
            {
                // MQTT subscribe routine wants a list of topics so we'll have to create one from the topic input...
                var topicArray = new string[1];
                topicArray[0] = topic;

                // MQTT subscribe routine wants a list of QoS values so we'll have to create one...
                var qosArray = new byte[1];
                qosArray[0] = qosLevel;

                // Do the actual subscribing.
                _client?.Subscribe(topicArray, qosArray);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// MQTT receive callback handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private async void ReceiveAsync(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs m)
        {
            // Launch the receive handler routine asynchronously.
            await Task.Run(() => Receive(m));
        }

        /// <summary>
        /// MQTT receive handler routine.
        /// </summary>
        /// <param name="m"></param>
        private void Receive(uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs m)
        {
            try
            {
                // Extract the message from the MQTT package.
                var receivedMessage = Encoding.UTF8.GetString(m.Message);

                // Launch message received event with the received message and topic.
                OnMessageReceivedAsync?.Invoke(m.Topic, receivedMessage);
                OnMessageReceived?.Invoke(m.Topic, receivedMessage);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Publish to MQTT.
        /// </summary>
        /// <param name="topic">Topic to publish to</param>
        /// <param name="message">Message to be published</param>
        /// <returns></returns>
        public async Task PublishAsync(string topic, string message)
        {
            try
            {
                // Launch publishing routine asynchronously.
                await Task.Run(() => Publish(topic, message));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// MQTT publishing routine.
        /// </summary>
        /// <param name="topic">Topic to publish to</param>
        /// <param name="message">Message to be published</param>
        public void Publish(string topic, string message)
        {
            try
            {
                // Publish to MQTT.
                _client?.Publish(topic, Encoding.UTF8.GetBytes(message));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// DisconnectAsync from MQTT.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            try
            {
                // Launch disconnection routine asynchronously.
                await Task.Run(() => Disconnect());
            }
            catch
            {
                // Launch MQTT disconnection failed event.
                OnConnectionDisconnected?.Invoke(false);
                throw;
            }
        }

        /// <summary>
        /// MQTT disconnection routine.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_client == null)
                    return;

                // Gracefully terminate the MQTT connection...
                _client.Disconnect();
                _client = null;

                // Launch MQTT disconnected succesfully event.
                OnConnectionDisconnected?.Invoke(true);
            }
            catch
            {
                // Launch MQTT disconnection failed event.
                OnConnectionDisconnected?.Invoke(false);
                throw;
            }
        }
    }
}