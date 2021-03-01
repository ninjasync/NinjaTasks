using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TaskWarriorLib.Parser
{
    public class TaskWarriorTaskParser 
    {
        private static JsonSerializerSettings _jsonSettings;
        
        public class LowercaseContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName.ToLower();
            }
        }

        public static JsonSerializerSettings JsonSettings
        {
            get
            {
                if (_jsonSettings == null)
                {
                    var settings = new JsonSerializerSettings();
                    settings.NullValueHandling = NullValueHandling.Ignore;

                    var datetimeconv = new IsoDateTimeConverter
                    {
                        Culture = CultureInfo.InvariantCulture,
                        DateTimeFormat = "yyyyMMdd'T'HHmmss'Z'"
                    };
                    settings.Converters.Add(datetimeconv);
                    settings.Converters.Add(new StringEnumConverter());
                    settings.ContractResolver = new LowercaseContractResolver();

                    _jsonSettings = settings;
                }
                return _jsonSettings;
            }
        }

        public TaskWarriorTask Parse(string repr)
        {
            var ret = JsonConvert.DeserializeObject<TaskWarriorTask>(repr, JsonSettings);
            ret.JsonOriginalLine = repr;
            return ret;
        }

        public string ToRepr(TaskWarriorTask task)
        {
            string repr = JsonConvert.SerializeObject(task, JsonSettings);
            return repr;
        }
    }
}
