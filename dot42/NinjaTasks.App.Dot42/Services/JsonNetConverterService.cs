using System;
using Cirrious.CrossCore.Platform;
using Newtonsoft.Json;

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
    }
}
