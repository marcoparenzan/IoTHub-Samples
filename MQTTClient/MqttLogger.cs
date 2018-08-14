using MQTTnet.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Console;

namespace MQTTClient
{
    class MqttLogger : IMqttNetLogger
    {
        event EventHandler<MqttNetLogMessagePublishedEventArgs> IMqttNetLogger.LogMessagePublished
        {
            add
            {
            }

            remove
            {
            }
        }

        IMqttNetChildLogger IMqttNetLogger.CreateChildLogger(string source)
        {
            return new Child();
        }

        void IMqttNetLogger.Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            WriteLine($"{logLevel} Source={source} Message={message}");
        }
    }

    class Child : IMqttNetChildLogger
    {
        IMqttNetChildLogger IMqttNetChildLogger.CreateChildLogger(string source)
        {
            return new Child();
        }

        void IMqttNetChildLogger.Error(Exception exception, string message, params object[] parameters)
        {
            WriteLine($"Error! {message}");
        }

        void IMqttNetChildLogger.Info(string message, params object[] parameters)
        {
            WriteLine($"Info! {message}");
        }

        void IMqttNetChildLogger.Verbose(string message, params object[] parameters)
        {
            WriteLine($"Verbose! {message}");
        }

        void IMqttNetChildLogger.Warning(Exception exception, string message, params object[] parameters)
        {
            WriteLine($"Warning! {message}");
        }
    }
}
