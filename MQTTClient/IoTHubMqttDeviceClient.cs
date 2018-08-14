using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Serializer;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MQTTClient
{
    public partial class IoTHubMqttDeviceClient: IDisposable
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
        public string DevicePublishTopic { get; private set; }

        public IMqttClient Client { get; private set; }

        public IoTHubMqttDeviceClient(string hostName, string deviceId, string sharedAccessKey, int port = 8883)
        {
            HostName = hostName;
            DeviceId = deviceId;
            SharedAccessKey = sharedAccessKey;
            Port = port;

            ClientId = deviceId;
            ResourceId = $"{hostName}/devices/{deviceId}";
            Username = $"{hostName}/{deviceId}/?api-version=2018-06-30";
            Password = CreateShareAccessSignature(ResourceId, SharedAccessKey);
            DevicePublishTopic = $"devices/{deviceId}/messages/events/";

            var options = (new MqttClientOptionsBuilder())
                .WithClientId(DeviceId)
                .WithCommunicationTimeout(TimeSpan.FromSeconds(5))
                .WithProtocolVersion(MqttProtocolVersion.V311)
                .WithTcpServer(HostName, Port)
                .WithCredentials(Username, Password)
                .WithTls()
                .Build()
            ;

            Client = new MqttFactory().CreateMqttClient();

            Client.ApplicationMessageReceived += (s, e) => {
                Console.WriteLine($"{e.ApplicationMessage}");
            };
            Client.Connected += (s, e) => {
                Console.WriteLine($"{e}");
            };
            Client.Disconnected += (s, e) => {
                Console.WriteLine($"{e}");
            };

            Client.ConnectAsync(options).Wait();
        }

        public IoTHubMqttDeviceClient(string hostName, string deviceId, X509Certificate2 deviceCertificate, int port = 8883)
        {
            HostName = hostName;
            DeviceId = deviceId;
            DeviceCertificate = deviceCertificate;
            Port = port;

            ClientId = deviceId;
            ResourceId = null; // nullify everything
            Username = $"{hostName}/{deviceId}/?api-version=2018-06-30";
            Password = null; // nullify everything
            DevicePublishTopic = $"devices/{deviceId}/messages/events/";

            var options = (new MqttClientOptionsBuilder())
                .WithClientId(deviceId)
                .WithKeepAlivePeriod(new TimeSpan(0, 0, 0, 40))
                .WithCommunicationTimeout(TimeSpan.FromSeconds(5))
                .WithProtocolVersion(MQTTnet.Serializer.MqttProtocolVersion.V311)
                .WithTcpServer(HostName, Port)
                .WithCredentials(Username, Password)
                .WithTls(true, false, false, deviceCertificate.Export(X509ContentType.Cert))
                .Build()
            ;

            Client = new MqttFactory().CreateMqttClient(new MqttLogger());

            Client.ApplicationMessageReceived += (s, e) => {
                Console.WriteLine($"{e.ApplicationMessage}");
            };
            Client.Connected += (s, e) => {
                Console.WriteLine($"{e}");
            };
            Client.Disconnected += (s, e) => {
                Console.WriteLine($"{e}");
            };

            Client.ConnectAsync(options).Wait();
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
                Client.DisconnectAsync().Wait();
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
