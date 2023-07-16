using System.Collections.Generic;
using Unify.Input;
using UnityEngine;

namespace Unify
{
	public class UserHelper
	{
		public enum Permissions
		{
			ViewUserGeneratedContent = 0
		}

		public enum EngagementResult
		{
			Failed = -1,
			Engaged = 0,
			RequiresSignin = 1000,
			EngagedAsOtherPlayer = -2,
			PlayerTaken = -3
		}

		public delegate void userPictureCallbackDelegate(Texture2D texture);

		public delegate void PlayerUserChangedDelegate(int playerNo, User was, User now);

		public delegate void PlayerGamePadChangedDelegate(int playerNo, User user);

		private static UserHelper singleton;

		protected static User[] playerUsers = new User[4];

		public const int MaxPlayers = 4;

		public static int activePlayers = 0;

		public static PlayerUserChangedDelegate OnPlayerUserChanged;

		public static PlayerGamePadChangedDelegate OnPlayerGamePadChanged;

		public static UserHelper Instance => singleton;

		public static void Init()
		{
			Logger.Log("USERHELPER: Init");
			playerUsers = new User[4];
			singleton = new UserHelper();
		}

		public virtual void Update()
		{
		}

		public virtual void Destroy()
		{
		}

		public static void Clear()
		{
			playerUsers = new User[4];
		}

		public virtual User LookupUserIdForPlayer(int playerId, GamePad gamePadId)
		{
			if (gamePadId == GamePad.None)
			{
				return null;
			}
			int num = gamePadId.GetJoystickId() + 1;
			return new User(num, "Player" + num, "Player " + num, 0u);
		}

		private static bool CompareUsers(User a, User b)
		{
			return UserAsId(a) == UserAsId(b);
		}

		private static int UserAsId(User u)
		{
			return u?.id ?? (-1);
		}

		public static EngagementResult EngagePlayer(int playerNo, GamePad gamePadId)
		{
			Logger.Log("USERS: EngagePlayer: " + playerNo + " gamePadId: " + gamePadId);
			User user = singleton.LookupUserIdForPlayer(playerNo, gamePadId);
			if (user == null || user.id < 0)
			{
				Logger.Log("USERS: User not found for gamepad: " + gamePadId);
				singleton.ShowAccountPicker(gamePadId);
				return EngagementResult.RequiresSignin;
			}
			return EngagePlayerUser(playerNo, gamePadId, user);
		}

		public static EngagementResult EngagePlayerUser(int playerNo, GamePad gamePadId, User user)
		{
			Logger.Log("USERS: found user: " + UserAsId(user));
			for (int i = 0; i < playerUsers.Length; i++)
			{
				if (playerNo != i && CompareUsers(playerUsers[i], user))
				{
					Logger.Log("USERS: user: " + user.id + ", is already registered as player: " + i);
					return EngagementResult.EngagedAsOtherPlayer;
				}
			}
			User player = GetPlayer(playerNo);
			if (player != null && !user.IsSame(player))
			{
				Logger.Log("USERS: cannot engage user: " + User.UserAsId(user) + "for player " + playerNo + ", because existing user is regsitered: " + User.UserAsId(player));
				return EngagementResult.PlayerTaken;
			}
			RegisterPlayer(playerNo, user, gamePadId);
			return EngagementResult.Engaged;
		}

		public static void DisengageAllPlayers()
		{
			Logger.Log("USERS: DisengageAllPlayers.");
			for (int i = 0; i < playerUsers.Length; i++)
			{
				if (playerUsers[i] != null)
				{
					RegisterPlayer(i, null, GamePad.None);
				}
			}
		}

		public static void DisengagePlayer(int playerId)
		{
			if (playerUsers[playerId] != null)
			{
				RegisterPlayer(playerId, null, GamePad.None);
			}
		}

