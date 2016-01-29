using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;


namespace ComUtils
{

    public class JsonUtils
    {
        public static string Serialize(object toSerialize)
        {
            return JsonConvert.SerializeObject(toSerialize);
        }

        public static string Serialize<T>(T toSerialize)
        {
            var jsonSerializerSettings = new JsonSerializerSettings() { };


            return JsonConvert.SerializeObject(toSerialize, typeof(T), jsonSerializerSettings);
        }

        public static T Deserialize<T>(string toDeserialize)
        {
            toDeserialize = toDeserialize.Replace(@"\\", string.Empty);
            toDeserialize = toDeserialize.Replace(@"\", string.Empty);
            toDeserialize = toDeserialize.Substring(1, toDeserialize.Length - 2);

        return JsonConvert.DeserializeObject<T>(toDeserialize);
        }

        public static T DeserializeAnonymous<T>(string toDeserialize, T obj)
        {
            return JsonConvert.DeserializeAnonymousType(toDeserialize, obj);
        }


        public static T DeserializeWithValidating<T>(string json)
        {
            json = json.Replace(@"\\", string.Empty);
            json = json.Replace(@"\", string.Empty);

            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema schema = generator.Generate(typeof(T));

            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(reader);
            validatingReader.Schema = schema;

            IList<string> messages = new List<string>();
            validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);

            foreach (string message in messages)
            {
                Console.WriteLine(message);
            }

            JsonSerializer serializer = new JsonSerializer();
            T p = serializer.Deserialize<T>(validatingReader);

            return p;

        }

        private interface ISerializer
        {
            string Serialize(object toSerialize);
            T Deserialize<T>(string toDeserialize);
            T DeserializeAnonymous<T>(string toDeserialize, T obj);
        }

    }

   
    
}

   
