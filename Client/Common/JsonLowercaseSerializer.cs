namespace Client.Common
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public class JsonLowercaseSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        static JsonLowercaseSerializer()
        {
            Settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });            
        }

        public static string SerializeObject(object o, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(o, Formatting.None, Settings);
        }

        public static T DeserializeObject<T>(string jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }
    }
}
