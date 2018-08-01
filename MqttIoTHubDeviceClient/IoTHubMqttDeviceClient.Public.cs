using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubSamples.MqttDevice
{
    public partial class IoTHubMqttDeviceClient
    {
        public IoTHubMqttDeviceClient SendEventAsJson<TEvent>(string deviceId, TEvent @event)
        {
            return this.PublishJson(@event, $"devices/{deviceId}/messages/events/");
        }

        public IoTHubMqttDeviceClient SendEventAsString(string deviceId, string @event)
        {
            return this.PublishString(@event, $"devices/{deviceId}/messages/events/");
        }

        public IoTHubMqttDeviceClient DeclareDesiredPropertiesHandler(Action<JObject> handler)
        {
            this.SubscribeTo($"$iothub/twin/PATCH/properties/desired/", (pp, xx) => {

                handler(JsonConvert.DeserializeObject<JObject>(xx.ToStringMessage()));

            }, true);

            return this;
        }

        public IoTHubMqttDeviceClient GetTwin(Action<JObject> handler)
        {
            var id = Guid.NewGuid();

            this.SubscribeTo($"$iothub/twin/res/200/?$rid={id}", (pp, xx) => {

                var jj = JsonConvert.DeserializeObject<JObject>(xx.ToStringMessage());
                handler(jj);

            }, true);

            this.PublishString(string.Empty, $"$iothub/twin/GET/?$rid={id}");

            return this;
        }

        public IoTHubMqttDeviceClient Cloud2DeviceMessages(string deviceId, Action<Dictionary<string, string>, byte[]> handler)
        {
            return this.SubscribeTo($"devices/{deviceId}/messages/devicebound/#", handler, filterTopic: $"devices/{deviceId}/messages/devicebound/");
        }
    }
}
