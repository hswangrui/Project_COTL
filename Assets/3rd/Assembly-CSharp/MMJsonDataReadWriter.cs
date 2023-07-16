using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Data.Serialization;
using FilepathUtils;
using Newtonsoft.Json;
using UnityEngine;

public class MMJsonDataReadWriter<T> : MMDataReadWriterBase<T>
{
	

	private const string kSaveDirectory = "saves";

	private const string kBackupDirectory = "backup";

	private const int kBackupLimit = 10;

	public override void Write(T data, string filename, bool encrypt = true, bool backup = true)
	{
		bool flag = true;
		string filepath = GetFilepath(filename);
		Debug.Log(("MMJsonDataReadWriter - Write File " + filepath).Colour(Color.yellow));
		if (backup)
		{
			MakeBackup(filename);
		}
		FileStream fileStream = null;
		RNGCryptoServiceProvider rNGCryptoServiceProvider = null;
		Aes aes = null;
		CryptoStream cryptoStream = null;
		StreamWriter streamWriter = null;
		try
		{
			if (encrypt)
			{
				using (fileStream = new FileStream(filepath, FileMode.Create))
				{
					fileStream.WriteByte(69);
					byte[] array = new byte[16];
					using (rNGCryptoServiceProvider = new RNGCryptoServiceProvider())
					{
						rNGCryptoServiceProvider.GetBytes(array);
					}
					fileStream.Write(array, 0, array.Length);
					using (aes = Aes.Create())
					{
						aes.Key = array;
						byte[] iV = aes.IV;
						fileStream.Write(iV, 0, iV.Length);
						using (cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
						{
							using (streamWriter = new StreamWriter(cryptoStream))
							{
								MMSerialization.JsonSerializer.Serialize(streamWriter, data);
								return;
							}
						}
					}
				}
			}
			using (streamWriter = File.CreateText(filepath))
			{
				MMSerialization.JsonSerializer.Serialize(streamWriter, data);
			}
		}
		catch (Exception ex)
		{
			Action<MMReadWriteError> onWriteError = OnWriteError;
			if (onWriteError != null)
			{
				onWriteError(new MMReadWriteError(ex.Message));
			}
			Debug.Log(ex.Message.Colour(Color.red));
			Debug.LogException(ex);
			flag = false;
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Dispose();
			}
			if (rNGCryptoServiceProvider != null)
			{
				rNGCryptoServiceProvider.Dispose();
			}
			if (aes != null)
			{
				aes.Dispose();
			}
			if (cryptoStream != null)
			{
				cryptoStream.Dispose();
			}
			if (streamWriter != null)
			{
				streamWriter.Dispose();
			}
			if (!flag)
			{
				string backupFilepath = GetBackupFilepath(GetMostRecentBackup(filename));
				if (File.Exists(backupFilepath) && backup)
				{
					File.Copy(backupFilepath, filepath, true);
				}
			}
			else
			{
				if (backup)
				{
					MakeBackup(filename);
				}
				Action onWriteCompleted = OnWriteCompleted;
				if (onWriteCompleted != null)
				{
					onWriteCompleted();
				}
			}
		}
	}


	private void ScrubBackups(string filename)
	{
		filename = filename.NormalizePath().Split(Path.DirectorySeparatorChar).LastElement();
		List<string> backups = GetBackups(filename);
		backups.Reverse();
		if (backups.Count > 0)
		{
			int num4 = 0;
			if (backups.Contains(filename))
			{
				Delete(GetBackupFilepath(filename));
				num4 = backups.IndexOf(filename);
				num4++;
			}
			if (num4 != -1 && num4 < backups.Count)
			{
				Read(GetBackupFilepath(backups[num4]));
			}
			else
			{
				Debug.Log("$MMJsonDataReadWriter - Read File - Some kind of corruption has occurred and unable to retrieve file from backup!".Colour(Color.red));
				OnReadError?.Invoke(new MMReadWriteError("File is corrupted!"));
			}
		}
		else
		{
			Debug.Log("$MMJsonDataReadWriter - Read File - Some kind of corruption has occurred! No backups available".Colour(Color.red));
			OnReadError?.Invoke(new MMReadWriteError("File is corrupted and reached the end of backups!"));
		}
	}

