using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;

namespace KSiS2
{
    public enum MessageType
    {
        Init,
        Text,
        Photo,
        File,
        Command,
        Error
    }
    public class Message
    {
        private IPEndPoint? IPEndPoint { get; set; }
        public byte[] Data { get; set; }
        public MessageType MessageType { get; set; }

        public string IP = "";

        public Message(byte[] serializedData)
        {
            try
            {
                if (serializedData == null)
                    throw new ArgumentNullException(nameof(serializedData));
                string buffer = Encoding.UTF8.GetString(serializedData!);
                Message? message = JsonConvert.DeserializeObject<Message>(buffer);
                if (message != null)
                {
                    MessageType = message.MessageType;
                    Data = message.GetData();
                    IPEndPoint = IPEndPoint.Parse(message.IP);
                    IP = message.IP;
                }
                else
                {
                    throw new Exception("message null");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Message construct error " + ex.Message);
            }
        }
        public Message(IPEndPoint iPEndPoint, byte[] data)
        {
            Data = data;
            SetIPEndPoint(iPEndPoint);
        }
        public Message() { }

        public Message(string message) => Data = Encoding.UTF8.GetBytes(message);
        public Message(IPEndPoint iPEndPoint, string message) : this(message) => SetIPEndPoint(iPEndPoint);

        public byte[] GetData() => Data;

        public void SetText(string text) => Data = Encoding.UTF8.GetBytes(text);
        public string GetText()
        {
            if (Data != null)
                return Encoding.UTF8.GetString(Data);
            else
                return string.Empty;
        }

        public void SetIPEndPoint(IPEndPoint iPEndPoint)
        {
            IPEndPoint = iPEndPoint;
            IP = IPEndPoint.ToString();
        }
        public IPEndPoint? GetIPEndPoint() => IPEndPoint;

        public byte[] GetSerializedBytes() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        public void SetMessageBytes(byte[] data) => Data = data;
        public byte[] GetMessageBytes() => Data;

    }
}