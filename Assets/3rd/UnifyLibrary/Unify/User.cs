using System;
using Unify.Input;

namespace Unify
{
	public class User
	{
		public int id;

		public string nickName;

		public string realName;

		public uint score;

		public string pictureUrl;

		public GamePad gamePadId;

		private UserStorage storage;

		public UserStorage Storage => storage;

		public User(int id, string nickName, string realName, uint score)
		{
			this.id = id;
			this.nickName = nickName;
			this.realName = realName;
			this.score = score;
			gamePadId = GamePad.None;
		}

		public void Destroy()
		{
			CloseStorage();
		}

		public void OpenStorage(UserStorage.UserStorageCallbackDelegate callback)
		{
			storage = UserStorage.Create(id, "test", delegate(UserStorage s)
			{
				storage = s;
				callback(s);
			});
			Logger.Log("User.OpenStorage: " + storage);
		}

		public virtual void CloseStorage()
		{
			Logger.Log("USER: CloseStorage: " + storage);
			if (storage != null)
			{
				try
				{
					storage.Close();
				}
				catch (Exception)
				{
				}
			}
			storage = null;
		}

		public static int UserAsId(User u)
		{
			return u?.id ?? (-1);
		}

		public bool IsAnonymous()
		{
			return id < 0;
		}

		public bool IsSignedIn()
		{
			return id >= 0;
		}

		public bool IsSame(User b)
		{
			return UserAsId(this) == UserAsId(b);
		}

		public static User getCurrentUser()
		{
			return UserHelper.GetPlayer(0);
		}
	}
}
