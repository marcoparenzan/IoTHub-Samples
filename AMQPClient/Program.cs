using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static System.Console;

namespace AMQPClient
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            // args
            var iothubName = "vernemq";
            var hostName = $"{iothubName}.azure-devices.net";
            var deviceId = "dev1"; 
            var sharedAccessKey = "";
            var port = 5671;

            // device certificate
            var deviceCertificateBytes = File.ReadAllBytes($"D:\\IoT Hub\\IoTHub Samples\\Certs\\{deviceId}.pfx");
            var deviceCertificatePassword = "1234";
            var deviceCertificate = (X509Certificate2)new X509Certificate2(deviceCertificateBytes, deviceCertificatePassword);

            //var store = new X509Store(StoreLocation.LocalMachine);

            //store.Open(OpenFlags.MaxAllowed);
            //var deviceCertificates = store.Certificates.Find(X509FindType.FindBySubjectName, deviceId, false);
            //var deviceCertificate = deviceCertificates[0];

            //var caCertificateBytes = File.ReadAllBytes($"D:\\IoT Hub\\IoTHub Samples\\rootca.pem");
            //var caCertificatePassword = "1234";
            //var caCertificate = (X509Certificate2)new X509Certificate2(caCertificateBytes, caCertificatePassword);
            //var caCertificates = store.Certificates.Find(X509FindType.FindBySubjectName, "Azure IoT CA TestOnly Root CA", false);
            //var caCertificate = caCertificates[0];

            // using
            using (var client = new IoTHubAmqpDeviceClient(hostName, deviceId, deviceCertificate, port))
            //using (var client = new IoTHubAmqpDeviceClient(hostName, deviceId, sharedAccessKey, port))
            {
                await client.GetTwin((x) =>
                {
                    WriteLine($"Received twin: {x}");
                });
                await client.DeclareDesiredPropertiesHandler((x) =>
                {
                    WriteLine($"Desired Properties: {x}");
                });
                await client.DeclareDirectMethodHandler((methodName, methodArgs) =>
                {
                    return new {
                        A = methodName,
                        B = methodArgs
                    };
                });
                await client.Cloud2DeviceMessages((x) =>
                {
                    WriteLine($"MessageReceived: {x.ToText()}");
                });

                await client.UpdateReportedProperties(new
                {
                    r5 = $"{DateTime.Now}",
                    r6 = $"{DateTime.Now}",
                }, (x) =>
                {
                    Console.Write($"UpdateReportedProperties: {x}");
                });

                var random = new Random();

                while (true)
                {
                    var json = JsonConvert.SerializeObject(new
                    {
                        DeviceId = deviceId,
                        Data = random.Next(0, 100),
                        DateTime = DateTimeOffset.Now
                    });

                    await client.SendEventAsString(json);

                    WriteLine(json);

                    await Task.Delay(1000);
                }
            }
        }
    }
}
