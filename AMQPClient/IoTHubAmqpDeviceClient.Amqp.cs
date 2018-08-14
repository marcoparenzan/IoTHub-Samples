using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Newtonsoft.Json;

namespace AMQPClient
{
    public partial class IoTHubAmqpDeviceClient
    {
        private (SenderLink, ReceiverLink) CreateTwinLinks(string address)
        {
            return CreateCorrelatedLinks(address, $"twin:{Guid.NewGuid()}");
        }

        private (SenderLink, ReceiverLink) CreateDirectMethodsLinks(string address)
        {
            return CreateCorrelatedLinks(address, $"methods:{Guid.NewGuid()}");
        }

        private (SenderLink, ReceiverLink) CreateLinks(string senderAddress, string receiverAddress)
        {
            var senderLink = new SenderLink(Session, GenerateLinkName(), senderAddress);
            var receiverLink = new ReceiverLink(Session, GenerateLinkName(), receiverAddress);
            return (senderLink, receiverLink);
        }

        private (SenderLink, ReceiverLink) CreateCorrelatedLinks(string address, string channelCorrelationId)
        {
            var senderAttach = CreateCorrelatedAttach(channelCorrelationId, target: address);
            var receiverAttach = CreateCorrelatedAttach(channelCorrelationId, source: address);
            var senderLink = new SenderLink(Session, senderAttach.LinkName, senderAttach, (l, a) =>
            {
            });
            var receiverLink = new ReceiverLink(Session, receiverAttach.LinkName, receiverAttach, (l, a) =>
            {
            });
            return (senderLink, receiverLink);
        }

        private Attach CreateCorrelatedAttach(string channelCorrelationId, string source = null, string target = null)
        {
            var attach = new Attach
            {
                Role = false,
                InitialDeliveryCount = 0,
                LinkName = GenerateLinkName(),
                SndSettleMode = SenderSettleMode.Settled,
                RcvSettleMode = ReceiverSettleMode.First,
                Properties = new Amqp.Types.Fields()
            };
            if (!string.IsNullOrWhiteSpace(source)) attach.Source = new Source { Address = source };
            if (!string.IsNullOrWhiteSpace(target)) attach.Target = new Target{ Address = target };
            attach.Properties.Add(new Symbol("com.microsoft:timeout"), "86400");
            attach.Properties.Add(new Symbol("com.microsoft:client-version"), "99");
            attach.Properties.Add(new Symbol("com.microsoft:api-version"), "2018-06-30");
            attach.Properties.Add(new Symbol("com.microsoft:channel-correlation-id"), channelCorrelationId);
            return attach;
        }

        private static string GenerateLinkName()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