		public static void RegisterPlayer(int playerNo, User user, GamePad gamePadId)
		{
			User user2 = playerUsers[playerNo];
			if (user2 != null)
			{
				_ = user2.gamePadId;
			}
			else
			{
				_ = GamePad.None;
			}
			if (UserAsId(user) < 0)
			{
				user = null;
			}
			playerUsers[playerNo] = user;
			if (!CompareUsers(user2, user))
			{
				Logger.Log("USERS: Player " + playerNo + " registration changed, was: " + UserAsId(user2) + ", now: " + UserAsId(user));
				if (user2 == null)
				{
					activePlayers++;
				}
				else if (user == null)
				{
					activePlayers--;
				}
				if (OnPlayerUserChanged != null)
				{
					OnPlayerUserChanged(playerNo, user2, user);
				}
			}
			SetUserGamePad(playerNo, user, gamePadId);
		}

		public static void SetUserGamePad(int playerNo, User user, GamePad gamePad)
		{
			if (user != null)
			{
				if (gamePad != GamePad.None)
				{
					Logger.Log("USERS: UpdateGamePad, set gamepad: " + gamePad.ToString());
				}
				else
				{
					Logger.Log("USERS: UpdateGamePad, set no gamepad.");
				}
				user.gamePadId = gamePad;
			}
			if (user != null)
			{
				Logger.Log("USERS: Player " + playerNo + " GamePad Changed: " + gamePad.ToString());
				if (OnPlayerGamePadChanged != null)
				{
					OnPlayerGamePadChanged(playerNo, user);
				}
			}
		}

		public static void CheckActivePlayers()
		{
			Logger.Log("USERS: CheckActivePlayers");
			for (int i = 0; i < playerUsers.Length; i++)
			{
				User user = playerUsers[i];
				if (user != null)
				{
					if (!singleton.IsSignedIn(user))
					{
						Logger.Log("USERS: removing user for inactive player: " + i);
						RegisterPlayer(i, null, GamePad.None);
					}
					else
					{
						Logger.Log("USERS: updating active player: " + i);
						RegisterPlayer(i, user, user.gamePadId);
					}
				}
			}
		}

		public static int GetNextOpenPlayerSlot()
		{
			for (int i = 0; i < playerUsers.Length; i++)
			{
				if (playerUsers[i] == null)
				{
					return i;
				}
			}
			return -1;
		}

		public static User[] GetAllPlayers()
		{
			return playerUsers;
		}

		public static User GetPlayer(int playerNo)
		{
			return playerUsers[playerNo];
		}

		public static GamePad GetPlayerGamePad(int playerNo)
		{
			if (playerUsers[playerNo] == null)
			{
				return GamePad.None;
			}
			return playerUsers[playerNo].gamePadId;
		}

		public static int GetNumPlayers()
		{
			return playerUsers.Length;
		}

		public virtual void SyncControllers()
		{
		}

		public virtual void ShowAccountPicker(GamePad gamePadId)
		{
			Logger.Log("USER: show account picker for gamepad: " + gamePadId);
		}

		public virtual bool IsSignedIn(User user)
		{
			return true;
		}

		public virtual bool IsAllowed(User user, Permissions permission)
		{
			return true;
		}

		public virtual RequestAuthorizer CreateRequestAuthorizer()
		{
			return new RequestAuthorizer();
		}

		public virtual void SetPresence(User user, string presenceString)
		{
			Logger.Log("PRESENCE: SetPrescence userId: " + user.id + ", string: " + presenceString);
		}

		public virtual void SetPresenceForAll(string presenceString)
		{
			User[] array = playerUsers;
			foreach (User user in array)
			{
				if (user != null)
				{
					SetPresence(user, presenceString);
				}
			}
		}

		public virtual void GetUserPicture(User user, int imageSize, userPictureCallbackDelegate callback)
		{
			Logger.Log("USERHELPER: GetUserPicture: " + user.id + ", " + imageSize + ", " + callback);
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.SetPixel(0, 0, Color.red);
			texture2D.SetPixel(0, 1, Color.green);
			texture2D.SetPixel(1, 0, Color.blue);
			texture2D.SetPixel(1, 1, Color.white);
			texture2D.Apply();
			callback(texture2D);
		}

		public virtual void GetPlatformAchievementStatus(Achievements.PlatformAchievementsStatusDelegate callback)
		{
			Logger.Log("USERHELPER: GetPlatformAchievementStatus not implemented");
			callback?.Invoke(null);
		}

		public virtual List<string> GetFriendsList()
		{
			Logger.Log("UserHelper:GetFriendsList default implementation.");
			return new List<string>();
		}
	}
}
