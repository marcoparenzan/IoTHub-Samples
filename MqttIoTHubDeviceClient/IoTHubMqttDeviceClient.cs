using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using uPLibrary.Networking.M2Mqtt;

namespace IoTHubSamples.MqttDevice
{
    public partial class IoTHubMqttDeviceClient: IDisposable
    {
        public string HostName { get; private set; }
        public string DeviceId { get; private set; }
        public string SharedAccessKey { get; private set; }
        public X509Certificate2 CACertificate { get; private set; }
        public X509Certificate2 DeviceCertificate { get; private set; }
        public int Port { get; private set; }

        public string ClientId { get; private set; }
        public string ResourceId { get; private set; }
        public string Username { get; private set; }
        private string Password { get; set; }
        public string DevicePublishTopic { get; private set; }

        public MqttClient Client { get; private set; }
        public byte ConnectionId { get; private set; }

        public IoTHubMqttDeviceClient(string hostName, string deviceId, string sharedAccessKey, int port = 8883)
        {
            HostName = hostName;
            DeviceId = deviceId;
            SharedAccessKey = sharedAccessKey;
            Port = port;

            ClientId = deviceId;
            ResourceId = $"{hostName}/devices/{deviceId}";
            Username = $"{hostName}/{deviceId}/api-version=2016-11-14";
            Password = CreateShareAccessSignature(ResourceId, sharedAccessKey);
            DevicePublishTopic = $"devices/{deviceId}/messages/events/";

            Client = new MqttClient(hostName, port, true, MqttSslProtocols.TLSv1_2, null, null);
            ConnectionId = Client.Connect(ClientId, Username, Password);
        }

        public IoTHubMqttDeviceClient(string hostName, string deviceId, X509Certificate2 caCertificate, X509Certificate2 deviceCertificate, int port = 8883)
        {
            HostName = hostName;
            DeviceId = deviceId;
            CACertificate = caCertificate;
            DeviceCertificate = deviceCertificate;
            Port = port;

            ClientId = deviceId;
            ResourceId = $"{hostName}/devices/{deviceId}";
            DevicePublishTopic = $"devices/{deviceId}/messages/events/";

            Client = new MqttClient(HostName, Port, true, CACertificate, DeviceCertificate, MqttSslProtocols.TLSv1_2);
            Client.MqttMsgPublished += (s, e) =>
            {
            };
            ConnectionId = Client.Connect(ClientId);
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
                Client.Disconnect();
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
            var sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
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
