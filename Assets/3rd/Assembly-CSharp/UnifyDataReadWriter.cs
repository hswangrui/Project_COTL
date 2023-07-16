using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Data.Serialization;
using Newtonsoft.Json;
using Unify;
using UnityEngine;

public class UnifyDataReadWriter<T> : MMDataReadWriterBase<T>
{
	private Thread _thread;

	private static int count;

	public int BufferSize;

	static UnifyDataReadWriter()
	{
		Debug.Log("UnifyDataReadWrtier: Static constructor");
	}

	public UnifyDataReadWriter()
	{
		count++;
		Debug.Log("UnifyDataReadWriter: Constructor, count: " + count);
	}

	~UnifyDataReadWriter()
	{
		count--;
		Debug.Log("UnifyDataReadWriter: Destructor, count: " + count);
	}

	public override void Write(T data, string filename, bool encrypt, bool backup)
	{
		Debug.Log(("UnifyDataReadWriter - Write File " + filename).Colour(Color.yellow));
		if (_thread != null && _thread.IsAlive)
		{
			Debug.Log("Write thread is already running!".Colour(Color.red));
			return;
		}
		try
		{
			lock (InitUnifyGlobal._serializedMemoryStream)
			{
				if (InitUnifyGlobal._serializedMemoryStream.Length != 0L)
				{
					InitUnifyGlobal._serializedMemoryStream.SetLength(0L);
					InitUnifyGlobal._serializedMemoryStream.Seek(0L, SeekOrigin.Begin);
				}
				using (StreamWriter textWriter = new StreamWriter(InitUnifyGlobal._serializedMemoryStream, Encoding.Default, 1024, true))
				{
					using (JsonTextWriter jsonWriter = new JsonTextWriter(textWriter))
					{
						MMSerialization.JsonSerializer.Serialize(jsonWriter, data);
					}
				}
				_thread = new Thread((ThreadStart)delegate
				{
					lock (InitUnifyGlobal._serializedMemoryStream)
					{
						Zip(InitUnifyGlobal._outputMemoryStream, InitUnifyGlobal._serializedMemoryStream);
						SaveData.PutBytes(filename, InitUnifyGlobal._outputMemoryStream.GetBuffer(), (int)InitUnifyGlobal._outputMemoryStream.Length);
						UnifyComponent.Instance.MainThreadEnqueue(delegate
						{
							Action onWriteCompleted = OnWriteCompleted;
							if (onWriteCompleted != null)
							{
								onWriteCompleted();
							}
						});
					}
				});
				_thread.Start();
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message.Colour(Color.red));
			Action<MMReadWriteError> onWriteError = OnWriteError;
			if (onWriteError != null)
			{
				onWriteError(new MMReadWriteError(ex.Message));
			}
			throw;
		}
	}

	public override void Read(string filename)
	{
		Debug.Log(("UnifyDataReadWriter - Read File " + filename).Colour(Color.yellow));
		try
		{
			if (FileExists(filename))
			{
				Debug.Log("UnifyDataReadWriter - Read File - Successfully read file!".Colour(Color.yellow));
				DoLoad(filename);
				return;
			}
			Debug.Log("UnifyDataReadWriter - Read File - File did not exist, creating new".Colour(Color.yellow));
			Action onCreateDefault = OnCreateDefault;
			if (onCreateDefault != null)
			{
				onCreateDefault();
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message.Colour(Color.red));
			Action<MMReadWriteError> onReadError = OnReadError;
			if (onReadError != null)
			{
				onReadError(new MMReadWriteError(ex.Message));
			}
			throw;
		}
	}

