using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json;
using System.Threading;


namespace KSiS2
{
    public struct MessageInfo
    {
            public int Number { get; set; }
            public string Username { get; set; }
            public string Message { get; set; }
    }

    class Server
    {
        public IPAddress IPAddress { get; }
        public int Port { get; }
        public static object locker = new();


        public static MessageInfo GetMessageInfo(byte[] receivedMessage)
        {
            string receivedJson = Encoding.UTF8.GetString(receivedMessage);
            MessageInfo receivedData = JsonConvert.DeserializeObject<MessageInfo>(receivedJson);
            return receivedData;
        }

        public static List<MessageInfo> GetMessagesInfo(byte[] receivedMessage)
        {
            string receivedJson = Encoding.UTF8.GetString(receivedMessage);
            var receivedData = JsonConvert.DeserializeObject<List<MessageInfo>>(receivedJson);
            return receivedData;
        }

        public static byte[] GetMessageBytes(MessageInfo message)
        {
            string json = JsonConvert.SerializeObject(message);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            return buffer;
        }
        public static byte[] GetMessagesBytes(List<MessageInfo> messages)
        {
            string json = JsonConvert.SerializeObject(messages);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            return buffer;
        }


        //public static 

        public static MessageInfo CommunicateWithClient(Socket clientSocket, List<MessageInfo> messages)
        {
            while (clientSocket.Connected)
            {
                int bytesRead = 0;
                byte[] inputData = new byte[1024];
                List<byte> input = new List<byte>();
                try
                {
                    do
                    {
                        bytesRead = 0;
                        bytesRead = clientSocket!.Receive(inputData);
                        foreach (var i in inputData)
                        {
                            input.Add(i);
                        }

                    }
                    while (bytesRead > 0);
                    var inputMessage = GetMessageInfo(input.ToArray());
                }
                catch
                {
                    return new MessageInfo { Username = "Server", Message = "Empty" };
                }
                byte[] outputData = GetMessagesBytes(messages);
                clientSocket.Send(outputData);
            }
            return new MessageInfo() { Message = "", Number = 10, Username = "" };

        }

        public static bool IsPortAvailable(int myPort)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var activeTcpConnections = properties.GetActiveTcpConnections();
            var activeTcpListeners = properties.GetActiveTcpListeners();
            var activeUdpListeners = properties.GetActiveUdpListeners();

            var allPorts = new List<int>();
            allPorts.AddRange(activeTcpConnections.Select(c => c.LocalEndPoint.Port));
            allPorts.AddRange(activeTcpListeners.Select(l => l.Port));
            allPorts.AddRange(activeUdpListeners.Select(l => l.Port));

            return !allPorts.Contains(myPort);
        }
    }
}
