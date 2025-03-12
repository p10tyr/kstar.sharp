using kstar.sharp.domain.Extensions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace kstar.sharp.datacollect
{
    public delegate void DataRecievedEventHandler(kstar.sharp.domain.Models.InverterData obj);


    public class Client
    {
        //public event 

        public event DataRecievedEventHandler DataRecieved;

        private int PORT_UDP_BROADCAST = 48899;
        private UdpClient receiver_Broadcast;
        private IPEndPoint inverterIpEndPoint_Broadcast;
        private int PORT_UDP_DATA = 8899;
        private UdpClient receiver_Data;
        private IPEndPoint inverterIpEndPoint_Data;

        public bool BroadcastRequest_ResponseRecieved { get; set; }

        private string IP_ADDRESS_INVERTER;
        public string IPAddressInverter { get { return IP_ADDRESS_INVERTER; } }

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnDataRecieved(kstar.sharp.domain.Models.InverterData obj)
        {
            DataRecieved?.Invoke(obj);
        }

        /// <summary>
        /// Sets up binding for Broadcast and Data but does not Start sending any data until method explicitly invoked
        /// </summary>
        /// <param name="IPAddress">If you require auto detect using broadcast or cannot recieve broadcasts pass in empty string or "0.0.0.0" - Broadcast needs to be sent any way but wont be parsed on reply</param>
        public Client(string IPAddress)
        {
            if (!string.IsNullOrWhiteSpace(IPAddress) || !IPAddress.Equals("0.0.0.0"))
            {
                IP_ADDRESS_INVERTER = IPAddress;
            }

            receiver_Data = new UdpClient(PORT_UDP_DATA);
            receiver_Data.BeginReceive(DataReceived, receiver_Data);

            receiver_Broadcast = new UdpClient(PORT_UDP_BROADCAST);
            receiver_Broadcast.BeginReceive(DataReceived, receiver_Broadcast);

            BroadcastRequest_ResponseRecieved = false;
        }


        /// <summary>
        /// Binds a callback to DataRecieve method to the Data Reciever
        /// </summary>
        public void SendDataRequest()
        {
            IPAddress inverterIP_Communication = IPAddress.Parse(IP_ADDRESS_INVERTER);
            inverterIpEndPoint_Data = new IPEndPoint(inverterIP_Communication, 8899);

            byte[] byteCmd = "0xaa55b07f0106000235".ToByteArray();
            receiver_Data.Send(byteCmd, byteCmd.Length, inverterIpEndPoint_Data);

            //////I think this command recieces serial number and model infomration. Its always the same
            //System.Threading.Thread.Sleep(5000);
            //byteCmd = ConvertToByteArray("0xaa55b07f0102000231");
            //receiver_Communication.Send(byteCmd, byteCmd.Length, inverterIpEndPoint_Communication);
        }


        /// <summary>
        /// This sends a UDP broadcast to the whole network and waits for a reply from the inverter. 
        /// This helps identify the Inverters IP address. It may be required to let the Inverter know to send Unicast packets to the requested IP too.
        /// If you provided an IPAddress in the constructor the response will be ignored here. Broadcast is request is required never the less.
        /// </summary>
        public void SendBroadcastRequest()
        {
            IPAddress inverterIP_Broadcast = IPAddress.Parse("255.255.255.255");
            inverterIpEndPoint_Broadcast = new IPEndPoint(inverterIP_Broadcast, PORT_UDP_BROADCAST);

            string command = "WIFIKIT-214028-READ";
            var sent = receiver_Broadcast.Send(Encoding.ASCII.GetBytes(command), command.Length, inverterIpEndPoint_Broadcast);

            Console.WriteLine($"Broadcast sent {sent} bytes");

            if (!string.IsNullOrWhiteSpace(IP_ADDRESS_INVERTER) && !IP_ADDRESS_INVERTER.Equals("0.0.0.0"))
            {
                BroadcastRequest_ResponseRecieved = true;
                receiver_Broadcast.Close(); //Wont need this any more
            }
        }

        /// <summary>
        /// Generic method to read data from any UdpClient Async bound singleton
        /// </summary>
        /// <param name="ar"></param>
        private void DataReceived(IAsyncResult ar)
        {
            //Console.WriteLine("DATA RECIEVED");
            try
            {
                UdpClient c = (UdpClient)ar.AsyncState;
                IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);

                string hex = BitConverter.ToString(receivedBytes);

                // Restart listening for udp data packages
                c.BeginReceive(DataReceived, ar.AsyncState);

                Parse(hex);
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing recieved data");
            }
        }


        /// <summary>
        /// This requires hex string that is deliminated. Default is char '-' (dash)
        /// </summary>
        /// <param name="HEXConverted"></param>
        public void Parse(string HEXConverted, char deliminator = '-')
        {
            string[] hexArray = HEXConverted.Split(deliminator);

            switch (hexArray.Length)
            {
                case 19:
                    //    //parseMonitoringData(hexArray); //echo from unicast no valubale data
                    //    //Console.WriteLine("Recieved Unicast Echo");
                    break;

                case 37:
                    //Console.WriteLine("Recieved Unicast Response");
                    parseBroadcastData(hexArray);//reply from unicast request
                    break;

                case 92:
                    parseInverterData(hexArray);
                    break;

                default:
                    //Console.WriteLine("Could not determine how to parse packet based on size: " + hexArray.Length + " msg: " + String.Join("", hexArray).HexToASCII() );
                    break;
            }

        }


        public void parseBroadcastData(string[] hexArray)
        {
            string asciiData = string.Join("", hexArray).HexToASCII();
            string _ip = asciiData.Split(',')[0];
            string _serial = asciiData.Split(',')[1];
            string _wifiAccess = asciiData.Split(',')[2];

            //Set the Inverters IP address
            if (IP_ADDRESS_INVERTER.Equals("0.0.0.0"))
                IP_ADDRESS_INVERTER = _ip;
            //Console.WriteLine("Setting IP to " + _ip);
            //Set flag to signal start query on data port and any thing else required
            BroadcastRequest_ResponseRecieved = true;
            receiver_Broadcast.Close(); //Wont need this any more
        }

        public void parseInverterData(string[] hexArray)
        {
            try
            {
                //kstar.sharp.Services.DBService db = new sharp.Services.DBService();
                var invDat = new kstar.sharp.domain.Models.InverterData(hexArray); // db.Save(hexArray);
                //raise event and pass the model back
                OnDataRecieved(invDat);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Monitor Data Parse Failed: " + ex.ToString());
            }


        }


    }
}
