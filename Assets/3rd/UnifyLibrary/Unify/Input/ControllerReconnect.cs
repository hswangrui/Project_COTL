using UnityEngine;

namespace Unify.Input
{
	public class ControllerReconnect : MonoBehaviour
	{
		private static bool disconnected;

		private static int visibleCount;

		public int player;

		public string engagementAction;

		public bool global;

		private static GameObject globalInstance;

		private UnifyManager unifyManager;

		public static bool IsVisible => visibleCount > 0;

		public void Start()
		{
			Logger.Log("UNIFY: ControllerReconnect, Awake");
			if (global)
			{
				if (globalInstance != null && globalInstance != base.gameObject)
				{
					Object.DestroyImmediate(base.gameObject);
				}
				else
				{
					Logger.Log("UNIFY: ControllerReconnect, global set.");
					Object.DontDestroyOnLoad(base.gameObject);
					globalInstance = base.gameObject;
				}
			}
			unifyManager = UnifyManager.Get();
		}

		public void OnEnable()
		{
			visibleCount++;
		}

		public void OnDisable()
		{
			visibleCount--;
		}

		public void OnDestroy()
		{
		}

		public void Update()
		{
			GamePad gamePad = (string.IsNullOrEmpty(engagementAction) ? InputManager.Instance.GetGamePadWithAnyAction() : InputManager.Instance.GetGamePadWithAction(engagementAction));
			if (!(gamePad != GamePad.None))
			{
				return;
			}
			Logger.Log("UNIFY: ControllerReconnect: Button pressed by player: " + gamePad);
			User[] allPlayers = UserHelper.GetAllPlayers();
			foreach (User user in allPlayers)
			{
				if (user != null && gamePad == user.gamePadId)
				{
					Debug.Log("UNIFY: ControllerReconnect: Player already engaged, ignoring...");
					return;
				}
			}
			User user2 = UserHelper.GetPlayer(player);
			if (user2 != null)
			{
				Debug.Log("UNIFY: ControllerReconnect: Setting gamepad for user: " + user2);
				UserHelper.SetUserGamePad(player, user2, gamePad);
			}
		}
	}
}
