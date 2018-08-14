using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MQTTClient
{
    public static class IoTHubMqttDeviceClientExtension
    {
        public static string ToText(this byte[] that)
        {
            if (that == null) return null;
            return Encoding.UTF8.GetString(that);
        }

        public static string ToJson(this object that)
        {
            return JsonConvert.SerializeObject(that);
        }

        public static JObject ToJObject(this string that)
        {
            if (that == null) return null;
            return JsonConvert.DeserializeObject<JObject>(that);
        }
    }
}
