using Unify.Standalone;

namespace Unify
{
	public class UserStorage
	{
		public delegate void ResultCallbackDelegate(string key, byte[] data, string result);

		public delegate void UserStorageCallbackDelegate(UserStorage storage);

		public const string ERROR_NONE = null;

		public const string ERROR_SUSPENDING = "error:suspending";

		public const string ERROR_NOT_CONNECTED = "error:notconnected";

		public const string ERROR_WRITE_FAILED = "error:writefailed";

		public const string ERROR_DELETE_FAILED = "error:deletefailed";

		public const string ERROR_READ_FAILED = "error:readfailed";

		public const string ERROR_NOT_FOUND = "error:notfound";

		public const string ERROR_TOO_MANY_OPS = "error:toomanyops";

		public const string ERROR_CANCELLED = "error:cancelled";

		public const string ERROR_DATA_SIZE = "error:datasize";

		public const string ERROR_NO_DATA = "error:nodata";

		public const int MAX_SAVE_DATA_SIZE = 16777216;

		public int MinSaveDataSizeBytes;

		private int userId;

		public static UserStorage Create(int userId, string containerName, UserStorageCallbackDelegate callback)
		{
			return new Unify.Standalone.UserStorage(userId, containerName, callback);
		}

		public virtual void Close()
		{
		}

		public virtual void writeKeyValue(string key, byte[] value, ResultCallbackDelegate successCallback = null, ResultCallbackDelegate failCallback = null)
		{
		}

		public void writeKey(string key, byte[] value, ResultCallbackDelegate successCallback = null, ResultCallbackDelegate failCallback = null)
		{
			writeKeyValue(key, value, successCallback, failCallback);
		}

		public virtual void deleteKey(string key, ResultCallbackDelegate successCallback = null, ResultCallbackDelegate failCallback = null)
		{
		}

		public virtual void readKey(string key, ResultCallbackDelegate successCallback = null, ResultCallbackDelegate failCallback = null)
		{
		}
	}
}
