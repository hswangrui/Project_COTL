using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class MMXMLDataReadWriter<T> : MMDataReadWriterBase<T>
{
	private const string kSaveDirectory = "saves";

	public override void Write(T data, string filename, bool encrypt = true, bool backup = true)
	{
		try
		{
			Debug.Log(("MMXMLDataReadWriter - Write File " + GetFilepath(filename)).Colour(Color.yellow));
			using (FileStream fileStream = new FileStream(GetFilepath(filename), FileMode.Create))
			{
				new XmlSerializer(typeof(T)).Serialize(fileStream, data);
				fileStream.Close();
			}
			Action onWriteCompleted = OnWriteCompleted;
			if (onWriteCompleted != null)
			{
				onWriteCompleted();
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message.Colour(Color.red));
			throw;
		}
	}

	public override void Read(string filename)
	{
		try
		{
			Debug.Log(("MMDataReadWriter - Read File " + GetFilepath(filename)).Colour(Color.yellow));
			if (FileExists(filename))
			{
				T obj;
				using (FileStream fileStream = new FileStream(GetFilepath(filename), FileMode.Open))
				{
					using (StreamReader textReader = new StreamReader(fileStream))
					{
						Debug.Log("MMDataReadWriter - Read File - Successfully read file!".Colour(Color.magenta));
						obj = (T)new XmlSerializer(typeof(T)).Deserialize(textReader);
						fileStream.Close();
					}
				}
				Action<T> onReadCompleted = OnReadCompleted;
				if (onReadCompleted != null)
				{
					onReadCompleted(obj);
				}
			}
			else
			{
				Debug.Log("MMDataReadWriter - Read File - File did not exist, creating new".Colour(Color.cyan));
				Directory.CreateDirectory(GetDirectory());
				Action onCreateDefault = OnCreateDefault;
				if (onCreateDefault != null)
				{
					onCreateDefault();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message.Colour(Color.red));
			throw;
		}
	}

	public override void Delete(string filename)
	{
		Debug.Log(("MMXMLDataReadWriter - Delete " + GetFilepath(filename)).Colour(Color.yellow));
		if (FileExists(filename))
		{
			Debug.Log(("MMXMLDataReadWriter - Deletion Successful " + GetFilepath(filename)).Colour(Color.yellow));
			File.Delete(GetFilepath(filename));
			Action onDeletionComplete = OnDeletionComplete;
			if (onDeletionComplete != null)
			{
				onDeletionComplete();
			}
		}
	}

	public override bool FileExists(string filename)
	{
		Debug.Log(("MMXMLDataReadWriter - File Exists? " + GetFilepath(filename)).Colour(Color.yellow));
		return File.Exists(GetFilepath(filename));
	}

	private string GetDirectory()
	{
		return Path.Combine(Application.persistentDataPath, "saves");
	}

	private string GetFilepath(string filename)
	{
		return Path.Combine(GetDirectory(), filename);
	}
}
