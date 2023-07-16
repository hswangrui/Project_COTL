namespace Unify
{
	public class SaveDataHelper
	{
		public static void Put(string file, string data)
		{
			SaveData.Put(file, data);
		}

		public static void PutBytes(string file, byte[] data)
		{
			SaveData.PutBytes(file, data);
		}
	}
}
