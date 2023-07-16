using Newtonsoft.Json.Linq;

namespace src.Extensions
{
	public static class JObjectExtensions
	{
		public static JObject GetJObject(this JObject jObject, string key)
		{
			if (jObject[key] != null)
			{
				return jObject[key].Value<JObject>();
			}
			return null;
		}

		public static bool TryGetAssignable<T>(this JObject jObject, string key, ref T assignable)
		{
			if (jObject[key] != null)
			{
				assignable = jObject[key].Value<T>();
				return true;
			}
			return false;
		}

		public static void AddAssignable<T>(this JObject jObject, string key, ref T assignable)
		{
			jObject.AddFirst(new JProperty(key, assignable));
		}
	}
}