	public override void Read(string filename)
	{

		Debug.Log(("MMJsonDataReadWriter - Read File " + GetFilepath(filename)).Colour(Color.yellow));
		FileStream fileStream = null;
		Aes aes = null;
		CryptoStream cryptoStream = null;
		StreamReader streamReader = null;
		JsonTextReader jsonTextReader = null;
		try
		{
			if (FileExists(filename))
			{
				T val;
				using (fileStream = new FileStream(GetFilepath(filename), FileMode.Open))
				{
					byte[] array = new byte[1];
					fileStream.Read(array, 0, array.Length);
					if (Convert.ToChar(array[0]) == 'E')
					{
						byte[] array2 = new byte[16];
						fileStream.Read(array2, 0, array2.Length);
						using (aes = Aes.Create())
						{
							byte[] array3 = new byte[aes.IV.Length];
							int num = aes.IV.Length;
							int num2 = 0;
							while (num > 0)
							{
								int num3 = fileStream.Read(array3, num2, num);
								if (num3 == 0)
								{
									break;
								}
								num2 += num3;
								num -= num3;
							}
							using (cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(array2, array3), CryptoStreamMode.Read))
							{
								using (streamReader = new StreamReader(cryptoStream))
								{
									string text = streamReader.ReadToEnd();
									Debug.Log(text);
									val = JsonConvert.DeserializeObject<T>(text, MMSerialization.JsonSerializerSettings);
									Debug.Log("MMJsonDataReadWriter - Read File - Successfully read encrypted file!".Colour(Color.yellow));
								}
							}
						}
					}
					else
					{
						fileStream.Position = 0L;
						using (streamReader = new StreamReader(fileStream))
						{
							using (jsonTextReader = new JsonTextReader(streamReader))
							{
								val = MMSerialization.JsonSerializer.Deserialize<T>(jsonTextReader);
								Debug.Log(val);
								Debug.Log("MMJsonDataReadWriter - Read File - Successfully read file!".Colour(Color.yellow));
							}
						}
					}
				}
				if (val == null)
				{
					ScrubBackups(filename);
					return;
				}
				Action<T> onReadCompleted = OnReadCompleted;
				if (onReadCompleted != null)
				{
					onReadCompleted(val);
				}
			}
			else
			{
				Debug.Log("MMJsonDataReadWriter - Read File - File did not exist, creating new".Colour(Color.cyan));
				Directory.CreateDirectory(GetDirectory());
				Directory.CreateDirectory(GetBackupDirectory());
				Action onCreateDefault = OnCreateDefault;
				if (onCreateDefault != null)
				{
					onCreateDefault();
				}
			}
		}
		catch (Exception ex)
		{
			ScrubBackups(filename);
			Debug.Log(ex.Message.Colour(Color.red));
			Debug.LogException(ex);
		}
		finally
		{
			if (fileStream != null)
			{
				fileStream.Dispose();
			}
			if (aes != null)
			{
				aes.Dispose();
			}
			if (cryptoStream != null)
			{
				cryptoStream.Dispose();
			}
			if (streamReader != null)
			{
				streamReader.Dispose();
			}
			if (jsonTextReader != null)
			{
				jsonTextReader.Close();
			}

		




		}
	}

	public override void Delete(string filename)
	{
		Debug.Log(("MMJsonDataReadWriter - Delete " + GetFilepath(filename)).Colour(Color.yellow));
		if (!FileExists(filename))
		{
			return;
		}
		Debug.Log(("MMDataReadWriter - Deletion Successful " + GetFilepath(filename)).Colour(Color.yellow));
		File.Delete(GetFilepath(filename));
		Action onDeletionComplete = OnDeletionComplete;
		if (onDeletionComplete != null)
		{
			onDeletionComplete();
		}
		foreach (string backup in GetBackups(filename))
		{
			File.Delete(GetBackupFilepath(backup));
		}
	}

	public override bool FileExists(string filename)
	{
		Debug.Log(("MMJsonDataReadWriter - File Exists? " + GetFilepath(filename)).Colour(Color.yellow));
		return File.Exists(GetFilepath(filename));
	}

	private void MakeBackup(string filename)
	{
		if (!FileExists(filename))
		{
			return;
		}
		string backupDirectory = GetBackupDirectory();
		if (!Directory.Exists(backupDirectory))
		{
			Directory.CreateDirectory(backupDirectory);
		}
		List<string> backups = GetBackups(filename);
		string[] array = backups.ToArray();
		foreach (string text in array)
		{
			if (text.Split('.').Length != 3)
			{
				File.Delete(GetBackupFilepath(text));
				backups.Remove(text);
			}
		}
		if (backups.Count >= 10)
		{
			File.Delete(GetBackupFilepath(backups[0]));
		}
		string text2 = string.Empty;
		if (backups.Count > 0)
		{
			string[] array2 = backups.LastElement().Split('.');
			int result;
			if (array2.Length == 3 && int.TryParse(array2[1], out result))
			{
				result++;
				text2 = array2[0] + "." + result;
			}
		}
		else
		{
			text2 = filename.Split('.')[0];
			text2 += ".1";
		}
		text2 += ".json";
		File.Copy(GetFilepath(filename), GetBackupFilepath(text2), true);
	}

	private string GetMostRecentBackup(string filename)
	{
		List<string> backups = GetBackups(filename);
		if (backups.Count > 0)
		{
			return backups.LastElement();
		}
		return string.Empty;
	}

	private List<string> GetBackups(string filename)
	{
		string[] array = filename.Split('.');
		List<string> list = new List<string>();
		foreach (FileInfo item in from f in new DirectoryInfo(GetBackupDirectory()).GetFiles()
			orderby f.LastWriteTime
			select f)
		{
			if (array.Length != 0 && item.Name.Contains(array[0]))
			{
				list.Add(item.Name);
			}
		}
		return list;
	}

	private string GetDirectory()
	{
		return Path.Combine(Application.persistentDataPath, "saves");
	}

	private string GetFilepath(string filename)
	{
		return Path.Combine(GetDirectory(), filename);
	}

	private string GetBackupDirectory()
	{
		return Path.Combine(Application.persistentDataPath, "backup");
	}

	private string GetBackupFilepath(string filename)
	{
		return Path.Combine(GetBackupDirectory(), filename);
	}
}
