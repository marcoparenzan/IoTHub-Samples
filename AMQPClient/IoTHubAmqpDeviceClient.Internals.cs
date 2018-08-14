using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AMQPClient
{
    public partial class IoTHubAmqpDeviceClient
    {
        private async Task<IoTHubAmqpDeviceClient> PublishJson<TEvent>(TEvent @event)
        {
            return await PublishString(JsonConvert.SerializeObject(@event));
        }

        private async Task<IoTHubAmqpDeviceClient> PublishString(string @event)
        {
            return await PublishBytes(Encoding.UTF8.GetBytes(@event));
        }

        private async Task<IoTHubAmqpDeviceClient> PublishBytes(byte[] @event)
        {
            return await Telemetry(SenderLink, (Action<Message>)(m =>
            {
                SetMessageBody(m, @event);
            }));
        }

        private void SetMessageBody(Message m, string body)
        {
            SetMessageBody(m, Encoding.UTF8.GetBytes(body));
        }

        private void SetMessageBody(Message m, byte[] body)
        {
            m.BodySection = new Data() { Binary = body };
        }

        private void SetMessageBody<TObject>(Message m, TObject body)
        {
            SetMessageBody(m, JsonConvert.SerializeObject(body));
        }

        private JObject GetMessageBody(Message m)
        {
            if (m.Body is byte[])
                return JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString((byte[])m.Body));
            else if (m.Body is string)
                return JsonConvert.DeserializeObject<JObject>((string)m.Body);
            else
                throw new NotSupportedException(m.Body.GetType().ToString());
        }

        private async Task<IoTHubAmqpDeviceClient> Telemetry(SenderLink sl, Action<Message> handler)
        {
            var message = NewMessage();
            handler(message);
            await sl.SendAsync(message);
            return this;
        }

        private static Message NewMessage()
        {
            var message = new Message();
            message.Properties = new Properties();
            message.MessageAnnotations = new MessageAnnotations();
            message.ApplicationProperties = new ApplicationProperties();
            return message;
        }

        private async Task<IoTHubAmqpDeviceClient> Notification<TObject>(ReceiverLink rl, Action<TObject> handler)
        {
            rl.Start(1, (l, m) =>
            {
                handler((TObject)m.Body);
                rl.Accept(m);
            });
            return this;
        }

        private async Task<IoTHubAmqpDeviceClient> Correlation<TObject>(ReceiverLink rl, Guid correlationId, Action<TObject> handler)
        {
            rl.Start(1, (l, m) =>
            {
                var cid = (Guid)m.Properties.GetCorrelationId();
                if (cid == correlationId)
                {
                    handler((TObject)m.Body);
                    rl.Accept(m);
                }
            });
            return this;
        }

        private async Task<IoTHubAmqpDeviceClient> Inquiry(ReceiverLink rl, SenderLink sl, Func<string, JObject, object> handler)
        {
            rl.Start(1, async (l, m) =>
            {
                var methodName = m.ApplicationProperties["IoThub-methodname"] as string;
                var correlationId = (Guid) m.Properties.GetCorrelationId();
                var response = handler(methodName, GetMessageBody(m));
                rl.Accept(m);

                await Telemetry(sl, m1 =>
                {
                    m1.Properties.SetCorrelationId(correlationId);
                    m1.ApplicationProperties["IoThub-status"] = 1;
                    SetMessageBody(m1, response);
                });
            });
            return this;
        }
    }
}
