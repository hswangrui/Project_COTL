using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unify.Utils;
using UnityEngine;

namespace Unify
{
	public class SaveData : MonoBehaviour
	{
		private static Dictionary<string, Dictionary<string, string>> saveData = new Dictionary<string, Dictionary<string, string>>();

		private static Dictionary<string, byte[]> saveStorage = new Dictionary<string, byte[]>();

		private static List<string> toSave = new List<string>();

		private static List<string> toDelete = new List<string>();

		public static bool HasKeyToSave => toSave.Count > 0;

		public static bool HasKeyToDelete => toDelete.Count > 0;

		public static string PopKeyToSave()
		{
			if (toSave.Count <= 0)
			{
				return null;
			}
			string text = toSave[0];
			toSave.Remove(text);
			return text;
		}

		public static void FlushKey(string key)
		{
			if (toSave.Contains(key))
			{
				toSave.Remove(key);
			}
			toSave.Insert(0, key);
		}

		public static string PopKeyToDelete()
		{
			if (toDelete.Count <= 0)
			{
				return null;
			}
			string text = toDelete[0];
			toDelete.Remove(text);
			return text;
		}

		public static void AddUnique(List<string> list, string key)
		{
			if (!list.Contains(key))
			{
				list.Add(key);
			}
		}

		public void Start()
		{
			Logger.Log("UNIFY:SAVEDATA: Start");
		}

		public static void Clear()
		{
			saveData.Clear();
			saveStorage.Clear();
			toSave.Clear();
			toDelete.Clear();
		}

		public void Update()
		{
		}

		public static void PutObject(string file, object data, BinaryFormatter bf = null)
		{
			if (bf == null)
			{
				bf = Serialization.CreateBinaryFormatter();
			}
			MemoryStream memoryStream = new MemoryStream();
			bf.Serialize(memoryStream, data);
			string data2 = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
			memoryStream.Close();
			Put(file, data2);
		}

		internal static void _Store(string file, string data)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (file.IndexOf(".dict") > 0)
			{
				DecodeSaveData(dictionary, data);
				saveData[file] = dictionary;
			}
			else
			{
				saveStorage[file] = Encoding.UTF8.GetBytes(data);
			}
		}

		internal static void _Store(string file, byte[] data, int length = -1)
		{
			if (file.IndexOf(".dict") > 0)
			{
				string @string = Encoding.UTF8.GetString(data);
				_Store(file, @string);
				return;
			}
			length = ((length < 0) ? data.Length : length);
			byte[] array = new byte[length];
			Array.Copy(data, array, length);
			saveStorage[file] = array;
		}

		public static void Put(string file, string data)
		{
			Logger.Log("UNIFY:SAVEDATA: Put: " + file + ", " + data.Length + " chars");
			_Store(file, data);
			AddUnique(toSave, file);
			toDelete.Remove(file);
		}

		public static void PutBytes(string file, byte[] data, int length = -1)
		{
			int num = ((length < 0) ? data.Length : length);
			Logger.Log($"UNIFY:SAVEDATA: Put: {file}, {length} ({num}) bytes");
			_Store(file, data, num);
			AddUnique(toSave, file);
			toDelete.Remove(file);
		}

		public static MemoryStream OpenStreamToFile(string file, int minCapacityMiB)
		{
			MemoryStream memoryStream = new NotClosingMemoryStream(new MemoryStream(minCapacityMiB * 1024 * 1024));
			saveStorage[file] = memoryStream.GetBuffer();
			return memoryStream;
		}

		public static void FlushStream(string file)
		{
			AddUnique(toSave, file);
			toDelete.Remove(file);
		}

		public static void Delete(string file, bool force = false)
		{
			if (force || saveData.ContainsKey(file) || saveStorage.ContainsKey(file))
			{
				if (saveData.ContainsKey(file))
				{
					saveData.Remove(file);
				}
				if (saveStorage.ContainsKey(file))
				{
					saveStorage.Remove(file);
				}
				if (!toDelete.Contains(file))
				{
					AddUnique(toDelete, file);
				}
				toSave.Remove(file);
			}
		}

