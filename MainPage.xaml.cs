using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using MessagePack;
using System.Net.Sockets;
using System.Net;
using Microsoft.Maui.Layouts;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;


namespace KSiS2
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            

        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            (sender as Button)!.BackgroundColor = Color.Parse("Blue");
        }

        private void OnButtonFocused(object sender, EventArgs e)
        {
            (sender as Button)!.BackgroundColor = Color.Parse("Orange");
            
        }
        private void AddToLog(string message)
        {
            Label label = new Label
            {
                Text = message,
                FontSize = 12,
                 
            };
            // Добавляем Label в AbsoluteLayout
            LogCont.Children.Add(label);
        }

        

        


        private static List<Message> Messages = new List<Message>();
        private Dictionary<IPEndPoint, string> Users = new Dictionary<IPEndPoint, string>();
        private Dictionary<IPEndPoint, byte[]> DataToSend = new Dictionary<IPEndPoint, byte[]>();

        //настройка сервера
        private async void OnStartClicked(object sender, EventArgs e)
        {
            try
            {
                IPAddress ip = IPAddress.Parse(ipEntry.Text);
                int port = int.Parse(portEntry.Text);
                if (Server.IsPortAvailable(port))
                {
                    try
                    {
                        StartServer(ipEntry.Text, port);
                        AddToLog("Сервер запущен! IP: " + ipEntry.Text + "Port: " + port.ToString());
                    }
                    catch
                    {
                        await DisplayAlert("Ошибка", "Ошибка запуска сервера", "ОK");
                    }
                }
                else
                    await DisplayAlert("Ошибка", "Порт недоступен", "ОK");
            }
            catch
            {
                await DisplayAlert("Ошибка", "IP-адрес или порт введен некоректно", "ОK");
            }
        }

        private async  void StartServer(string ipAddress, int port)
        {
            
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipPoint);
            socket.Listen(10);
            AddToLog("Сервер ожидает подключений...");
            while (true)
            {
                var clientSocket = await socket.AcceptAsync();
                AddToLog("К серверу подключился: " + clientSocket!.RemoteEndPoint!.ToString()!);
                try
                {
                    Thread thread = new Thread(() => 
                    {
                        Process(clientSocket);
               
                    });
                }
                catch 
                { 
                    await DisplayAlert("Ошибка!", "Ошибка при получении данных", "ОK");
                }
                /*if (messages!.Last().Username == "Server" && messages!.Last().Message == "Error");
                    await DisplayAlert("Ошибка", "Ошибка при получении данных", "ОK");
                AddToLog(messages!.Last().Username + ": " + messages!.Last().Message);
                *//*clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();*/
            }
        }

        private void Process(Socket clientSocket)
        {
            Thread receiveThread = new Thread(() => { ReceiveDataThread(clientSocket); });
            Thread sendThread = new Thread(() => { SendDataThread(clientSocket); });

        }

        private void SendDataThread(Socket clientSocket)
        {
            IPEndPoint? ipEndPoint = clientSocket.RemoteEndPoint as IPEndPoint; 
            while (true && clientSocket.Connected && ipEndPoint != null)
            {
                if (DataToSend.ContainsKey(ipEndPoint)) 
                {
                    clientSocket.Send(DataToSend[ipEndPoint]);
                }
            }
        }
        private void ReceiveDataThread(Socket clientSocket)
        {
            while (true)
            {
                int bytesRead = 0;
                byte[] buffer = new byte[2048];
                List<byte> data = new List<byte>();
                try
                {
                    do
                    {
                        bytesRead = 0;
                        bytesRead = clientSocket!.Receive(buffer);
                        foreach (byte b in buffer)
                            data.Add(b);
                    }
                    while (bytesRead > 0);
                    Messages.Add(new Message(data.ToArray()));
                    data.Clear();
                    clientSocket.Send(new Message(new IPEndPoint(0, 0), "").GetSerializedBytes());
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex.Message); 
                }
            }
        }

        private void ProcessMessage(Message message)
        {

        }

    }

}
