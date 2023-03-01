using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace kstar.sharp.console
{
    //UDP Multicast must be supported if running in VM or Continers. Needs full access to network.
    //example kstar.console.exe --ip-192.168.1.50 --sqlite-"Data Source=c:\sqlite\inverter-data.db"

    internal class Program
    {
        private static int REFRESH_SECONDS = 5;
        private static int REFRESH_DATABASE_SAVES_SECONDS = 30; //30; //30 seconds is 2880 rows per day, and ~1mln rows per year. SQLite should handle a few billion no problem
        private static string IP_ADDRESS_INVERTER = "0.0.0.0";
        private static kstar.sharp.datacollect.Client client;

        private static string SQL_LITE_CONNECTION_STRING = "";
        private static string MQTT_CONNECTION_STRING = "";
        private static bool SILENT_MODE = false;


        private static IMqttClient mqttClient;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("------------------------");

            parseArguments(args);

            if (string.IsNullOrWhiteSpace(IP_ADDRESS_INVERTER))
            {
                Console.WriteLine("You must pass in the Inverters IP address or 0.0.0.0 for broadcast request");
                Console.ReadLine();
                return;
            }

            //Initialise the client
            client = new sharp.datacollect.Client(IP_ADDRESS_INVERTER);

            //Test database access if SQL String connection added or skip if not
            if (!string.IsNullOrWhiteSpace(SQL_LITE_CONNECTION_STRING))
            {
                using (var dbContext = new sharp.ef.InverterDataContext(SQL_LITE_CONNECTION_STRING))
                {
                    kstar.sharp.Services.DbService db = new sharp.Services.DbService(dbContext);
                    await db.Get(DateTime.Now, DateTime.Now);

                    Console.WriteLine("");
                    Console.WriteLine("SQLite is accessible");
                }
            }


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

            nextDbSaveTime = DateTime.Now.AddSeconds(30);

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
            var factory = new MqttFactory();

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

                var messages = new List<MqttApplicationMessage>();
                messages.Add(CreateMqttMessage("sensor/inverter/pvpower", inverterDataModel.PVData.PVPower.ToString()));
                messages.Add(CreateMqttMessage("sensor/inverter/grid", inverterDataModel.GridData.GridPower.ToString()));
                messages.Add(CreateMqttMessage("sensor/inverter/grid/import", grid_import.ToString()));
                messages.Add(CreateMqttMessage("sensor/inverter/grid/export", grid_export.ToString()));
                messages.Add(CreateMqttMessage("sensor/inverter/load", inverterDataModel.LoadData.LoadPower.ToString()));
                messages.Add(CreateMqttMessage("sensor/inverter/temp", inverterDataModel.StatData.InverterTemperature.ToString()));
                messages.Add(CreateMqttMessage("sensor/inverter/etoday", inverterDataModel.StatData.EnergyToday.ToString()));

                await mqttClient.PublishAsync(messages);
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
            .WithAtMostOnceQoS()
            .Build();
        }

        private static DateTime nextDbSaveTime;

        private static void DataRecievedUpdateConsole(domain.Models.InverterData inverterDataModel)
        {
            if (SILENT_MODE)
            {
                //heartbeat?
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

            if (DateTime.Now >= nextDbSaveTime)
            {
                nextDbSaveTime = DateTime.Now.AddSeconds(REFRESH_DATABASE_SAVES_SECONDS);
                Task.Run(() => SaveToDb(inverterDataModel));
            }

        }

        public static void SaveToDb(domain.Models.InverterData inverterDataModel)
        {
            if (string.IsNullOrWhiteSpace(SQL_LITE_CONNECTION_STRING))
                return;

            if (!SILENT_MODE)
                Console.WriteLine("Saving DB Entry");

            try
            {
                using (var dbContext = new ef.InverterDataContext(SQL_LITE_CONNECTION_STRING))
                {
                    Services.DbService db = new Services.DbService(dbContext);

                    db.Save(inverterDataModel);
                }
            }
            catch (Exception x)
            {
                Console.WriteLine($"ERROR - Saving to DB - FAILED - ${x.Message}");
            }

        }

        private static void parseArguments(string[] args)
        {

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--ip-"))
                {
                    IP_ADDRESS_INVERTER = args[i].Replace("--ip-", "");
                    Console.WriteLine("Set IP address from parameter: " + IP_ADDRESS_INVERTER);
                }

                if (args[i].StartsWith("--sqlite-"))
                {
                    SQL_LITE_CONNECTION_STRING = args[i].Replace("--sqlite-", "");
                    Console.WriteLine("Set SQLite connection string from parameter: " + SQL_LITE_CONNECTION_STRING);
                }

                if (args[i].StartsWith("--mqtt-"))
                {
                    MQTT_CONNECTION_STRING = args[i].Replace("--mqtt-", "");
                    Console.WriteLine("Set MQTT connection string from parameter: " + MQTT_CONNECTION_STRING);
                }

                if (args[i].StartsWith("--silent-"))
                {
                    SILENT_MODE = true;
                    Console.WriteLine("Setting silent mode - console output to a minumum (eg, docker)");
                }


            }
        }
    }
}
