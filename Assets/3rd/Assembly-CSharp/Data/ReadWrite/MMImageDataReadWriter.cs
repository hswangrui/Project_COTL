using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Data.ReadWrite
{
	public class MMImageDataReadWriter : MMImageReadWriterBase<Texture2D>
	{
		private const string kSaveDirectory = "Photos";

		public override void Write(Texture2D data, string filename)
		{
			bool flag = true;
			FileStream fileStream = null;
			Debug.Log(("MMImageDataReadWriter - Write File " + GetFilepath(filename)).Colour(Color.yellow));
			try
			{
				string directory = GetDirectory();
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				byte[] array = data.EncodeToJPG(100);
				using (fileStream = new FileStream(GetFilepath(filename), FileMode.Create))
				{
					fileStream.Write(array, 0, array.Length);
				}
			}
			catch (Exception ex)
			{
				OnWriteError?.Invoke(new MMReadWriteError(ex.Message));
				Debug.Log(ex.Message.Colour(Color.red));
				Debug.LogException(ex);
				flag = false;
			}
			finally
			{
				fileStream?.Dispose();
				if (flag)
				{
					OnWriteCompleted?.Invoke();
				}
			}
		}

		public override void Read(string filename)
		{
			Debug.Log(("MMImageDataReadWriter - Read File " + GetFilepath(filename)).Colour(Color.yellow));
			bool flag = true;
			Texture2D texture2D = null;
			try
			{
				if (FileExists(filename))
				{
					texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
					byte[] data = File.ReadAllBytes(GetFilepath(filename));
					texture2D.LoadImage(data);
					texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				}
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message.Colour(Color.red));
				Debug.LogException(ex);
				OnReadError?.Invoke(new MMReadWriteError(ex.Message));
				flag = false;
			}
			finally
			{
				if (flag)
				{
					OnReadCompleted?.Invoke(texture2D);
				}
			}
		}

		public override void Delete(string filename)
		{
			Debug.Log(("MMImageDataReadWriter - Delete " + GetFilepath(filename)).Colour(Color.yellow));
			if (FileExists(filename))
			{
				Debug.Log(("MMImageDataReadWriter - Deletion Successful " + GetFilepath(filename)).Colour(Color.yellow));
				File.Delete(GetFilepath(filename));
				OnDeletionComplete?.Invoke();
			}
		}

		public override bool FileExists(string filename)
		{
			Debug.Log(("MMImageDataReadWriter - File Exists? " + GetFilepath(filename)).Colour(Color.yellow));
			return File.Exists(GetFilepath(filename));
		}

		public override string[] GetFiles()
		{
			if (Directory.Exists(GetDirectory()))
			{
				FileInfo[] files = new DirectoryInfo(GetDirectory()).GetFiles();
				files = files.OrderBy((FileInfo p) => p.CreationTime).ToArray();
				files = files.Reverse().ToArray();
				string[] array = new string[files.Length];
				for (int i = 0; i < files.Length; i++)
				{
					array[i] = files[i].Name.Replace(files[i].Extension, string.Empty);
				}
				return array;
			}
			return Array.Empty<string>();
		}

		public static string GetDirectory()
		{
			return Path.Combine(Application.persistentDataPath, "Photos");
		}

		private static string GetFilepath(string filename)
		{
			return Path.Combine(GetDirectory(), filename + ".jpeg");
		}
	}
}
