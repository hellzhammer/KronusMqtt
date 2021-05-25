using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Net;
using System.Threading.Tasks;

namespace KronusMqttLib
{
    public class Broker
    {
        public bool IsRunning { get; private set; }

        private string BrokerID { get; set; }
        private string IP_Address { get; set; }

        private IMqttServer MqttServer { get; set; }
        private MqttServerOptionsBuilder OptionsBuilder { get; set; }

        /// <summary>
        /// constructor takes and sets the main server ip to start up with
        /// </summary>
        /// <param name="serverIP">Server ip.</param>
        public Broker(string serverIP, string brokerID)
        {
            this.BrokerID = brokerID;
            this.IP_Address = serverIP;
            this.IsRunning = false;
            this.MqttServer = new MqttFactory().CreateMqttServer();
        }

        /// <summary>
        /// builds the options to be used by the broker
        /// </summary>
        private void optionBuild()
        {
            // Configure MQTT server.
            this.OptionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpointBoundIPAddress(IPAddress.Parse(this.IP_Address))
                .WithConnectionBacklog(100)
                .WithConnectionValidator(Connection_Validation)
                .WithApplicationMessageInterceptor(Message_Intercept)
                .WithSubscriptionInterceptor(Sub_Intercept)
                .WithDisconnectedInterceptor((context) => {
                    switch (context.DisconnectType)
                    {
                        case MqttClientDisconnectType.Clean:
                                //do something
                                break;
                        case MqttClientDisconnectType.NotClean:
                                //do something
                                break;
                        case MqttClientDisconnectType.Takeover:
                                //do something
                                break;
                    }

                }).WithClientId(this.BrokerID);
        }

        /// <summary>
        /// validates connections
        /// </summary>
        /// <param name="context">C.</param>
        private void Connection_Validation(MqttConnectionValidatorContext context)
        {
            context.ReasonCode = MqttConnectReasonCode.Success;
        }

        /// <summary>
        /// intercepts messages coming from clients. 
        /// </summary>
        /// <param name="context">Context.</param>
        private void Message_Intercept(MqttApplicationMessageInterceptorContext context)
        {
            //Console.WriteLine(Encoding.UTF8.GetString(context.ApplicationMessage.Payload));
            context.AcceptPublish = true;
        }

        /// <summary>
        /// intercepts subscriptions from clients
        /// </summary>
        /// <param name="context">Context.</param>
        private void Sub_Intercept(MqttSubscriptionInterceptorContext context)
        {
            context.AcceptSubscription = true;
        }

        /// <summary>
        /// start the server
        /// </summary>
        /// <returns>The start.</returns>
        public async Task Start()
        {
            optionBuild();
            await MqttServer.StartAsync(OptionsBuilder.Build());
            if (MqttServer.IsStarted)
            {
                this.IsRunning = true;
                Console.WriteLine("Server started");
                Console.WriteLine(MqttServer.Options.DefaultEndpointOptions.BoundInterNetworkAddress);
            }
            else
            {
                Console.WriteLine("There has been an issue. Please exit and restart the broker.");
            }
        }

        /// <summary>
        /// kill the server
        /// </summary>
        /// <returns>The kill.</returns>
        public async Task Kill()
        {
            await MqttServer.StopAsync();
            MqttServer.Dispose();
            this.IsRunning = false;
        }
    }
}

