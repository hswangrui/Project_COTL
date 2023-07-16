using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.Assets;
using Lamb.UI.PauseMenu;
//using Microsoft.CSharp.RuntimeBinder;
using MMBiomeGeneration;
using MMTools;
using src.UI.Overlays.TutorialOverlay;
using src.UINavigator;
using Steamworks;
using Unify;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

public class GameManager : MonoBehaviour
{
	[CompilerGenerated]
	private static class _003C_003Eo__53
	{
		public static CallSite<Func<CallSite, Type, object, float, float, object>> _003C_003Ep__0;

		public static CallSite<Func<CallSite, object, float>> _003C_003Ep__1;

		public static CallSite<Func<CallSite, Type, object, Color, float, object>> _003C_003Ep__2;

		public static CallSite<Func<CallSite, object, Color>> _003C_003Ep__3;

		public static CallSite<Func<CallSite, Type, object, Vector2, float, object>> _003C_003Ep__4;

		public static CallSite<Func<CallSite, object, Vector2>> _003C_003Ep__5;
	}

	private static GameManager instance;

	private static bool reactivateGraph = false;

	private static float reactivateGraphTimer;

	public GameObject GameOverScreen;

	public static float CurrentZ;

	public static bool IsQuitting;

	public UpgradeTreeConfiguration UpgradeTreeConfiguration;

	public UpgradeTreeConfiguration UpgradePlayerConfiguration;

	public RenderTexture LightingRenderTexture;

	private Color StartItemsInWoodsColor = new Color(0.0741f, 0f, 0.129f, 1f);

	public float _UnscaledTime;

	private float scaledTimeElapsed;

	private AudioSource audioSource;

	public static bool GoG_Initialised = false;

	private float autoPauseTimestamp;

	private const float idleTimeUntilAutoPause = 600f;

	public static bool overridePlayerPosition = false;

	private static Coroutine AdjustShadersCoroutine;

	public static float _timeinDungeon = 0f;

	private List<int> GameSpeed = new List<int> { 1, 2, 3 };

	private int CurrentGameSpeed;

	public CameraFollowTarget _CamFollowTarget;

	public List<CameraFollowTarget.Target> CachedCamTargets = new List<CameraFollowTarget.Target>();

	public static bool InMenu = false;

	private UIPauseMenuController _pauseMenuInstance;

	private bool DisplayingInactiveWarning;

	private float HoldToResetDemo;

	public Coroutine cGeneratePathfinding;

	private static float TimeScale = 1f;

	public static bool DungeonUseAllLayers = false;

	public static int CurrentDungeonLayer = 1;

	public static int PreviousDungeonLayer = 0;

	public static int CurrentDungeonFloor = 1;

	private static int _DungeonEndlessLevel = 1;

	public static string DungeonNameTerm;

	public static bool InitialDungeonEnter = true;

	public static bool CULTISTPAC_DLC = false;

	public static bool HERETICPAC_DLC = false;

	public static bool CTHULHUWOR_DLC = false;

	public EndOfDay EndOfDayScreen;

	private static readonly int GlobalDitherIntensity = Shader.PropertyToID("_GlobalDitherIntensity");

	private static readonly int GlobalResourceHighlight = Shader.PropertyToID("_GlobalResourceHighlight");

	private static readonly int TimeOfDayColor = Shader.PropertyToID("_TimeOfDayColor");

	private static readonly int CloudAlpha = Shader.PropertyToID("_CloudAlpha");

	private static readonly int CloudDensity = Shader.PropertyToID("_CloudDensity");

	private static readonly int VerticalFogGradientSpread = Shader.PropertyToID("_VerticalFog_GradientSpread");

	private static readonly int VerticalFogZOffset = Shader.PropertyToID("_VerticalFog_ZOffset");

	private static readonly int LightingRenderTexture1 = Shader.PropertyToID("_Lighting_RenderTexture");

	private static readonly int WindDensity = Shader.PropertyToID("_WindDensity");

	private static readonly int WindSpeed = Shader.PropertyToID("_WindSpeed");

	private static readonly int WindDiection = Shader.PropertyToID("_WindDiection");

	private static readonly int ItemInWoodsColor = Shader.PropertyToID("_ItemInWoodsColor");

	private static readonly int GlobalTimeUnscaled = Shader.PropertyToID("_GlobalTimeUnscaled");

	public static bool RoomActive
	{
		get
		{
			if (BiomeGenerator.Instance != null)
			{
				return BiomeGenerator.Instance.CurrentRoom.Active;
			}
			return true;
		}
	}

	public float CurrentTime
	{
		get
		{
			return scaledTimeElapsed;
		}
	}

	public static float DeltaTime
	{
		get
		{
			return Time.deltaTime * 60f;
		}
	}

	public static float UnscaledDeltaTime
	{
		get
		{
			return Time.unscaledDeltaTime * 60f;
		}
	}

	public static float FixedDeltaTime
	{
		get
		{
			return Time.fixedDeltaTime * 60f;
		}
	}

	public static float FixedUnscaledDeltaTime
	{
		get
		{
			return Time.fixedUnscaledDeltaTime * 60f;
		}
	}

	public static float TimeInDungeon
	{
		get
		{
			return _timeinDungeon;
		}
		set
		{
			_timeinDungeon = value;
		}
	}

	public CameraFollowTarget CamFollowTarget
	{
		get
		{
			if (_CamFollowTarget == null || !_CamFollowTarget.gameObject.activeSelf)
			{
				_CamFollowTarget = CameraFollowTarget.Instance;
			}
			return _CamFollowTarget;
		}
	}

	public float CachedZoom { get; set; } = -999f;


