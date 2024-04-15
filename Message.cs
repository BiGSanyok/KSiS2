using Obvs.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using GoogleGson;

namespace KSiS2
{
    public enum MessageType
    {
        Init,
        Text, 
        Photo,
        File,
        Command
    }
    public class Message
    {
        private IPEndPoint IPEndPoint { get; set; }
        private byte[] Data { get; set; }
        public MessageType MessageType { get; set; }

        public Message(byte[] serializedData)
        {
            string buffer = Encoding.UTF8.GetString(serializedData);
            Message? message = JsonConvert.DeserializeObject<Message>(buffer);
            if (message != null)
            {
                this.MessageType = message.MessageType;
                this.Data = message.Data;
                this.IPEndPoint = message.IPEndPoint;
            }
            else
            {
                throw new Exception("Invalid message");
            }
        }
        public Message(IPEndPoint iPEndPoint, byte[] data)
        {
            Data = data;
            IPEndPoint = iPEndPoint;
        }
        public Message(IPEndPoint iPEndPoint, string message)
        {
            IPEndPoint = iPEndPoint;
            Data = Encoding.UTF8.GetBytes(message);
        }

        public void SetIPEndPoint(IPEndPoint iPEndPoint) => IPEndPoint = iPEndPoint;

        public byte[] GetSerializedBytes() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        public void SetMessageBytes(byte[] data) => Data = data;
        public byte[] GetMessageBytes() => Data;
        
    }
}
