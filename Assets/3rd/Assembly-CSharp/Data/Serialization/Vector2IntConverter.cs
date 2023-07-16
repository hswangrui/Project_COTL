using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Data.Serialization
{
	public class Vector2IntConverter : JsonConverter<Vector2Int>
	{
		private string kXProperty = "x";

		private string kYProperty = "y";

		public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
		{
			JObject jObject = new JObject();
			jObject.AddFirst(new JProperty(kXProperty, value.x));
			jObject.AddFirst(new JProperty(kYProperty, value.y));
			jObject.WriteTo(writer);
		}

		public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			Vector2Int zero = Vector2Int.zero;
			if (jObject[kXProperty] != null)
			{
				zero.x = jObject[kXProperty].Value<int>();
			}
			if (jObject[kYProperty] != null)
			{
				zero.y = jObject[kYProperty].Value<int>();
			}
			return zero;
		}
	}
}