	public static int DungeonEndlessLevel
	{
		get
		{
			return _DungeonEndlessLevel;
		}
		set
		{
			Debug.Log("CHANGING ENDLESS LEVEL " + _DungeonEndlessLevel + "   -   " + value);
			_DungeonEndlessLevel = value;
		}
	}

	public static bool Layer2
	{
		get
		{
			if (DataManager.Instance.DeathCatBeaten)
			{
				return !DungeonSandboxManager.Active;
			}
			return false;
		}
	}

	public static bool SandboxDungeonEnabled
	{
		get
		{
			return DungeonSandboxManager.Instance != null;
		}
	}

	private void Awake()
	{
		instance = this;
		GarbageCollector.incrementalTimeSliceNanoseconds = 1000000uL;
		autoPauseTimestamp = Time.unscaledTime + 600f;
		Singleton<SettingsManager>.Instance.LoadAndApply();
		DataManager.Instance.CurrentWeapon = EquipmentType.None;
		DataManager.Instance.CurrentWeaponLevel = 0;
		DataManager.Instance.CurrentCurse = EquipmentType.None;
		DataManager.Instance.CurrentCurseLevel = 0;
		DataManager.Instance.CurrentRelic = RelicType.None;
		DataManager.Instance.RelicChargeAmount = 0f;
		DataManager.Instance.PLAYER_STARTING_HEALTH_CACHED = DataManager.Instance.PLAYER_STARTING_HEALTH;
		DifficultyManager.LoadCurrentDifficulty();
		DataManager.Instance.ReplaceDeprication(this);
		DataManager.Instance.Followers_Transitioning_IDs.Clear();
		if (DataManager.Instance.WeaponPool.Count == 0)
		{
			DataManager.Instance.AddWeapon(EquipmentType.Sword);
			if (CheatConsole.IN_DEMO)
			{
				DataManager.Instance.AddWeapon(EquipmentType.Dagger);
				DataManager.Instance.AddWeapon(EquipmentType.Dagger_Critical);
				DataManager.Instance.AddWeapon(EquipmentType.Dagger_Fervour);
				DataManager.Instance.AddWeapon(EquipmentType.Dagger_Healing);
				DataManager.Instance.AddWeapon(EquipmentType.Dagger_Nercomancy);
				DataManager.Instance.AddWeapon(EquipmentType.Dagger_Poison);
				DataManager.Instance.AddWeapon(EquipmentType.Axe);
				DataManager.Instance.AddWeapon(EquipmentType.Axe_Critical);
				DataManager.Instance.AddWeapon(EquipmentType.Axe_Fervour);
				DataManager.Instance.AddWeapon(EquipmentType.Axe_Healing);
				DataManager.Instance.AddWeapon(EquipmentType.Axe_Nercomancy);
				DataManager.Instance.AddWeapon(EquipmentType.Axe_Poison);
				DataManager.Instance.AddWeapon(EquipmentType.Sword_Critical);
				DataManager.Instance.AddWeapon(EquipmentType.Sword_Fervour);
				DataManager.Instance.AddWeapon(EquipmentType.Sword_Healing);
				DataManager.Instance.AddWeapon(EquipmentType.Sword_Nercomancy);
				DataManager.Instance.AddWeapon(EquipmentType.Sword_Poison);
			}
		}
		if (DataManager.Instance.CursePool.Count == 0)
		{
			DataManager.Instance.AddCurse(EquipmentType.Fireball);
			if (CheatConsole.IN_DEMO)
			{
				DataManager.Instance.AddCurse(EquipmentType.Tentacles);
				DataManager.Instance.AddCurse(EquipmentType.EnemyBlast);
				DataManager.Instance.AddCurse(EquipmentType.ProjectileAOE);
				DataManager.Instance.AddCurse(EquipmentType.MegaSlash);
			}
		}
		if (DataManager.Instance.UnlockedFleeces.Count == 0)
		{
			DataManager.Instance.UnlockedFleeces.Add(0);
		}
		if (DataManager.Instance.RecipesDiscovered.Count == 0)
		{
			DataManager.Instance.RecipesDiscovered.Add(InventoryItem.ITEM_TYPE.MEAL_BERRIES);
		}
		if (DataManager.Instance.DiscoveredLocations.Count == 0)
		{
			DataManager.Instance.DiscoveredLocations.Add(FollowerLocation.Base);
			DataManager.Instance.VisitedLocations.Add(FollowerLocation.Base);
		}
		if (DataManager.Instance.UnlockedUpgrades.Count == 0)
		{
			DataManager.Instance.UnlockedUpgrades.Add(UpgradeSystem.Type.Ritual_HeartsOfTheFaithful1);
			DataManager.Instance.UnlockedUpgrades.Add(UpgradeSystem.Type.Ritual_UnlockCurse);
			DataManager.Instance.UnlockedUpgrades.Add(UpgradeSystem.Type.Ritual_UnlockWeapon);
		}
		if (DataManager.Instance.PlayerFoundTrinkets.Count == 0)
		{
			DataManager.Instance.PlayerFoundTrinkets = new List<TarotCards.Card>
			{
				TarotCards.Card.Hearts1,
				TarotCards.Card.Lovers1,
				TarotCards.Card.EyeOfWeakness,
				TarotCards.Card.Telescope,
				TarotCards.Card.DiseasedHeart,
				TarotCards.Card.Spider,
				TarotCards.Card.AttackRate,
				TarotCards.Card.IncreasedDamage,
				TarotCards.Card.IncreaseBlackSoulsDrop,
				TarotCards.Card.NegateDamageChance,
				TarotCards.Card.AmmoEfficient,
				TarotCards.Card.HealTwiceAmount,
				TarotCards.Card.DeathsDoor,
				TarotCards.Card.GiftFromBelow,
				TarotCards.Card.RabbitFoot
			};
		}
		if (DataManager.Instance.WeaponSelectionPositions.Count == 0)
		{
			DataManager.Instance.WeaponSelectionPositions = new List<TarotCards.Card>
			{
				TarotCards.Card.Dagger,
				TarotCards.Card.Axe,
				TarotCards.Card.Gauntlet,
				TarotCards.Card.Hammer
			};
		}
		//TwitchAuthentication.OnAuthenticated += TwitchAuthentication_OnAuthenticated;
	}

