using Newtonsoft.Json;

namespace Data.Serialization
{
	public class MMSerialization
	{
		public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			Formatting = Formatting.None,
			Converters = 
			{
				(JsonConverter)new Vector2Converter(),
				(JsonConverter)new Vector3Converter(),
				(JsonConverter)new Vector2IntConverter(),
				(JsonConverter)new Vector3IntConverter()
			}
		};

		public static JsonSerializer JsonSerializer = JsonSerializer.Create(JsonSerializerSettings);
	}
}
