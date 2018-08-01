using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using static System.Console;

namespace IoTHubSamples.MqttDevice
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            // args
            var iotHubName = $"<your iothub name>";
            var hostName = $"{iotHubName}.azure-devices.net";
            var deviceId = "dev1"; // dev1/SAS
            var sharedAccessKey = $"<your device key>";
            var port = 8883;

            // device certificate
            //var deviceCertificateBytes = File.ReadAllBytes($"{deviceId}.pfx");
            //var deviceCertificate = (X509Certificate2)new X509Certificate2(deviceCertificateBytes);

            //var store = new X509Store(StoreLocation.LocalMachine);
            //store.Open(OpenFlags.MaxAllowed);
            ////var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            //var deviceCertificates = store.Certificates.Find(X509FindType.FindBySubjectName, deviceId, false);
            //var deviceCertificate = deviceCertificates[0];

            // ca certificate
            //var caCertificateBytes = File.ReadAllBytes($"RootCA.cer");
            //var caCertificatePassword = "1234";
            //var caCertificate = (X509Certificate2)new X509Certificate2(caCertificateBytes, caCertificatePassword);
            //var caCertificates = store.Certificates.Find(X509FindType.FindBySubjectName, "Azure IoT CA TestOnly Root CA", false);
            //var caCertificate = caCertificates[0];

            // using
            //using (var client = new IoTHubMqttDeviceClient(hostName, deviceId, null, deviceCertificate, port))
            using (var client = new IoTHubMqttDeviceClient(hostName, deviceId, sharedAccessKey, port))
            {
                // vars

                client.GetTwin((x) =>
                {
                    WriteLine($"Received twin: {x.ToJson()}");
                });
                client.DeclareDesiredPropertiesHandler((x) =>
                {
                    WriteLine($"Received twin patch: {x.ToJson()}");
                });
                client.Cloud2DeviceMessages($"dev1", (p, x) =>
                {
                    WriteLine($"MessageReceived: {x}");
                    WriteLine($"PayloadReceived: {p}");
                });

                var random = new Random();

                while (true)
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        DeviceId = deviceId,
                        Data = random.Next(0, 100),
                        Index = 1,
                        DateTime = DateTimeOffset.Now.ToString()
                    });

                    client.SendEventAsString(deviceId, json);

                    WriteLine(json);

                    await Task.Delay(1000);
                }

                ReadLine();
            }
        }
    }
}
