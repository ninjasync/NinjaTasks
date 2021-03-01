using System;
using Newtonsoft.Json;
using System.IO;
using MvvmCross.Base;

namespace NinjaTasks.App.Droid.Services
{
    public class JsonNetConverterService : IMvxJsonConverter
    {
        public T DeserializeObject<T>(string inputText)
        {
            return JsonConvert.DeserializeObject<T>(inputText);
        }

        public string SerializeObject(object toSerialise)
        {
            return JsonConvert.SerializeObject(toSerialise);
        }

        public object DeserializeObject(Type type, string inputText)
        {
            return JsonConvert.DeserializeObject(inputText, type);
        }

        public T DeserializeObject<T>(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}