		public static string Get(string file)
		{
			if (file.IndexOf(".dict") > 0)
			{
				return EncodeSaveData(saveData[file]);
			}
			return Encoding.UTF8.GetString(saveStorage[file]);
		}

		public static byte[] GetBytes(string file)
		{
			if (file.IndexOf(".dict") > 0)
			{
				string s = EncodeSaveData(saveData[file]);
				return Encoding.UTF8.GetBytes(s);
			}
			return saveStorage[file];
		}

		public static object GetObject(string file, BinaryFormatter bf = null)
		{
			string text = null;
			object obj = null;
			text = ((file.IndexOf(".dict") <= 0) ? Encoding.UTF8.GetString(saveStorage[file]) : EncodeSaveData(saveData[file]));
			try
			{
				MemoryStream memoryStream = new MemoryStream();
				byte[] array = Convert.FromBase64String(text);
				memoryStream.Write(array, 0, array.Length);
				memoryStream.Position = 0L;
				if (bf == null)
				{
					bf = Serialization.CreateBinaryFormatter();
				}
				obj = bf.Deserialize(memoryStream);
				memoryStream.Close();
				return obj;
			}
			catch (Exception)
			{
				Debug.LogError("Unify:SaveData: exception during deserialize.");
				return null;
			}
		}

		public static void Put<T>(string file, string key, T value)
		{
			string value2 = value.ToString();
			if (!saveData.ContainsKey(file))
			{
				Logger.Log("UNIFY:SAVEDATA: Creating new file: " + file);
				saveData[file] = new Dictionary<string, string>();
			}
			saveData[file][key] = value2;
			AddUnique(toSave, file);
			toDelete.Remove(file);
		}

		public static void Retry(string file)
		{
			Logger.Log("UNIFY:SAVEDATA: Retry: " + file);
			if (toSave.Contains(file))
			{
				toSave.Remove(file);
			}
			toSave.Insert(0, file);
			toDelete.Remove(file);
		}

		public static bool IsDirty(string file)
		{
			return toSave.Contains(file);
		}

		public static int GetInt(string file, string key)
		{
			int result = 0;
			if (saveData.ContainsKey(file) && saveData[file].TryGetValue(key, out var value))
			{
				int.TryParse(value, out result);
			}
			return result;
		}

		public static float GetFloat(string file, string key)
		{
			float result = 0f;
			if (saveData.ContainsKey(file) && saveData[file].TryGetValue(key, out var value))
			{
				float.TryParse(value, out result);
			}
			return result;
		}

		public static bool GetBool(string file, string key)
		{
			bool result = false;
			if (saveData.ContainsKey(file) && saveData[file].TryGetValue(key, out var value))
			{
				bool.TryParse(value, out result);
			}
			return result;
		}

		public static string GetString(string file, string key)
		{
			string result = null;
			if (saveData.ContainsKey(file) && saveData[file].TryGetValue(key, out var value))
			{
				result = value;
			}
			return result;
		}

		public static bool KeyExists(string file, string key)
		{
			if (saveData.ContainsKey(file))
			{
				return saveData[file].ContainsKey(key);
			}
			return false;
		}

		public static Dictionary<string, string> GetAll(string file)
		{
			if (saveData.ContainsKey(file))
			{
				return new Dictionary<string, string>(saveData[file]);
			}
			return null;
		}

		public static bool Exists(string file)
		{
			if (!saveData.ContainsKey(file))
			{
				return saveStorage.ContainsKey(file);
			}
			return true;
		}

		private static string EncodeSaveData(Dictionary<string, string> dict)
		{
			string text = "";
			foreach (KeyValuePair<string, string> item in dict)
			{
				text = text + item.Key + "=" + item.Value + ";";
			}
			return text;
		}

		private static void DecodeSaveData(Dictionary<string, string> dict, string data)
		{
			string[] array = data.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				if (array2.Length == 2)
				{
					dict[array2[0]] = array2[1];
				}
			}
		}
	}
}
