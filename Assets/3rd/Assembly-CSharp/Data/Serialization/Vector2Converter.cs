using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Data.Serialization
{
	public class Vector2Converter : JsonConverter<Vector2>
	{
		private string kXProperty = "x";

		private string kYProperty = "y";

		public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
		{
			JObject jObject = new JObject();
			jObject.AddFirst(new JProperty(kXProperty, value.x));
			jObject.AddFirst(new JProperty(kYProperty, value.y));
			jObject.WriteTo(writer);
		}

		public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			Vector2 zero = Vector2.zero;
			if (jObject[kXProperty] != null)
			{
				zero.x = jObject[kXProperty].Value<float>();
			}
			if (jObject[kYProperty] != null)
			{
				zero.y = jObject[kYProperty].Value<float>();
			}
			return zero;
		}
	}
}
