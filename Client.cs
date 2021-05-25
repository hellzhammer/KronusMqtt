using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KronusMqttLib
{
    public class Client
    {
        public ObservableCollection<string> Messages { get; private set; }

        public string ClientID { get; private set; }
        public bool IsConnected { get; private set; }

        private MqttFactory Factory { get; set; }
        private IMqttClient MqttClient { get; set; }
        private IMqttClientOptions Options { get; set; }

        public Client(string clientID)
        {
            //set main parameters
            this.IsConnected = false;
            this.ClientID = clientID;

            this.Messages = new ObservableCollection<string>();

            // Create a new MQTT client.
            this.Factory = new MqttFactory();
            this.MqttClient = Factory.CreateMqttClient();
            this.Options = new MqttClientOptionsBuilder()
                .WithClientId(ClientID)
                //.WithTcpServer("broker.hivemq.com")
                .WithTcpServer("192.168.0.21")
                //.WithWebSocketServer("broker.hivemq.com:8000/mqtt")
                //.WithCredentials("bud", "%spencer%")
                //.WithTls()
                .WithCleanSession()
                .Build();
        }

        public async Task Kill()
        {
            //do some stuff like un-sub from anything 
            //app is currently listening to.

            //tell server this client is offline.

            //kill it with fire
            await this.MqttClient.DisconnectAsync();
            this.MqttClient.Dispose();
        }

        /// <summary>
        /// Builds the client.
        /// </summary>
        /// <returns>The client.</returns>
        /// <param name="mainTopic">Main topic.</param>
        public virtual async Task BuildClient(string mainTopic)
        {
            // setup the handlers
            MqttClient.UseDisconnectedHandler(async e => await DisconnectionHandler());
            MqttClient.UseApplicationMessageReceivedHandler(e => MessageHandler(e));
            MqttClient.UseConnectedHandler(async e => await this.Subscribe(mainTopic));

            //when done, connect to server
            await Connect();
        }

        /// <summary>
        /// Connect this instance.
        /// </summary>
        /// <returns>The connect.</returns>
        public virtual async Task Connect()
        {
            await MqttClient.ConnectAsync(Options, CancellationToken.None);
            this.IsConnected = MqttClient.IsConnected;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="topic">Topic.</param>
        /// <param name="message_content">Message content.</param>
        public virtual async Task SendMessage(string topic, string message_content)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message_content)
                .WithExactlyOnceQoS()
                //.WithRetainFlag()
                .Build();

            var res = await MqttClient.PublishAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Disconnections the handler.
        /// </summary>
        /// <returns>The handler.</returns>
        public virtual async Task DisconnectionHandler()
        {
            //ensure this is set to false
            this.IsConnected = false;
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await MqttClient.ConnectAsync(Options, CancellationToken.None);
            }
            catch
            {
                //only for testing
                Console.WriteLine("### RECONNECTING FAILED ###");
            }
        }

        /// <summary>
        /// Messages the handler.
        /// </summary>
        /// <param name="e">Event for when messages are recieved.</param>
        public virtual void MessageHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            // for testing only
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Console.WriteLine($"+ Payload = { payload}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
            //will remove above when done this class..
            this.Messages.Add(payload);
        }

        /// <summary>
        /// Subscribe the specified _topic.
        /// </summary>
        /// <returns>The subscribe.</returns>
        /// <param name="_topic">Topic.</param>
        public virtual async Task Subscribe(string _topic)
        {
            //ensure this is set to true
            Console.WriteLine("### CONNECTED WITH SERVER ###");

            // Subscribe to a topic
            await MqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_topic).Build());

            Console.WriteLine("### SUBSCRIBED ###");
        }

        /// <summary>
        /// removes a message from the current collections
        /// </summary>
        /// <param name="toRemove">To remove.</param>
        public void Clear_Message(string toRemove)
        {
            if (this.Messages.Contains(toRemove))
            {
                this.Messages.Remove(toRemove);
            }
        }
    }
}
