using System.IO;
using System.IO.Compression;

public class Compression
{
	public static byte[] Compress(byte[] data)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
			{
				gZipStream.Write(data, 0, data.Length);
				gZipStream.Close();
				return memoryStream.ToArray();
			}
		}
	}
}
