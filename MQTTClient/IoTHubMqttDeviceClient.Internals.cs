using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace MQTTClient
{
    // https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-mqtt-support
    public partial class IoTHubMqttDeviceClient
    {
        const int QoS_AT_MOST_ONCE = 1;

        private async Task<IoTHubMqttDeviceClient> PublishJson<TEvent>(TEvent @event, string topic)
        {
            await Client.PublishAsync(topic, JsonConvert.SerializeObject(@event), MqttQualityOfServiceLevel.AtMostOnce);

            return this;
        }

        private async Task<IoTHubMqttDeviceClient> PublishString(string @event, string topic)
        {
            await Client.PublishAsync(topic, @event, MqttQualityOfServiceLevel.AtMostOnce);

            return this;
        }

        private async Task<IoTHubMqttDeviceClient> PublishBytes<TEvent>(TEvent @event, string topic)
        {
            ////// Protobuf
            //var eventStream = new MemoryStream();
            //ProtoBuf.Serializer.Serialize(eventStream, @event);

            //await Client.PublishAsync(b => b.WithAtMostOnceQoS().WithTopic(topic).WithPayload(eventStream));

            return this;
        }

        private async Task<IoTHubMqttDeviceClient> SubscribeTo(string topic, Action<string, Dictionary<string, string>, byte[]> x, bool oneShot = false, string filterTopic = null)
        {
            EventHandler<MqttApplicationMessageReceivedEventArgs> handler = null;
            if (filterTopic == null) filterTopic = topic;
            handler = new EventHandler<MqttApplicationMessageReceivedEventArgs>((s, e) =>
            {
                var m = e.ApplicationMessage;
                var tt = WebUtility.UrlDecode(m.Topic);
                if (!tt.StartsWith(filterTopic, StringComparison.InvariantCultureIgnoreCase)) return;
                Dictionary<string, string> p = null;
                if (tt.Length > filterTopic.Length)
                {
                    p = tt.Substring(filterTopic.Length).Split('&').Select(xx => xx.Split('=')).ToDictionary(xx => xx[0], xx => xx[1]);
                }
                else
                {
                    p = new Dictionary<string, string>();
                }
                x(tt, p, m.Payload);
                if (oneShot)
                    Client.ApplicationMessageReceived -= handler;
            });
            Client.ApplicationMessageReceived += handler;

            await Client.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtMostOnce);

            return this;
        }
    }
}
