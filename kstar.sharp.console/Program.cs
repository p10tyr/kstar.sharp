﻿using MQTTnet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace kstar.sharp.console
{
    //UDP Multicast must be supported if running in VM or Continers. Needs full access to network.
    //example kstar.console.exe --ip-192.168.1.50 --sqlite-"Data Source=c:\sqlite\inverter-data.db"

    internal class Program
    {
        private static int REFRESH_SECONDS = 5;
        private static string IP_ADDRESS_INVERTER = "0.0.0.0";
        private static kstar.sharp.datacollect.Client client;

        //private static string SQL_LITE_CONNECTION_STRING = "";
        private static string MQTT_CONNECTION_STRING = "homeassistant";
        private static bool SILENT_MODE = false;


        private static IMqttClient mqttClient;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("------------------------");

            //parseArguments(args);
            parseEnvironmentVariables();

            if (string.IsNullOrWhiteSpace(IP_ADDRESS_INVERTER))
            {
                Console.WriteLine("You must pass in the Inverters IP address or 0.0.0.0 for broadcast request");
                Console.ReadLine();
                return;
            }

            //Initialise the client
            client = new sharp.datacollect.Client(IP_ADDRESS_INVERTER);

            // Display some information
            Console.WriteLine("");
            Console.WriteLine("Starting UDP Broadcast");// on port: " + receiverPort);
            Console.WriteLine("-------------------------------\n");

            while (!client.BroadcastRequest_ResponseRecieved)
            {
                Console.WriteLine("Sending Broadcast Request to " + IP_ADDRESS_INVERTER);
                client.SendBroadcastRequest();
                Thread.Sleep(2000);
            }
            IP_ADDRESS_INVERTER = client.IPAddressInverter;
            Console.WriteLine("IP Address set to " + IP_ADDRESS_INVERTER);
            Console.WriteLine("");


            Console.WriteLine("Configuring MQTT");
            Console.WriteLine("-------------------------------\n");
            if (string.IsNullOrWhiteSpace(MQTT_CONNECTION_STRING))
                Console.WriteLine("MQTT is disabled because no connection string passed in");
            await ConfigureAndConnectMqtt();
            Console.WriteLine("");

            // Display some information
            Console.WriteLine("Starting UDP Data");// on port: " + receiverPort);
            Console.WriteLine("-------------------------------\n");
            client.DataRecieved += new kstar.sharp.datacollect.DataRecievedEventHandler(DataRecievedUpdateConsole);
            //while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)) //does not work in docker!


            while (true)  // ctrl+c or sigterm kills this
            {
                try
                {
                    client.SendDataRequest();

                }
                catch (Exception x)
                {
                    Console.WriteLine("SendDataRequest FAILED - " + x.Message);

                }

                await Task.Delay(REFRESH_SECONDS * 1000);

                if (!SILENT_MODE)
                    Console.WriteLine("Refresh...");
            }
        }


        private static async Task ConfigureAndConnectMqtt()
        {
            var factory = new MqttClientFactory();

            mqttClient = factory.CreateMqttClient();

            if (string.IsNullOrWhiteSpace(MQTT_CONNECTION_STRING))
                return;

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(MQTT_CONNECTION_STRING, 1883) // Port is optional
                .WithClientId("kstar.sharp.console")
                .WithCredentials("mqtt", "mqtt")
                .Build();

            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None);

            }
            catch (Exception x)
            {
                Console.WriteLine("Could not connect to MQTT - " + x.Message);
            }

            //mqttClient.UseDisconnectedHandler(async e =>
            //{
            //    Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            //    await Task.Delay(TimeSpan.FromSeconds(5));

            //    try
            //    {
            //        await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
            //    }
            //    catch
            //    {
            //        Console.WriteLine("### RECONNECTING FAILED ###");
            //    }
            //});

        }

        private static async Task PublishSensorTopic(domain.Models.InverterData inverterDataModel)
        {
            if (!SILENT_MODE)
                Console.WriteLine("Publishing MQTT Topics");

            try
            {
                if (!mqttClient.IsConnected)
                    await ConfigureAndConnectMqtt();

                if (!mqttClient.IsConnected)
                    return;

                decimal grid_import = 0m;
                decimal grid_export = 0m;
                if (inverterDataModel.GridData.GridPower > 0)
                {
                    grid_import = inverterDataModel.GridData.GridPower;
                }
                else
                {
                    grid_export = inverterDataModel.GridData.GridPower * -1;
                }

                await mqttClient.PublishAsync(CreateMqttMessage("sensor/inverter/pvpower", inverterDataModel.PVData.PVPower.ToString()));
                await mqttClient.PublishAsync(CreateMqttMessage("sensor/inverter/grid", inverterDataModel.GridData.GridPower.ToString()));
                await mqttClient.PublishAsync(CreateMqttMessage("sensor/inverter/grid/import", grid_import.ToString()));
                await mqttClient.PublishAsync(CreateMqttMessage("sensor/inverter/grid/export", grid_export.ToString()));
                await mqttClient.PublishAsync(CreateMqttMessage("sensor/inverter/load", inverterDataModel.LoadData.LoadPower.ToString()));
                await mqttClient.PublishAsync(CreateMqttMessage("sensor/inverter/temp", inverterDataModel.StatData.InverterTemperature.ToString()));
                await mqttClient.PublishAsync(CreateMqttMessage("sensor/inverter/etoday", inverterDataModel.StatData.EnergyToday.ToString()));
            }
            catch (Exception x)
            {
                Console.WriteLine($"ERROR - Publishing MQTT Topics ${x.Message}");
            }

        }

        private static MqttApplicationMessage CreateMqttMessage(string topic, string value)
        {
            return new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(value)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();
        }


        static int count = 999;
        private static void DataRecievedUpdateConsole(domain.Models.InverterData inverterDataModel)
        {
            count++;

            if (SILENT_MODE)
            {
                if (count > 100)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} {inverterDataModel.PVData} {inverterDataModel.GridData} {inverterDataModel.LoadData} {inverterDataModel.StatData}");
                    count = 0;
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " (localtime)");
                Console.WriteLine(inverterDataModel.PVData);
                Console.WriteLine(inverterDataModel.GridData);
                Console.WriteLine(inverterDataModel.LoadData);
                Console.WriteLine(inverterDataModel.BatteryData);
                Console.WriteLine(inverterDataModel.StatData);
                Console.WriteLine(string.Empty);
            }

            Task.Run(async () => await PublishSensorTopic(inverterDataModel));

        }


        //private static void parseArguments(string[] args)
        //{
        //    for (int i = 0; i < args.Length; i++)
        //    {
        //        if (args[i].StartsWith("--ip-"))
        //        {
        //            IP_ADDRESS_INVERTER = args[i].Replace("--ip-", "");
        //            Console.WriteLine("Set IP address from parameter: " + IP_ADDRESS_INVERTER);
        //        }

        //        if (args[i].StartsWith("--mqtt-"))
        //        {
        //            MQTT_CONNECTION_STRING = args[i].Replace("--mqtt-", "");
        //            Console.WriteLine("Set MQTT connection string from parameter: " + MQTT_CONNECTION_STRING);
        //        }

        //        if (args[i].StartsWith("--silent-"))
        //        {
        //            SILENT_MODE = true;
        //            Console.WriteLine("Setting silent mode - console output to a minumum (eg, docker)");
        //        }
        //    }
        //}

        private static void parseEnvironmentVariables()
        {
            var envVariables = Environment.GetEnvironmentVariables();


            Console.WriteLine("---ENV---");
            foreach (var key in envVariables.Keys)
            {
                Console.WriteLine($"{key} {envVariables[key].ToString()}");
            }
            Console.WriteLine("---ENV---");


            if (envVariables.Contains("IP"))
            {
                IP_ADDRESS_INVERTER = envVariables["IP"].ToString();
                Console.WriteLine("Set IP address from parameter: " + IP_ADDRESS_INVERTER);
            }

            if (envVariables.Contains("MQTT"))
            {
                MQTT_CONNECTION_STRING = envVariables["MQTT"].ToString();
                Console.WriteLine("Set MQTT connection string from parameter: " + MQTT_CONNECTION_STRING);
            }

            if (envVariables.Contains("SILENT"))
            {
                SILENT_MODE = true;
                Console.WriteLine("Setting silent mode - console output to a minumum (eg, docker)");
            }
        }


    }
}
