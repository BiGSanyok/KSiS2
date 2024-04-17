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
using System.Collections.Concurrent;


namespace KSiS2
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();


        }

        private object locker = new();

        private void OnButtonReleased(object sender, EventArgs e)
        {
            (sender as Button)!.BackgroundColor = Color.Parse("Blue");
        }

        private void OnButtonFocused(object sender, EventArgs e)
        {
            (sender as Button)!.BackgroundColor = Color.Parse("Orange");

        }
        private async Task AddToLog(string message)
        {
            await this.Dispatcher.DispatchAsync(() =>
            {
                Label label = new Label
                {
                    Text = message,
                    FontSize = 12,
                };
                lock (locker)
                {
                    LogCont.Children.Add(label);
                }
            });
        }
        

        

        


        private static List<Message> Messages = [];
        private Dictionary<IPEndPoint, string> Users = new();
        private ConcurrentDictionary<IPEndPoint, byte[]> DataToSend = new();

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
                        Thread thread = new(() => { StartServer(ipEntry.Text, port); });
                        thread.Start();

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

        private async Task StartServer(string ipAddress, int port)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipPoint);
            socket.Listen(10);
            await AddToLog("Сервер ожидает подключений...");
            while (true)
            {
                var clientSocket = await socket.AcceptAsync();
                await AddToLog("К серверу подключился: " + clientSocket!.RemoteEndPoint!.ToString()!);
                _ = Process(clientSocket); // Запускаем обработку клиента в фоновом режиме
            }
        }

        private async Task Process(Socket clientSocket)
        {
            var receiveTask = ReceiveDataThread(clientSocket);
            var sendTask = SendDataThread(clientSocket);
            await Task.WhenAny(receiveTask, sendTask); // Ждем завершения любой из задач
        }

        private async Task SendDataThread(Socket clientSocket)
        {
            IPEndPoint? ipEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
            while (true && clientSocket.Connected && ipEndPoint != null)
            {
                if (DataToSend.ContainsKey(ipEndPoint))
                {
                    var sendBuffer = DataToSend[ipEndPoint];
                    await clientSocket.SendAsync(new ArraySegment<byte>(sendBuffer), SocketFlags.None);
                }
            }
        }

        private async Task ReceiveDataThread(Socket clientSocket)
        {
            byte[] buffer = new byte[2048];
            while (true && clientSocket.Connected)
            {
                var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                await AddToLog($"{result} received data");
                /*for (int i = 0; i < result;  i++)
                {
                    var arr = buffer.Take(result).ToArray();
                    await AddToLog($"{arr[i]}");
                }*/
                if (result > 0)
                {
                    Message message = new Message(buffer.Take(result).ToArray());
                    var answer = ProcessMessage(message).Result;
                    if (answer.MessageType != MessageType.Error)
                    {
                        await clientSocket.SendAsync(new ArraySegment<byte>(answer.GetSerializedBytes()), SocketFlags.None);
                        await AddToLog("Success" + answer.MessageType.ToString());
                    }
                    else
                    {
                        await AddToLog("Error" + answer.MessageType.ToString());
                    }
                }
            }
        }



        /*private async void StartServer(string ipAddress, int port)
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
                *//*Thread receive = new Thread(() => ReceiveDataThread(clientSocket));
                Thread send = new Thread(() => SendDataThread(clientSocket));
                //Task.WaitAll(receiveTask, sendTask);
                receive.Start();
                send.Start();*//*
                Thread receive = new Thread(() => { ReceiveDataThread(clientSocket); });
                receive.Start();
                Thread send = new Thread(() => {  SendDataThread(clientSocket); }); 
                send.Start();
                //SendDataThread(clientSocket);
                
            }
        }

        private async void Process(Socket clientSocket)
        {
            Thread receive = new Thread(() => ReceiveDataThread(clientSocket));
            Thread send = new Thread(() => SendDataThread(clientSocket));
            //Task.WaitAll(receiveTask, sendTask);
            receive.Start();
            send.Start();
        }

        private async void SendDataThread(Socket clientSocket)
        {
            IPEndPoint? ipEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
            while (true && clientSocket.Connected && ipEndPoint != null)
            {
                AddToLog("send");

                Console.WriteLine("send");
                if (DataToSend.ContainsKey(ipEndPoint))
                {
                    await clientSocket.SendAsync(DataToSend[ipEndPoint], SocketFlags.None);
                }
            }
        }

        private async void ReceiveDataThread(Socket clientSocket)
        {
            while (true && clientSocket.Connected)
            {
                Console.WriteLine("receive");
                AddToLog("receive");
                int bytesRead = 0;
                byte[] buffer = new byte[2048];
                List<byte> data = new List<byte>();
                try
                {
                    do
                    {
                        bytesRead = 0;
                        bytesRead = await clientSocket!.ReceiveAsync(buffer, SocketFlags.None);
                        foreach (byte b in buffer)
                            data.Add(b);
                    }
                    while (bytesRead > 0);
                    Message message = new Message(data.ToArray());
                    data.Clear();
                    var answer = ProcessMessage(message);
                    await clientSocket.SendAsync(answer.GetSerializedBytes(), SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }*/


        private async Task<Message> ProcessMessage(Message message)
        {
            Message answer = new Message("Ошибка обработки ");
            answer.MessageType = MessageType.Error;
            switch (message.MessageType)
            {
                case MessageType.Init:
                    Users.Add(message.GetIPEndPoint()!, message.GetText());
                    answer.MessageType = MessageType.Text;
                    await AddToLog($"К серверу подключился пользователь: {message.GetText()}");
                    break;
                case MessageType.Text:
                    Messages.Add(message);
                    answer.MessageType = MessageType.Text;
                    await AddToLog($"Получено сообщение от \"{Users[message.GetIPEndPoint()!]}\": {message.GetText()}");
                    break;
                case MessageType.Photo:
                    break;
                case MessageType.File:
                    break;
                case MessageType.Command:
                    break;
            }
            return answer;
        }

    }

}
