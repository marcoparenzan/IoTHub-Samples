using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMQPClient
{
    public partial class IoTHubAmqpDeviceClient
    {
        public async Task<IoTHubAmqpDeviceClient> SendEventAsJson<TEvent>(TEvent @event)
        {
            return await this.PublishJson(@event);
        }

        public async Task<IoTHubAmqpDeviceClient> SendEventAsString(string @event)
        {
            return await this.PublishString(@event);
        }

        public async Task<IoTHubAmqpDeviceClient> DeclareDesiredPropertiesHandler(Action<JObject> handler)
        {
            await Notification<byte[]>(TwinReceiverLink, async x =>
            {
                handler(x.ToText().ToJObject());
            });
            return this;
        }

        public async Task<IoTHubAmqpDeviceClient> UpdateReportedProperties<TUpdate>(TUpdate update, Action<JObject> handler)
        {
            var correlationId = Guid.NewGuid();
            await this.Telemetry(TwinSenderLink, m => {
                m.MessageAnnotations.Map[new Symbol("operation")] = "PATCH";
                m.MessageAnnotations.Map[new Symbol("resource")] = "/properties/reported";
                m.MessageAnnotations.Map[new Symbol("version")] = null;
                SetMessageBody(m, update);
                m.Properties.SetCorrelationId(correlationId);
            });
            return await Notification<byte[]>(TwinReceiverLink, x => {
                handler(x.ToText().ToJObject());
            });
        }

        public async Task<IoTHubAmqpDeviceClient> DeclareDirectMethodHandler(Func<string, JObject, object> handler)
        {
            await Inquiry(DirectMethodsReceiverLink, DirectMethodsSenderLink, (methodName, methodArgs) =>
            {
                return handler(methodName, methodArgs);
            });
            return this;
        }

        public async Task<IoTHubAmqpDeviceClient> GetTwin(Action<JObject> handler)
        {
            var correlationId = Guid.NewGuid();
            await this.Telemetry(TwinSenderLink, m => {
                m.MessageAnnotations.Map[new Symbol("operation")] = "GET";
                m.Properties.SetCorrelationId(correlationId);
            });
            return await Notification<byte[]>(TwinReceiverLink, x => {
                handler(x.ToText().ToJObject());
            });
        }

        public async Task<IoTHubAmqpDeviceClient> Cloud2DeviceMessages(Action<byte[]> handler)
        {
            return await Notification(ReceiverLink, handler);
        }
    }
}
