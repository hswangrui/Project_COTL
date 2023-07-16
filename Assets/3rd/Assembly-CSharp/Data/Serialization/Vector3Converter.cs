using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Data.Serialization
{
	public class Vector3Converter : JsonConverter<Vector3>
	{
		private string kXProperty = "x";

		private string kYProperty = "y";

		private string kZProperty = "z";

		public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
		{
			JObject jObject = new JObject();
			jObject.AddFirst(new JProperty(kXProperty, value.x));
			jObject.AddFirst(new JProperty(kYProperty, value.y));
			jObject.AddFirst(new JProperty(kZProperty, value.z));
			jObject.WriteTo(writer);
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			Vector3 zero = Vector3.zero;
			if (jObject[kXProperty] != null)
			{
				zero.x = jObject[kXProperty].Value<float>();
			}
			if (jObject[kYProperty] != null)
			{
				zero.y = jObject[kYProperty].Value<float>();
			}
			if (jObject[kZProperty] != null)
			{
				zero.z = jObject[kZProperty].Value<float>();
			}
			return zero;
		}
	}
}
