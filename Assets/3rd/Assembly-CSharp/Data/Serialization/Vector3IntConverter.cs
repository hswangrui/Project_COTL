using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Data.Serialization
{
	public class Vector3IntConverter : JsonConverter<Vector3Int>
	{
		private string kXProperty = "x";

		private string kYProperty = "y";

		private string kZProperty = "z";

		public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
		{
			JObject jObject = new JObject();
			jObject.AddFirst(new JProperty(kXProperty, value.x));
			jObject.AddFirst(new JProperty(kYProperty, value.y));
			jObject.WriteTo(writer);
		}

		public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			Vector3Int zero = Vector3Int.zero;
			if (jObject[kXProperty] != null)
			{
				zero.x = jObject[kXProperty].Value<int>();
			}
			if (jObject[kYProperty] != null)
			{
				zero.y = jObject[kYProperty].Value<int>();
			}
			if (jObject[kZProperty] != null)
			{
				zero.z = jObject[kZProperty].Value<int>();
			}
			return zero;
		}
	}
}
