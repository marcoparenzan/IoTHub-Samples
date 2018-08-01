using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;

namespace IoTHubSamples.MqttDevice
{
    public static class DataTypeExtension
    {
        public static string ToStringMessage(this byte[] that)
        {
            return Encoding.UTF8.GetString(that);
        }
        public static string ToJson(this object that)
        {
            return JsonConvert.SerializeObject(that);
        }
    }
}
