
using System.Text.Json;

namespace Methane.Toolkit
{
    public static class Core
    {
        public static JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        public static string ToJSON<T>(T Obj)
        {
            return JsonSerializer.Serialize(Obj, JsonOptions);
        }
        public static T FromJSON<T>(string JSON)
        {
            return JsonSerializer.Deserialize<T>(JSON, JsonOptions);
        }
    }
}