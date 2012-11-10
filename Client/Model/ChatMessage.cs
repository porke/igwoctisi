namespace Client.Model
{
	using System.Runtime.Serialization;

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
