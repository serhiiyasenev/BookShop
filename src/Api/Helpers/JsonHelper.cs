using System.IO;
using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace Api.Helpers
{
    public static class JsonHelper
    {
        public static T FromJsonToObject<T>(object content)
        {
            var model = JsonConvert.DeserializeObject<T>(content.ToString());
            return model;
        }

        public static string FromObjectToJson(object content)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            var serializedObject = JsonConvert.SerializeObject(content, Formatting.None, jsonSerializerSettings);
            return serializedObject;
        }

        public static StringContent ToStringContent(object content)
        {
            return new StringContent(FromObjectToJson(content), Encoding.UTF8, "application/json");
        }

        public static T LoadFromFile<T>(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();
                T result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
        }

        public static T Deserialize<T>(object jsonObject)
        {
            return JsonConvert.DeserializeObject<T>(Convert.ToString(jsonObject));
        }

        public static async Task<T> GetModelAsync<T>(this HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}