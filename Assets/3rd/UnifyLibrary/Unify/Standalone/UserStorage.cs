using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Unify.Standalone
{
	public class UserStorage : Unify.UserStorage
	{
		public static string basePath;

		private static string busy;

		private int userId;

		public UserStorage(int userId, string containerName, UserStorageCallbackDelegate callback)
		{
			UserStorage storage = this;
			if (basePath == null)
			{
				basePath = Application.persistentDataPath;
			}
			this.userId = userId;
			new Timer(delegate
			{
				callback(storage);
			}, "", 100, -1);
		}

		private bool HandleBusyError(string key, ResultCallbackDelegate failCallback)
		{
			if (busy != null)
			{
				Logger.Log("USERSTORAGE:STANDALONE: Error overlapping ops: " + busy + " - " + key);
				busy = null;
				failCallback(key, null, "error:toomanyops");
				return true;
			}
			return false;
		}

		public override void writeKeyValue(string key, byte[] data, ResultCallbackDelegate successCallback = null, ResultCallbackDelegate failCallback = null)
		{
			string text = basePath + "/user-" + userId + "/" + key;
			if (HandleBusyError("write " + key, failCallback))
			{
				return;
			}
			if ((data == null || data.Length == 0) && failCallback != null)
			{
				failCallback(key, null, "error:nodata");
				return;
			}
			if (UnifyManager.debugWriteFail)
			{
				if (failCallback != null)
				{
					new Timer(delegate
					{
						failCallback(key, null, "error:writefailed");
					}, "", 100, -1);
				}
				return;
			}
			if (data.Length > 16777216)
			{
				Logger.Log("GDK:USERSTORAGE: Error save data exceeds maximum size.");
				new Timer(delegate
				{
					failCallback(key, null, "error:datasize");
				}, "", 100, -1);
				return;
			}
			busy = "write " + key;
			try
			{
				int num = text.LastIndexOf("/");
				if (num >= 0)
				{
					string path = text.Substring(0, num);
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
				}
				File.WriteAllBytes(text, data);
			}
			catch (Exception)
			{
				if (failCallback != null)
				{
					new Timer(delegate
					{
						busy = null;
						failCallback(key, null, "error:writefailed");
					}, "", 100, -1);
				}
				return;
			}
			if (successCallback != null)
			{
				new Timer(delegate
				{
					busy = null;
					successCallback(key, data, null);
				}, "", 100, -1);
			}
		}

		public override void deleteKey(string key, ResultCallbackDelegate successCallback = null, ResultCallbackDelegate failCallback = null)
		{
			string path = basePath + "/user-" + userId + "/" + key;
			try
			{
				File.Delete(path);
			}
			catch (Exception)
			{
				failCallback?.Invoke(key, null, "error:deletefailed");
			}
			successCallback?.Invoke(key, null, null);
		}

		public override void readKey(string key, ResultCallbackDelegate successCallback = null, ResultCallbackDelegate failCallback = null)
		{
			string path = basePath + "/user-" + userId + "/" + key;
			byte[] data = null;
			if (UnifyManager.debugReadFail)
			{
				if (failCallback != null)
				{
					failCallback(key, null, "error:readfailed");
				}
			}
			else if (File.Exists(path))
			{
				try
				{
					data = File.ReadAllBytes(path);
				}
				catch (Exception ex)
				{
					Logger.Log("USERSTORAGE: Unable to load UserData: " + key + ", " + ex.Message);
				}
				if (data != null && successCallback != null)
				{
					new Timer(delegate
					{
						successCallback(key, data, null);
					}, "", 100, -1);
				}
				else if (failCallback != null)
				{
					new Timer(delegate
					{
						failCallback(key, null, "error:readfailed");
					}, "", 100, -1);
				}
			}
			else
			{
				failCallback(key, null, "error:notfound");
			}
		}
	}
}
