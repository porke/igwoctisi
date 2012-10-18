using System.Runtime.Serialization;
using System;
namespace Client.Model
{
    [DataContract]
    public class ChatMessage
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Time { get; set; }
    }
}