	private void TwitchAuthentication_OnAuthenticated()
	{
		StartCoroutine(_003CTwitchAuthentication_OnAuthenticated_003Eg__Wait_007C29_0());
		if (!DataManager.Instance.TwitchSentFollowers)
		{
			//TwitchFollowers.SendFollowersAllData();
			DataManager.Instance.TwitchSentFollowers = true;
		}
	}

	private void Start()
	{
		SetGlobalShaders();
		SteamAPI.Init();
		if (!DataManager.CheckIfThereAreSkinsAvailableAll())
		{
			Debug.Log("Follower Skin Achievement Unlocked");
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_SKINS_UNLOCKED"));
		}
		CheckDLCStatus();
		DataManager.Instance.SandboxModeEnabled = false;
	}

	private void OnEnable()
	{
		instance = this;
		CachedCamTargets = new List<CameraFollowTarget.Target>();
		AudioManager.Instance.SetGameManager(this);
		MMConversation.OnConversationNew += OnConversationNew;
		MMConversation.OnConversationNext += OnConversationNext;
		MMConversation.OnConversationEnd += OnConversationEnd;
		Application.quitting += OnQuit;
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(Save));
		//TwitchAuthentication.TryAuthenticate(null);
		if (CheatConsole.IN_DEMO)
		{
			CheatConsole.ForceResetTimeSinceLastKeyPress();
		}
		CheckAchievements();
		AccessibilityManager accessibilityManager = Singleton<AccessibilityManager>.Instance;
		accessibilityManager.OnStopTimeInCrusadeChanged = (Action<bool>)Delegate.Combine(accessibilityManager.OnStopTimeInCrusadeChanged, new Action<bool>(OnStopTimeInCrusadeSettingChanged));
	}

	private void CheckAchievements()
	{
		if (!(Achievements.Instance == null) && DataManager.Instance.PlayerFoundTrinkets.Count >= DataManager.AllTrinkets.Count)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("ALL_TAROTS_UNLOCKED"));
		}
	}

	public static bool AuthenticateCultistDLC()
	{
		Debug.Log("## AuthenticateCultistDLC");
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2015880u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticateHereticDLC()
	{
		Debug.Log("## AuthenticateHereticDLC");
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2331540u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticatePrePurchaseDLC()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2013550u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticatePlushBonusDLC()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(1944680u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticatePAXDLC()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2202626u)))
		{
			return true;
		}
		return false;
	}

	private void OnDestroy()
	{
	//	TwitchAuthentication.OnAuthenticated -= TwitchAuthentication_OnAuthenticated;
	}

	private void OnDisable()
	{
		if (instance == this)
		{
			instance = null;
		}
		MMConversation.OnConversationNew -= OnConversationNew;
		MMConversation.OnConversationNext -= OnConversationNext;
		MMConversation.OnConversationEnd -= OnConversationEnd;
		Application.quitting -= OnQuit;
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(Save));
		AccessibilityManager accessibilityManager = Singleton<AccessibilityManager>.Instance;
		accessibilityManager.OnStopTimeInCrusadeChanged = (Action<bool>)Delegate.Remove(accessibilityManager.OnStopTimeInCrusadeChanged, new Action<bool>(OnStopTimeInCrusadeSettingChanged));
	}

	private void Save()
	{
		if (PlayerFarming.Location == FollowerLocation.Base)
		{
			SaveAndLoad.Save();
		}
	}

	private void OnQuit()
	{
		IsQuitting = true;
	}

	private void CheckDLCStatus()
	{
		StartCoroutine(WaitForDLCCheck(1f, delegate
		{
			if (!AuthenticateHereticDLC() && DataManager.Instance.DLC_Heretic_Pack)
			{
				Debug.Log("## Deactivate HERETIC DLC");
				DataManager.DeactivateHereticDLC();
			}
			if (!AuthenticateCultistDLC() && DataManager.Instance.DLC_Cultist_Pack)
			{
				Debug.Log("## Deactivate CULTIST DLC");
				DataManager.DeactivateCultistDLC();
			}
			if (!AuthenticatePrePurchaseDLC() && DataManager.Instance.DLC_Pre_Purchase)
			{
				Debug.Log("## Deactivate PRE PURCHASE");
				DataManager.DeactivatePrePurchaseDLC();
			}
			float num = 1f;
			if (PlayerFarming.Location == FollowerLocation.Base)
			{
				if (AuthenticateHereticDLC())
				{
					Debug.Log("## Activate HERETIC DLC");
					if (DataManager.ActivateHereticDLC())
					{
						StartCoroutine(WaitForTime(num + 0.5f, delegate
						{
							Debug.Log("## HERETIC DLC Notification");
							NotificationCentre.Instance.PlayGenericNotificationLocalizedParams("UI/DLC/ActivatedPack", "UI/DLC/HereticEdition");
						}));
					}
				}
				if (AuthenticateCultistDLC())
				{
					num += 0.5f;
					Debug.Log("## Activate CULTIST DLC");
					if (DataManager.ActivateCultistDLC())
					{
						StartCoroutine(WaitForTime(num + 1f, delegate
						{
							Debug.Log("## CULTIST DLC Notification");
							NotificationCentre.Instance.PlayGenericNotificationLocalizedParams("UI/DLC/ActivatedPack", "UI/DLC/CultistEdition");
						}));
					}
				}
				if (AuthenticatePrePurchaseDLC())
				{
					num += 0.5f;
					Debug.Log("## Activate PRE PURCHASE");
					if (DataManager.ActivatePrePurchaseDLC())
					{
						StartCoroutine(WaitForTime(num + 1.5f, delegate
						{
							Debug.Log("## PRE PURCHASE DLC Notification");
							NotificationCentre.Instance.PlayGenericNotificationLocalizedParams("UI/DLC/ActivatedPack", "UI/DLC/CthuluPack");
						}));
					}
				}
				if (AuthenticatePlushBonusDLC() && DataManager.ActivatePlushBonusDLC())
				{
					NotificationCentre.Instance.PlayGenericNotificationLocalizedParams("UI/DLC/ActivatedPack", "Structures/DECORATION_PLUSH");
				}
				if (AuthenticatePAXDLC() && DataManager.ActivatePAXDLC())
				{
					NotificationCentre.Instance.PlayGenericNotificationLocalizedParams("UI/DLC/ActivatedPack", "PAX DLC");
				}
				if (AuthenticateTwitchDrop1() && DataManager.ActivateTwitchDrop1())
				{
					NotificationCentre.Instance.PlayTwitchNotification("Notifications/Twitch/ReceivedDrop");
				}
				if (AuthenticateTwitchDrop2() && DataManager.ActivateTwitchDrop2())
				{
					NotificationCentre.Instance.PlayTwitchNotification("Notifications/Twitch/ReceivedDrop");
				}
				if (AuthenticateTwitchDrop3() && DataManager.ActivateTwitchDrop3())
				{
					NotificationCentre.Instance.PlayTwitchNotification("Notifications/Twitch/ReceivedDrop");
				}
				if (AuthenticateTwitchDrop4() && DataManager.ActivateTwitchDrop4())
				{
					NotificationCentre.Instance.PlayTwitchNotification("Notifications/Twitch/ReceivedDrop");
				}
				if (AuthenticateTwitchDrop5() && DataManager.ActivateTwitchDrop5())
				{
					NotificationCentre.Instance.PlayTwitchNotification("Notifications/Twitch/ReceivedDrop");
				}
				if (AuthenticateTwitchDrop6() && DataManager.ActivateTwitchDrop6())
				{
					NotificationCentre.Instance.PlayTwitchNotification("Notifications/Twitch/ReceivedDrop");
				}
				if (AuthenticateTwitchDrop7() && DataManager.ActivateTwitchDrop7())
				{
					NotificationCentre.Instance.PlayTwitchNotification("Notifications/Twitch/ReceivedDrop");
				}
			}
		}));
	}

	public static IEnumerator WaitForDLCCheck(float delay, Action callback)
	{
		yield return new WaitForSecondsRealtime(delay);
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator WaitForTime(float delay, Action callback)
	{
		Debug.Log("WaitForTime" + delay);
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	public void SetGlobalShaders()
	{
		Shader.SetGlobalFloat(GlobalTimeUnscaled, _UnscaledTime);
		Shader.SetGlobalColor(ItemInWoodsColor, StartItemsInWoodsColor);
		Shader.SetGlobalVector(value: new Vector2(1f, 0.2f), nameID: WindDiection);
		Shader.SetGlobalFloat(WindSpeed, 3f);
		Shader.SetGlobalFloat(WindDensity, 0.1f);
		Shader.SetGlobalTexture(LightingRenderTexture1, LightingRenderTexture);
		Shader.SetGlobalFloat(VerticalFogZOffset, 0.1f);
		Shader.SetGlobalFloat(VerticalFogGradientSpread, 1f);
		Shader.SetGlobalFloat(CloudDensity, 1f);
		Shader.SetGlobalFloat(CloudAlpha, 0.1f);
		Shader.SetGlobalColor(TimeOfDayColor, Color.white);
		Shader.SetGlobalFloat(GlobalDitherIntensity, SettingsManager.Settings.Accessibility.DitherFadeDistance);
		Shader.SetGlobalInt(GlobalResourceHighlight, 1);
	}

	private void GetPlayerPosition()
	{
		if (!overridePlayerPosition && PlayerFarming.Instance != null)
		{
			Shader.SetGlobalVector("_PlayerPosition", PlayerFarming.Instance.gameObject.transform.position);
		}
	}

	public static void setDefaultGlobalShaders()
	{
		instance.SetGlobalShaders();
	}

	public static void startCoroutineAdjustGlobalShaders(List<BiomeVolume> MyList, float BlendTime, float Start, float Target)
	{
		if (AdjustShadersCoroutine != null)
		{
			instance.StopCoroutine(AdjustShadersCoroutine);
		}
		AdjustShadersCoroutine = instance.StartCoroutine(instance.adjustGlobalShaders(MyList, BlendTime, Start, Target));
	}

	public static void SetDither(float value)
	{
		Shader.SetGlobalFloat(GlobalDitherIntensity, value);
	}

	public void SetDitherTween(float value, float duration = 1f)
	{
		DOTween.Kill(this);
		DOTween.To(SetDither, Shader.GetGlobalFloat(GlobalDitherIntensity), SettingsManager.Settings.Accessibility.DitherFadeDistance + value, duration).SetEase(Ease.OutQuart);
	}

	private IEnumerator adjustGlobalShaders(List<BiomeVolume> MyList, float duration, float Start, float Target)
	{
		float Progress = 0f;
		List<object> startValues = new List<object>();
		for (int i = 0; i < MyList.Count; i++)
		{
			if (MyList[i].types == BiomeVolume.ShaderTypes.Float)
			{
				startValues.Add(Shader.GetGlobalFloat(MyList[i].shaderName));
			}
			else if (MyList[i].types == BiomeVolume.ShaderTypes.Color)
			{
				startValues.Add(Shader.GetGlobalColor(MyList[i].shaderName));
			}
			else if (MyList[i].types == BiomeVolume.ShaderTypes.Texture)
			{
				startValues.Add(Shader.GetGlobalTexture(MyList[i].shaderName));
			}
			else if (MyList[i].types == BiomeVolume.ShaderTypes.Vector2)
			{
				startValues.Add(Shader.GetGlobalVector(MyList[i].shaderName));
			}
		}
		if (duration > 0f)
		{
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.deltaTime);
				if (!(num < duration))
				{
					break;
				}
				float t = Progress / duration;
				for (int j = 0; j < MyList.Count; j++)
				{
					if (MyList[j].types == BiomeVolume.ShaderTypes.Float)
					{
						//if (_003C_003Eo__53._003C_003Ep__0 == null)
						//{
						//	_003C_003Eo__53._003C_003Ep__0 = CallSite<Func<CallSite, Type, object, float, float, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Lerp", null, typeof(GameManager), new CSharpArgumentInfo[4]
						//	{
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
						//	}));
						//}
					//	float value = (dynamic)_003C_003Eo__53._003C_003Ep__0.Target(_003C_003Eo__53._003C_003Ep__0, typeof(Mathf), startValues[j], MyList[j].valueToGoTo, Mathf.SmoothStep(Start, Target, t));
					//	Shader.SetGlobalFloat(MyList[j].shaderName, value);
					}
					else if (MyList[j].types == BiomeVolume.ShaderTypes.Color)
					{
						//if (_003C_003Eo__53._003C_003Ep__2 == null)
						//{
						//	_003C_003Eo__53._003C_003Ep__2 = CallSite<Func<CallSite, Type, object, Color, float, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Lerp", null, typeof(GameManager), new CSharpArgumentInfo[4]
						//	{
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
						//	}));
						//}
					//	Color value2 = (dynamic)_003C_003Eo__53._003C_003Ep__2.Target(_003C_003Eo__53._003C_003Ep__2, typeof(Color), startValues[j], MyList[j].colorToGoTo, Mathf.SmoothStep(Start, Target, t));
					//	Shader.SetGlobalColor(MyList[j].shaderName, value2);
					}
					else if (MyList[j].types != BiomeVolume.ShaderTypes.Texture && MyList[j].types == BiomeVolume.ShaderTypes.Vector2)
					{
						//if (_003C_003Eo__53._003C_003Ep__4 == null)
						//{
						//	_003C_003Eo__53._003C_003Ep__4 = CallSite<Func<CallSite, Type, object, Vector2, float, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Lerp", null, typeof(GameManager), new CSharpArgumentInfo[4]
						//	{
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
						//		CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
						//	}));
						//}
						//Vector2 vector = (dynamic)_003C_003Eo__53._003C_003Ep__4.Target(_003C_003Eo__53._003C_003Ep__4, typeof(Vector2), startValues[j], MyList[j].Vector2ToGoTo, Mathf.SmoothStep(Start, Target, t));
						//Shader.SetGlobalVector(MyList[j].shaderName, vector);
					}
				}
				yield return null;
			}
			yield break;
		}
		for (int k = 0; k < MyList.Count; k++)
		{
			if (MyList[k].types == BiomeVolume.ShaderTypes.Float)
			{
				Shader.SetGlobalFloat(MyList[k].shaderName, MyList[k].valueToGoTo);
			}
			else if (MyList[k].types == BiomeVolume.ShaderTypes.Color)
			{
				Shader.SetGlobalColor(MyList[k].shaderName, MyList[k].colorToGoTo);
			}
			else if (MyList[k].types != BiomeVolume.ShaderTypes.Texture && MyList[k].types == BiomeVolume.ShaderTypes.Float)
			{
				Shader.SetGlobalVector(MyList[k].shaderName, MyList[k].Vector2ToGoTo);
			}
		}
	}

	public static bool AuthenticateTwitchDrop1()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2090901u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticateTwitchDrop2()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2071370u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticateTwitchDrop3()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2090900u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticateTwitchDrop4()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2202621u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticateTwitchDrop5()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2202620u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticateTwitchDrop6()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2202622u)))
		{
			return true;
		}
		return false;
	}

	public static bool AuthenticateTwitchDrop7()
	{
		if (SteamAPI.Init() && SteamApps.BIsSubscribedApp(new AppId_t(2202623u)))
		{
			return true;
		}
		return false;
	}

	public void NextGameSpeed()
	{
		CurrentGameSpeed = ++CurrentGameSpeed % GameSpeed.Count;
		SetTimeScale(GameSpeed[CurrentGameSpeed]);
		Debug.Log(Time.timeScale + "   " + CurrentGameSpeed + "  " + GameSpeed[CurrentGameSpeed]);
	}

	public void OnConversationNew(bool SetPlayerInactive = true, bool SnapLetterBox = false)
	{
		ResetCachedCameraTargets();
		CameraSetConversationMode(true);
		CacheCameraTargets();
		RemoveAllFromCamera();
		if (SetPlayerInactive && PlayerFarming.Instance != null)
		{
			if (PlayerFarming.Instance.GoToAndStopping)
			{
				UnitObject unitObject = PlayerFarming.Instance.unitObject;
				unitObject.EndOfPath = (Action)Delegate.Combine(unitObject.EndOfPath, new Action(PlayerInactiveOnArrival));
			}
			else
			{
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
			}
		}
		LetterBox.Show(SnapLetterBox);
		DOTween.Kill(this);
		DOTween.To(SetDither, Shader.GetGlobalFloat(GlobalDitherIntensity), SettingsManager.Settings.Accessibility.DitherFadeDistance + 5f, 1f).SetEase(Ease.OutQuart);
	}

	private void PlayerInactiveOnArrival()
	{
		UnitObject unitObject = PlayerFarming.Instance.unitObject;
		unitObject.EndOfPath = (Action)Delegate.Remove(unitObject.EndOfPath, new Action(PlayerInactiveOnArrival));
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
	}

	public void OnConversationNew(bool SetPlayerInactive, bool SnapLetterBox, bool showLetterBox)
	{
		if (NotificationCentreScreen.Instance != null)
		{
			NotificationCentreScreen.Instance.FadeAndStop();
		}
		ResetCachedCameraTargets();
		CameraSetConversationMode(true);
		CacheCameraTargets();
		RemoveAllFromCamera();
		GameObject.FindWithTag("Player");
		if (SetPlayerInactive && PlayerFarming.Instance != null)
		{
			if (PlayerFarming.Instance.GoToAndStopping)
			{
				UnitObject unitObject = PlayerFarming.Instance.unitObject;
				unitObject.EndOfPath = (Action)Delegate.Combine(unitObject.EndOfPath, new Action(PlayerInactiveOnArrival));
			}
			else
			{
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
			}
		}
		if (showLetterBox)
		{
			if (SnapLetterBox)
			{
				HUD_Manager.Instance.Hide(true, 0);
			}
			LetterBox.Show(SnapLetterBox);
			DOTween.Kill(this);
			DOTween.To(SetDither, Shader.GetGlobalFloat(GlobalDitherIntensity), SettingsManager.Settings.Accessibility.DitherFadeDistance + 5f, 1f).SetEase(Ease.OutQuart);
		}
		else
		{
			HUD_Manager.Instance.Hide(false, 0);
		}
	}

	public void OnConversationNext(GameObject Speaker, float Zoom = 9f)
	{
		if (Speaker != null)
		{
			RemoveAllFromCamera();
			AddToCamera(Speaker.gameObject);
		}
		CamFollowTarget.targetDistance = Zoom;
		if (!IsDungeon(PlayerFarming.Location))
		{
			BiomeConstants.Instance.DepthOfFieldTween(1.5f, Zoom + 1f, 10f, 1f, 145f);
		}
	}

	public void OnConversationEnd(bool SetPlayerToIdle = true, bool ShowHUD = true)
	{
		if (SetPlayerToIdle && PlayerFarming.Instance != null)
		{
			if (PlayerFarming.Instance.GoToAndStopping)
			{
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Moving;
				PlayerFarming.Instance.IdleOnEnd = true;
			}
			else
			{
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			}
		}
		DOTween.Kill(this);
		DOTween.To(SetDither, Shader.GetGlobalFloat(GlobalDitherIntensity), SettingsManager.Settings.Accessibility.DitherFadeDistance, 1f).SetEase(Ease.OutQuart);
		if (IsDungeon(PlayerFarming.Location))
		{
			BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 0f);
		}
		else
		{
			BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
		}
		CameraSetConversationMode(false);
		ResetCachedCameraTargets();
		LetterBox.Hide(ShowHUD);
	}

	public void AddPlayerToCamera()
	{
		if (PlayerFarming.Instance != null)
		{
			GetInstance().AddToCamera(PlayerFarming.Instance.CameraBone);
		}
	}

	public static GameManager GetInstance()
	{
		return instance;
	}

	private void CacheCameraTargets()
	{
		for (int num = CamFollowTarget.targets.Count - 1; num >= 0; num--)
		{
			if (CamFollowTarget.targets[num] == null || CamFollowTarget.targets[num].gameObject == null)
			{
				CamFollowTarget.targets.RemoveAt(num);
			}
		}
		if (CamFollowTarget.targets.Count <= 0)
		{
			AddPlayerToCamera();
		}
		CachedCamTargets = new List<CameraFollowTarget.Target>(CamFollowTarget.targets);
		CachedZoom = CamFollowTarget.targetDistance;
	}

	private void ResetCachedCameraTargets()
	{
		CamFollowTarget.targets = new List<CameraFollowTarget.Target>(CachedCamTargets);
		if (CachedZoom != -999f)
		{
			CamFollowTarget.targetDistance = CachedZoom;
		}
		if (CamFollowTarget.targets.Count <= 0)
		{
			AddPlayerToCamera();
		}
	}

	public void RemoveAllFromCamera()
	{
		CamFollowTarget.ClearAllTargets();
	}

	public void AddToCamera(GameObject gameObject, float Weight = 1f)
	{
		if (!(CamFollowTarget == null))
		{
			CamFollowTarget.AddTarget(gameObject, Weight);
		}
	}

	public void RemoveFromCamera(GameObject gameObject)
	{
		CamFollowTarget.RemoveTarget(gameObject);
	}

	public bool CameraContains(GameObject gameObject)
	{
		return CamFollowTarget.Contains(gameObject);
	}

	public void CameraSetZoom(float Zoom)
	{
		CamFollowTarget.distance = (CamFollowTarget.targetDistance = Zoom);
		if (!IsDungeon(PlayerFarming.Location))
		{
			BiomeConstants.Instance.DepthOfFieldTween(1.5f, Zoom + 1f, 10f, 1f, 145f);
		}
	}

	public void CameraResetTargetZoom()
	{
		CamFollowTarget.distance = (CamFollowTarget.targetDistance = 12f);
		if (BiomeConstants.Instance != null)
		{
			if (IsDungeon(PlayerFarming.Location))
			{
				BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 0f);
			}
			else
			{
				BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
			}
		}
	}

	public void CameraSetTargetZoom(float Zoom)
	{
		CamFollowTarget.targetDistance = Zoom;
		if (!IsDungeon(PlayerFarming.Location))
		{
			BiomeConstants.Instance.DepthOfFieldTween(1.5f, Zoom + 1f, 10f, 1f, 145f);
		}
	}

	public void CameraSetOffset(Vector3 Offset)
	{
		CamFollowTarget.TargetOffset = Offset;
	}

	public void CameraSetConversationMode(bool toggle)
	{
		CamFollowTarget.IN_CONVERSATION = toggle;
	}

	public void CameraSnapToPosition(Vector3 position)
	{
		CamFollowTarget.SnapTo(position);
	}

	public void HitStop(float SleepDuration = 0.1f)
	{
		if (Time.timeScale == 1f)
		{
			StartCoroutine(HitStopRoutine(SleepDuration));
		}
	}

	private IEnumerator HitStopRoutine(float SleepDuration)
	{
		Time.timeScale = 0f;
		yield return new WaitForSecondsRealtime(SleepDuration);
		Time.timeScale = 1f;
	}

	public static bool IsDungeon(FollowerLocation location)
	{
		switch (location)
		{
		case FollowerLocation.Dungeon1_1:
		case FollowerLocation.Dungeon1_2:
		case FollowerLocation.Dungeon1_3:
		case FollowerLocation.Dungeon1_4:
		case FollowerLocation.Boss_5:
		case FollowerLocation.Dungeon1_5:
		case FollowerLocation.IntroDungeon:
			return true;
		default:
			return false;
		}
	}

	private void Update()
	{
		GetPlayerPosition();
		_UnscaledTime = Time.unscaledTime;
		scaledTimeElapsed += Time.deltaTime;
		Shader.SetGlobalFloat("_GlobalTimeUnscaled", _UnscaledTime);
		Shader.SetGlobalFloat("_GlobalTimeUnscaled1", _UnscaledTime);
		if (SettingsManager.Settings.Accessibility.StopTimeInCrusade && IsDungeon(PlayerFarming.Location))
		{
			SimulationManager.Pause();
		}
		//if (TwitchAuthentication.IsAuthenticated)
		//{
		//	TwitchManager.UpdateEvents();
		//}
		if (CheatConsole.IN_DEMO)
		{
			if (InputManager.UI.GetPageNavigateLeftHeld() && InputManager.UI.GetPageNavigateRightHeld())
			{
				CheatConsole.Instance.DisplayText("Restarting... " + Mathf.Max(0, Mathf.FloorToInt(10f - HoldToResetDemo / 5f * 10f)), Color.red);
				HoldToResetDemo += Time.unscaledDeltaTime;
			}
			else
			{
				if (HoldToResetDemo != 0f)
				{
					CheatConsole.Instance.DisplayText("", Color.red);
				}
				HoldToResetDemo = 0f;
			}
			if (HoldToResetDemo > 5f)
			{
				DemoQuitToMenu();
				return;
			}
			if (HoldToResetDemo == 0f && CheatConsole.TimeSinceLastKeyPress > 90f)
			{
				DisplayingInactiveWarning = true;
				CheatConsole.Instance.DisplayText("Game Inactive. Restarting... " + Mathf.Max(0, Mathf.FloorToInt(120f - CheatConsole.TimeSinceLastKeyPress)), Color.red);
			}
			else if (DisplayingInactiveWarning)
			{
				DisplayingInactiveWarning = false;
				CheatConsole.Instance.DisplayText("", Color.red);
			}
			if (CheatConsole.TimeSinceLastKeyPress >= 120f)
			{
				CheatConsole.ForceAutoAttractMode = true;
				DemoQuitToMenu();
				return;
			}
			if (!MMTransition.IsPlaying && (CheatConsole.DemoBeginTime += Time.unscaledDeltaTime) > 1200f)
			{
				Debug.Log("OUT OF TME!!");
				QuitToThanksForPlaying();
				return;
			}
		}
		if (InputManager.General.GetAnyButton() || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > 0.1f || Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > 0.1f)
		{
			autoPauseTimestamp = Time.unscaledTime + 600f;
		}
		else if (Time.unscaledTime > autoPauseTimestamp && MonoSingleton<UIManager>.Instance != null && !MonoSingleton<UIManager>.Instance.MenusBlocked)
		{
			MonoSingleton<UIManager>.Instance.ShowPauseMenu();
			autoPauseTimestamp = Time.unscaledTime + 600f;
		}
	}

	public static void RecalculatePaths(bool immediate = false)
	{
		if (GetInstance().cGeneratePathfinding != null)
		{
			GetInstance().StopCoroutine(GetInstance().cGeneratePathfinding);
		}
		GetInstance().cGeneratePathfinding = GetInstance().StartCoroutine(GetInstance().GeneratePathfinding((!immediate) ? 1 : 0));
	}

	private IEnumerator GeneratePathfinding(float Delay)
	{
		while (true)
		{
			float num;
			Delay = (num = Delay - Time.deltaTime);
			if (!(num > 0f))
			{
				break;
			}
			yield return null;
		}
		if(AstarPath.active.data.gridGraph != null)
			AstarPath.active.Scan(AstarPath.active.data.gridGraph);
		MMTransition.ResumePlay();
	}

	public void ShowGameOverScreen()
	{
		GameOverScreen.SetActive(true);
		SetTimeScale(0.1f);
	}

	public static void SetTimeScale(float NewTimeScale)
	{
		TimeScale = NewTimeScale;
		Time.timeScale = TimeScale;
	}

	public void QuitToMenu()
	{
		SceneManager.LoadScene("Main Menu");
	}

	private void DemoQuitToMenu()
	{
		if (CheatConsole.DemoBeginTime != 0f)
		{
			CheatConsole.DemoBeginTime = 0f;
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			UIMenuBase.ActiveMenus.Clear();
			SetTimeScale(1f);
			UIManager.PlayAudio("event:/sermon/select_upgrade");
			UIManager.PlayAudio("event:/sermon/Sermon Speech Bubble2");
			MMTransition.StopCurrentTransition();
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 3f, "", delegate
			{
			});
		}
	}

	public void QuitToThanksForPlaying()
	{
		if (CheatConsole.DemoBeginTime != 0f)
		{
			CheatConsole.DemoBeginTime = 0f;
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			UIMenuBase.ActiveMenus.Clear();
			SetTimeScale(1f);
			UIManager.PlayAudio("event:/sermon/select_upgrade");
			UIManager.PlayAudio("event:/sermon/select_upgrade");
			MMTransition.StopCurrentTransition();
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "DemoOver", 3f, "", delegate
			{
			});
		}
	}

	public static void NewRun(string PlaceName, bool InDungeon, FollowerLocation location = FollowerLocation.None)
	{
		Debug.Log(">>>>>>>>>>>>>>>   NewRun ".Colour(Color.yellow));
		InitialDungeonEnter = true;
		CurrentDungeonFloor = 1;
		if (!InDungeon)
		{
			DungeonEndlessLevel = 1;
		}
		BiomeGenerator.UsedEncounters = new List<string>();
		TimeManager.SetOverrideScheduledActivity(ScheduledActivity.None);
		MiniBossManager.CurrentIndex = 0;
		DungeonNameTerm = PlaceName;
		DataManager.SetNewRun(location);
		TimeInDungeon = DataManager.Instance.TimeInGame;
		if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ability_Resurrection))
		{
			ResurrectOnHud.ResurrectionType = ResurrectionType.Pyre;
		}
		Health.team2.Clear();
		try
		{
			instance.OnStopTimeInCrusadeSettingChanged(SettingsManager.Settings.Accessibility.StopTimeInCrusade);
		}
		catch
		{
		}
	}

	private void OnStopTimeInCrusadeSettingChanged(bool state)
	{
		if (IsDungeon(PlayerFarming.Location))
		{
			if (state)
			{
				SimulationManager.Pause();
			}
			else
			{
				SimulationManager.UnPause();
			}
		}
	}

	public static void NextDungeonLayer(int NewLayer)
	{
		DataManager.RandomSeed = new System.Random(DataManager.RandomSeed.Next(int.MinValue, int.MaxValue));
		PreviousDungeonLayer = CurrentDungeonLayer;
		CurrentDungeonLayer = NewLayer;
	}

	public static void ToShip(string Scene = "Base Biome 1", float Duration = 2f, MMTransition.Effect Effect = MMTransition.Effect.BlackFade)
	{
		SimulationManager.Pause();
		DataManager.ResetRunData();
		SaveAndLoad.Save();
		MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, Effect, Scene, Duration, "", ToShipCallback);
	}

	private static void ToShipCallback()
	{
		SetTimeScale(1f);
	}

	public static void healPlayer()
	{
		DataManager.Instance.PLAYER_HEALTH = DataManager.Instance.PLAYER_TOTAL_HEALTH;
		HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
		if (component != null)
		{
			component.HP += DataManager.Instance.PLAYER_HEALTH;
		}
	}

	public void EndCurrentDay(bool PlayerDied)
	{
		if (EndOfDayScreen != null)
		{
			GameObject gameObject = GameObject.FindWithTag("Player");
			if (gameObject != null)
			{
				SaveAndLoad.Save();
				UnityEngine.Object.Destroy(gameObject);
				SceneManager.LoadScene("Ship");
			}
		}
	}

	public static bool HasUnlockAvailable()
	{
		if (GetInstance() == null)
		{
			return true;
		}
		return GetInstance().UpgradeTreeConfiguration.HasUnlockAvailable();
	}

	public void CreateName()
	{
		Debug.Log(Villager_Info.GenerateName());
	}

	public float UnscaledTimeSince(float timestamp)
	{
		return Time.unscaledTime - timestamp;
	}

	public float TimeSince(float timestamp)
	{
		return scaledTimeElapsed - timestamp;
	}

	public void WaitForSeconds(float seconds, Action callback)
	{
		StartCoroutine(WaitForSecondsIE(seconds, false, callback));
	}

	public void WaitForSecondsRealtime(float seconds, Action callback)
	{
		StartCoroutine(WaitForSecondsIE(seconds, true, callback));
	}

	public IEnumerator WaitForSecondsIE(float seconds, bool realtime, Action callback)
	{
		if (seconds == 0f)
		{
			yield return new WaitForEndOfFrame();
		}
		else if (realtime)
		{
			yield return new WaitForSecondsRealtime(seconds);
		}
		else
		{
			yield return new WaitForSeconds(seconds);
		}
		if (callback != null)
		{
			callback();
		}
	}

	[CompilerGenerated]
	internal static IEnumerator _003CTwitchAuthentication_OnAuthenticated_003Eg__Wait_007C29_0()
	{
		while (MonoSingleton<UIManager>.Instance == null || MonoSingleton<UIManager>.Instance.MenusBlocked)
		{
			yield return null;
		}
		bool loop = true;
		if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Twitch))
		{
			UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Twitch);
			uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
			{
				loop = false;
			});
		}
		else
		{
			loop = false;
		}
		while (loop)
		{
			yield return null;
		}
		loop = true;
		if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Twitch_2))
		{
			UITutorialOverlayController uITutorialOverlayController2 = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Twitch_2);
			uITutorialOverlayController2.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController2.OnHidden, (Action)delegate
			{
				loop = false;
			});
		}
		else
		{
			loop = false;
		}
		while (loop)
		{
			yield return null;
		}
	}
}
