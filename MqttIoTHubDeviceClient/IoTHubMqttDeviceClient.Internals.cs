using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;

namespace IoTHubSamples.MqttDevice
{
    // https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-mqtt-support
    public partial class IoTHubMqttDeviceClient
    {
        const int QoS_AT_MOST_ONCE = 1;

        private IoTHubMqttDeviceClient PublishJson<TEvent>(TEvent @event, string topic)
        {
            // JSON
            var eventJson = JsonConvert.SerializeObject(@event);

            var result = Client.Publish(topic, Encoding.UTF8.GetBytes(eventJson), QoS_AT_MOST_ONCE, false);

            return this;
        }

        private IoTHubMqttDeviceClient PublishString(string @event, string topic)
        {
            var result = Client.Publish(topic, Encoding.UTF8.GetBytes(@event), QoS_AT_MOST_ONCE, false);

            return this;
        }

        private IoTHubMqttDeviceClient PublishBytes<TEvent>(TEvent @event, string topic)
        {
            //// Protobuf
            var eventStream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(eventStream, @event);

            var result = Client.Publish(topic, eventStream.ToArray(), QoS_AT_MOST_ONCE, false);

            return this;
        }

        private IoTHubMqttDeviceClient SubscribeTo(string topic, Action<Dictionary<string, string>, byte[]> x, bool oneShot = false, string filterTopic = null)
        {
            MqttClient.MqttMsgPublishEventHandler handler = null;
            if (filterTopic == null) filterTopic = topic;
            handler = new MqttClient.MqttMsgPublishEventHandler((s, e) =>
            {
                var tt = WebUtility.UrlDecode(e.Topic);
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
                x(p, e.Message);
                if (oneShot)
                    Client.MqttMsgPublishReceived -= handler;
            });
            Client.MqttMsgPublishReceived += handler;

            Client.Subscribe(new string[] { topic }, new byte[] { QoS_AT_MOST_ONCE });

            return this;
        }
    }
}
