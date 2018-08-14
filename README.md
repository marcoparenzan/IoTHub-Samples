# IoTHub-Samples

This repository contains a collection of relevant IoT Hub experiments. Alle the code must be documented.

## MQTTClient (C#)

A (quite) simple MQTT-based device client rebuild with MQTT.NET library, with CA certificates based authentication (and SAS, obviously). Not a library, just a sample app. It will be transformed into a .NET Standard lib soon.
Implemented: D2C messaging, C2D messaging, GetTwin, Twin DP Patch Update, Twin Reported Properties, Direct Methods invocation

## AMQPClient (C#)

A (quite) simple AMQP-based device client rebuild with AMQP.NET library, with CA certificates based authentication (and SAS, obviously). Not a library, just a sample app. It will be transformed into a .NET Standard lib soon.
Implemented: D2C messaging, C2D messaging, GetTwin, Twin DP Patch Update, Twin Reported Properties, Direct Methods invocation

## Mosquitto MQTT Broker

Configuration of Mosquitto MQTT broker as a bridge to IoT Hub, with SAS and CA certificates authentication

## VerneMQ MQTT Broker

Configuration of VerneMQ MQTT broker as a bridge to IoT Hub, with SAS authentication
