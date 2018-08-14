using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MQTTClient
{
    public partial class IoTHubMqttDeviceClient
    {
        public async Task<IoTHubMqttDeviceClient> SendEventAsJson<TEvent>(TEvent @event)
        {
            return await this.PublishJson(@event, $"devices/{DeviceId}/messages/events/");
        }

        public async Task<IoTHubMqttDeviceClient> SendEventAsString(string @event)
        {
            return await this.PublishString(@event, $"devices/{DeviceId}/messages/events/");
        }

        public async Task<IoTHubMqttDeviceClient> DeclareDesiredPropertiesHandler(Action<JObject> handler)
        {
            return await this.SubscribeTo($"$iothub/twin/PATCH/properties/desired/", (tt, pp, xx) => {

                handler(JsonConvert.DeserializeObject<JObject>(xx.ToText()));

            }, true);
        }

        public async Task<IoTHubMqttDeviceClient> UpdateReportedProperties<TUpdate>(TUpdate update, Action<JObject> handler)
        {
            var requestId = Guid.NewGuid();
            await this.PublishJson(update, $"$iothub/twin/PATCH/properties/reported/?$rid={requestId}");
            return await this.SubscribeTo($"$iothub/twin/res/200/?$rid={requestId}", async (tt, pp, xx) => {

                handler(JsonConvert.DeserializeObject<JObject>(xx.ToText()));

            }, true);
        }

        public async Task<IoTHubMqttDeviceClient> GetTwin(Action<JObject> handler)
        {
            var id = Guid.NewGuid();

            await this.SubscribeTo($"$iothub/twin/res/200/?$rid={id}", (tt, pp, xx) => {

                var jj = JsonConvert.DeserializeObject<JObject>(xx.ToText());
                handler(jj);

            }, true);

            return await this.PublishString(string.Empty, $"$iothub/twin/GET/?$rid={id}");
        }

        public async Task<IoTHubMqttDeviceClient> Cloud2DeviceMessages(Action<Dictionary<string, string>, byte[]> handler)
        {
            return await this.SubscribeTo($"devices/{DeviceId}/messages/devicebound/#", (t, a, b) => {
                handler(a, b);
            }, filterTopic: $"devices/{DeviceId}/messages/devicebound/");
        }

        private static Regex directMethodRequest = new Regex(@"\$iothub\/methods\/POST\/(?<methodName>[a-zA-Z0-9]+)\/\?\$rid\=(?<requestId>[a-zA-Z0-9]+)");
        public async Task<IoTHubMqttDeviceClient> DeclareDirectMethodHandler(Func<string, JObject, object> handler)
        {
            return await this.SubscribeTo($"$iothub/methods/POST/#", async (t,a,b)=> {
                var match = directMethodRequest.Match(t);
                var methodName = match.Groups["methodName"].Value;
                var requestId = match.Groups["requestId"].Value;
                var response = handler(methodName, null);
                var status = 200;
                await this.PublishJson(response, $"$iothub/methods/res/{status}/?$rid={requestId}");
            }, filterTopic: $"$iothub/methods/POST/");
        }
    }
}
