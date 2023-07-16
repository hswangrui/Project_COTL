using UnityEngine;

namespace Unify
{
	internal class PlayerPrefs
	{
		private const string SAVE_FILE = "Prefs.dict";

		public static void DeleteAll()
		{
			SaveData.Delete("Prefs.dict");
		}

		public static void DeleteKey(string key)
		{
			Debug.LogError("DeleteKey is NOT IMPLEMENETED");
		}

		public static float GetFloat(string key)
		{
			return SaveData.GetFloat("Prefs.dict", key);
		}

		public static float GetFloat(string key, float defaultValue = 0.5f)
		{
			if (HasKey(key))
			{
				return GetFloat(key);
			}
			return defaultValue;
		}

		public static int GetInt(string key)
		{
			return SaveData.GetInt("Prefs.dict", key);
		}

		public static int GetInt(string key, int defaultValue = 0)
		{
			if (HasKey(key))
			{
				return GetInt(key);
			}
			return defaultValue;
		}

		public static string GetString(string key)
		{
			return SaveData.GetString("Prefs.dict", key);
		}

		public static string GetString(string key, string defaultValue = "\"\"")
		{
			if (HasKey(key))
			{
				return GetString(key);
			}
			return defaultValue;
		}

		public static bool HasKey(string key)
		{
			return SaveData.KeyExists("Prefs.dict", key);
		}

		public static void Save()
		{
		}

		public static void SetFloat(string key, float value)
		{
			SaveData.Put("Prefs.dict", key, value);
		}

		public static void SetInt(string key, int value)
		{
			SaveData.Put("Prefs.dict", key, value);
		}

		public static void SetString(string key, string value)
		{
			SaveData.Put("Prefs.dict", key, value);
		}
	}
}