	private void DoLoad(string filename)
	{
		byte[] bytes = SaveData.GetBytes(filename);
		char c = Convert.ToChar(bytes[0]);
		char c2 = Convert.ToChar(bytes[1]);
		T obj;
		if ((c == 'Z' && c2 == 'P') || (c == 'Z' && c2 == 'B'))
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				memoryStream.Seek(2L, SeekOrigin.Begin);
				using (GZipStream stream = new GZipStream(memoryStream, CompressionMode.Decompress, true))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						using (JsonTextReader reader2 = new JsonTextReader(reader))
						{
							obj = MMSerialization.JsonSerializer.Deserialize<T>(reader2);
						}
					}
				}
			}
		}
		else
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using (MemoryStream stream2 = new MemoryStream(bytes))
			{
				obj = (T)xmlSerializer.Deserialize(stream2);
			}
		}
		Action<T> onReadCompleted = OnReadCompleted;
		if (onReadCompleted != null)
		{
			onReadCompleted(obj);
		}
	}

	public override void Delete(string filename)
	{
		Debug.Log(("UnifyDataReadWriter - Delete File " + filename).Colour(Color.yellow));
		if (FileExists(filename))
		{
			Debug.Log(("UnifyDataReadWriter - Deletion Successful " + filename).Colour(Color.yellow));
			SaveData.Delete(filename);
			Action onDeletionComplete = OnDeletionComplete;
			if (onDeletionComplete != null)
			{
				onDeletionComplete();
			}
		}
	}

	public override bool FileExists(string filename)
	{
		Debug.Log(("UnifyDataReadWriter - File Exists? " + filename).Colour(Color.yellow));
		return SaveData.Exists(filename);
	}

	private static void Zip(MemoryStream mso, MemoryStream data)
	{
		if (mso.Length != 0L)
		{
			mso.SetLength(0L);
			mso.Seek(0L, SeekOrigin.Begin);
		}
		mso.WriteByte(90);
		mso.WriteByte(66);
		using (GZipStream destination = new GZipStream(mso, CompressionMode.Compress))
		{
			data.Seek(0L, SeekOrigin.Begin);
			data.CopyTo(destination);
		}
	}

	public void WriteScreenshotFile(T data, string filename)
	{
		Debug.Log(("UnifyDataReadWriter - Write File " + filename).Colour(Color.yellow));
		if (_thread != null && _thread.IsAlive)
		{
			Debug.Log("Write thread is already running!".Colour(Color.red));
			return;
		}
		try
		{
			lock (InitUnifyGlobal._serializedMemoryStream)
			{
				_thread = new Thread((ThreadStart)delegate
				{
					lock (InitUnifyGlobal._serializedMemoryStream)
					{
						InitUnifyGlobal._serializedMemoryStream.SetLength(0L);
						InitUnifyGlobal._serializedMemoryStream.Seek(0L, SeekOrigin.Begin);
						float delayBetweenSaves = SessionManager.instance.delayBetweenSaves;
						SessionManager.instance.delayBetweenSaves = -1f;
						new BinaryFormatter().Serialize(InitUnifyGlobal._serializedMemoryStream, data);
						SaveData.PutBytes(filename, InitUnifyGlobal._serializedMemoryStream.GetBuffer(), (int)InitUnifyGlobal._serializedMemoryStream.Length);
						BufferSize = (int)InitUnifyGlobal._serializedMemoryStream.Length;
						SessionManager.instance.delayBetweenSaves = delayBetweenSaves;
						UnifyComponent.Instance.MainThreadEnqueue(delegate
						{
							Action onWriteCompleted = OnWriteCompleted;
							if (onWriteCompleted != null)
							{
								onWriteCompleted();
							}
						});
					}
				});
				_thread.Start();
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message.Colour(Color.red));
			Action<MMReadWriteError> onWriteError = OnWriteError;
			if (onWriteError != null)
			{
				onWriteError(new MMReadWriteError(ex.Message));
			}
			throw;
		}
	}

	public void ReadScreenshotFile(string filename)
	{
		Debug.Log(("UnifyDataReadWriter - Read File " + filename).Colour(Color.yellow));
		try
		{
			if (FileExists(filename))
			{
				Debug.Log("UnifyDataReadWriter - Read File - Successfully read file!".Colour(Color.yellow));
				SaveData.GetBytes(filename);
				lock (InitUnifyGlobal._serializedMemoryStream)
				{
					InitUnifyGlobal._serializedMemoryStream.Seek(0L, SeekOrigin.Begin);
					byte[] bytes = SaveData.GetBytes(filename);
					InitUnifyGlobal._serializedMemoryStream.Write(bytes, 0, bytes.Length);
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					InitUnifyGlobal._serializedMemoryStream.Seek(0L, SeekOrigin.Begin);
					T obj = (T)binaryFormatter.Deserialize(InitUnifyGlobal._serializedMemoryStream);
					BufferSize = bytes.Length;
					Action<T> onReadCompleted = OnReadCompleted;
					if (onReadCompleted != null)
					{
						onReadCompleted(obj);
					}
					return;
				}
			}
			Debug.Log("UnifyDataReadWriter - Read File - File did not exist, creating new".Colour(Color.yellow));
			Action<MMReadWriteError> onReadError = OnReadError;
			if (onReadError != null)
			{
				onReadError(new MMReadWriteError("File did not exist"));
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message.Colour(Color.red));
			Action<MMReadWriteError> onReadError2 = OnReadError;
			if (onReadError2 != null)
			{
				onReadError2(new MMReadWriteError(ex.Message));
			}
			throw;
		}
	}
}
