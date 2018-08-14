using Amqp;
using Amqp.Framing;
using Amqp.Types;
using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AMQPClient
{
    public partial class IoTHubAmqpDeviceClient : IDisposable
    {
        public string HostName { get; private set; }
        public string DeviceId { get; private set; }
        public string SharedAccessKey { get; private set; }
        public X509Certificate DeviceCertificate { get; private set; }
        public int Port { get; private set; }

        public string ClientId { get; private set; }
        public string ResourceId { get; private set; }
        public string Username { get; private set; }
        private string Password { get; set; }

        public Connection Connection { get; private set; }
        public Session Session { get; private set; }

        public SenderLink SenderLink { get; private set; }
        public ReceiverLink ReceiverLink { get; private set; }
        public SenderLink TwinSenderLink { get; private set; }
        public ReceiverLink TwinReceiverLink { get; private set; }
        public SenderLink DirectMethodsSenderLink { get; private set; }
        public ReceiverLink DirectMethodsReceiverLink { get; private set; }

        public IoTHubAmqpDeviceClient(string hostName, string deviceId, string sharedAccessKey, int port = 5671)
        {
            HostName = hostName;
            DeviceId = deviceId;
            SharedAccessKey = sharedAccessKey;
            Port = port;

            ClientId = deviceId;
            ResourceId = $"{hostName}/devices/{deviceId}";
            Username = $"{hostName}/{deviceId}/?api-version=2018-06-30";
            Password = CreateShareAccessSignature(ResourceId, SharedAccessKey);

            var factory = new ConnectionFactory();
            var address = new Address(HostName, Port, Username, Password);
            Connection = factory.CreateAsync(address).Result;
            Session = new Session(Connection);

            (SenderLink, ReceiverLink) = CreateLinks($"/devices/{DeviceId}/messages/events", $"/devices/{DeviceId}/messages/deviceBound");
            (TwinSenderLink, TwinReceiverLink) = CreateTwinLinks($"/devices/{DeviceId}/twin");
            (DirectMethodsSenderLink, DirectMethodsReceiverLink) = CreateDirectMethodsLinks($"/devices/{DeviceId}/methods/deviceBound");
        }

        public IoTHubAmqpDeviceClient(string hostName, string deviceId, X509Certificate2 deviceCertificate, int port = 5671)
        {
            HostName = hostName;
            DeviceId = deviceId;
            DeviceCertificate = deviceCertificate;
            Port = port;

            ClientId = deviceId;
            ResourceId = $"{hostName}/devices/{deviceId}";
            Username = $"{hostName}/{deviceId}/?api-version=2018-06-30";

            var factory = new ConnectionFactory();
            factory.SSL.ClientCertificates.Add(deviceCertificate);
            factory.SSL.Protocols = SslProtocols.Tls12;
            factory.SSL.CheckCertificateRevocation = false;

            var address = new Address(HostName, Port);
            Connection = factory.CreateAsync(address).Result;
            Session = new Session(Connection);

            (SenderLink, ReceiverLink) = CreateLinks($"/devices/{DeviceId}/messages/events", $"/devices/{DeviceId}/messages/deviceBound");
            (TwinSenderLink, TwinReceiverLink) = CreateTwinLinks($"/devices/{DeviceId}/twin");
            (DirectMethodsSenderLink, DirectMethodsReceiverLink) = CreateDirectMethodsLinks($"/devices/{DeviceId}/methods/deviceBound");
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                Session.Close();
                Connection.Close();
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IoTHubMqttDeviceClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        private static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private string CreateShareAccessSignature(string resourceUri, string key, int timeToLive = 86400)
        {
            var sinceEpoch = DateTime.UtcNow - EpochTime;
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + timeToLive);
            string text2 = WebUtility.UrlEncode(resourceUri);

            string value;
            using (HMACSHA256 hMACSHA = new HMACSHA256(Convert.FromBase64String(key)))
            {
                value = Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes($"{text2}\n{expiry}")));
            }

            return $"SharedAccessSignature sr={text2}&sig={WebUtility.UrlEncode(value)}&se={expiry}";
        }
    }
}
