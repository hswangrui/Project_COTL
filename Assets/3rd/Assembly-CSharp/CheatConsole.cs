using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeStage.AdvancedFPSCounter;
using Lamb.UI;
using Lamb.UI.MainMenu;
using Map;
using MMBiomeGeneration;
using MMTools;
using src.UI.Menus;
using Unify;
using UnityEngine;
using UnityEngine.UI;

public class CheatConsole : BaseMonoBehaviour
{
	public enum Phase
	{
		OFF,
		ON,
		RESPONSE
	}

	public enum FaithTypes
	{
		OFF,
		DRIP
	}

	private static bool _inDemo = false;

	public const float DEMO_ATTRACT_MODE_TIMER = 20f;

	public const float DEMO_INACTIVE_TIMER = 120f;

	public const float DEMO_HOLD_TO_RESET_TIMER = 5f;

	public const float DEMO_MAX_TIMER = 1200f;

	public static float DemoBeginTime = 0f;

	public static bool WIREFRAME_ENABLED = false;

	public Text text;

	public Text backgroundText;

	public Text autoCompleteItemText;

	public static Phase CurrentPhase;

	private float Timer;

	public static FaithTypes FaithType = FaithTypes.DRIP;

	private List<Text> autoCompleteItems = new List<Text>();

	public static GameObject[] resourcesToSpawn;

	public static GameObject SubmitReportPrefab;

	public static bool ShowAllMapLocations = false;

	public static bool Robes = false;

	public static bool ForceSpiderMiniBoss = false;

	public static bool UglyWeeds = false;

	public static bool ForceSmoochEnabled = false;

	public static bool ForceBlessEnabled = false;

	public static bool QuickUnlock = false;

	public static bool BleatKillAll = false;

	private static bool playerHidden = false;

	public Dictionary<string, Action> Cheats = new Dictionary<string, Action>();

	public static CheatConsole Instance;

	public Color TextColor = Color.white;

	public static bool ForceAutoAttractMode = false;

	private static float LastKeyPressTime;

	public static bool BuildingsFree = false;

	public static bool AllBuildingsUnlocked = false;

	public static bool UnlockAllRituals = false;

	private static HideUI HideUIObject;

	public static bool HidingUI = false;

	public static Action OnHideUI;

	public static Action OnShowUI;

	public static bool IN_DEMO
	{
		get
		{
			return _inDemo;
		}
		set
		{
			_inDemo = value;
		}
	}

	public static float TimeSinceLastKeyPress
	{
		get
		{
			return Time.unscaledTime - LastKeyPressTime;
		}
	}



	public static void EnableDemo()
	{
		IN_DEMO = true;
		MainMenuController mainMenuController = UnityEngine.Object.FindObjectOfType<MainMenuController>();
		if (mainMenuController != null)
		{
			mainMenuController.AttractMode();
		}
		MainMenu mainMenu = UnityEngine.Object.FindObjectOfType<MainMenu>();
		if (mainMenu != null)
		{
			mainMenu.EnableDemo(false);
		}
		DataManager.Instance.CanReadMinds = true;
		DemoWatermark.Play();
	}

	private static IEnumerator CombatNodes()
	{
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		yield return adventureMapOverlayController.ConvertAllNodesToCombatNodes();
		MapManager.Instance.CloseMap();
		while (adventureMapOverlayController.IsHiding)
		{
			yield return null;
		}
	}

	private static IEnumerator BossNodes()
	{
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		yield return adventureMapOverlayController.ConvertMiniBossNodeToBossNode();
		MapManager.Instance.CloseMap();
		while (adventureMapOverlayController.IsHiding)
		{
			yield return null;
		}
	}

	private static IEnumerator RandomizeNodes()
	{
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		yield return adventureMapOverlayController.RandomiseNextNodes();
		MapManager.Instance.CloseMap();
		while (adventureMapOverlayController.IsHiding)
		{
			yield return null;
		}
	}

	private static IEnumerator TeleportNode()
	{
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		Node randomNodeOnLayer = MapGenerator.GetRandomNodeOnLayer(MapManager.Instance.CurrentLayer + 2);
		if (randomNodeOnLayer != null)
		{
			yield return adventureMapOverlayController.TeleportNode(randomNodeOnLayer);
		}
	}

	private static void SoakTest()
	{
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_HeartsOfTheFaithful6);
		DataManager.Instance.PLAYER_HEARTS_LEVEL = 5;
		UnifyComponent.Instance.gameObject.AddComponent<CheatChainRunner>();
		UnifyComponent.Instance.GetComponent<CheatChainRunner>().RunChainForEver(new string[196]
		{
			"SETDUNGEON1LAYER1", "LOADD1", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM", "NEXTROOM",
			"BOSSROOM", "BASE", "SETDUNGEON1LAYER2", "LOADD1", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD",
			"NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "SETDUNGEON1LAYER3", "LOADD1", "GOD", "NEXTROOM", "NEXTROOM", "N",
			"NODELAST", "GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "SETDUNGEON1LAYER4", "LOADD1", "GOD", "NEXTROOM",
			"NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "FIXFOLLOWERS", "SETDUNGEON2LAYER1",
			"LOADD2", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM",
			"BASE", "SETDUNGEON2LAYER2", "LOADD2", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM",
			"NEXTROOM", "BOSSROOM", "BASE", "SETDUNGEON2LAYER3", "LOADD2", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST",
			"GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "SETDUNGEON2LAYER4", "LOADD2", "GOD", "NEXTROOM", "NEXTROOM",
			"N", "NODELAST", "GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "FIXFOLLOWERS", "SETDUNGEON3LAYER1", "LOADD3",
			"GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE",
			"SETDUNGEON3LAYER2", "LOADD3", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM", "NEXTROOM",
			"BOSSROOM", "BASE", "SETDUNGEON3LAYER3", "LOADD3", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD",
			"NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "SETDUNGEON3LAYER4", "LOADD3", "GOD", "NEXTROOM", "NEXTROOM", "N",
			"NODELAST", "GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "FIXFOLLOWERS", "SETDUNGEON4LAYER1", "LOADD4", "GOD",
			"NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "SETDUNGEON4LAYER2",
			"LOADD4", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM",
			"BASE", "SETDUNGEON4LAYER3", "LOADD4", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST", "GOD", "NEXTROOM",
			"NEXTROOM", "BOSSROOM", "BASE", "SETDUNGEON4LAYER4", "LOADD4", "GOD", "NEXTROOM", "NEXTROOM", "N", "NODELAST",
			"GOD", "NEXTROOM", "NEXTROOM", "BOSSROOM", "BASE", "FIXFOLLOWERS"
		}, 60f);
	}

	private static void BossRoom(int layer, int Dungeon)
	{
		UnifyComponent.Instance.gameObject.AddComponent<CheatChainRunner>();
		UnifyComponent.Instance.GetComponent<CheatChainRunner>().RunChain(new string[5]
		{
			"SETDUNGEON" + Dungeon + "LAYER" + layer,
			"LOADD" + Dungeon,
			"N",
			"NODELAST",
			"BOSSROOM"
		}, new float[5] { 5f, 5f, 5f, 5f, 5f });
	}

	private static void SkipBossRoom()
	{
		UnifyComponent.Instance.gameObject.AddComponent<CheatChainRunner>();
		UnifyComponent.Instance.GetComponent<CheatChainRunner>().RunChain(new string[3] { "N", "NODELAST", "BOSSROOM" }, new float[5] { 5f, 5f, 5f, 5f, 5f });
	}

	private static void SubmitReport()
	{
		GameManager.GetInstance();
	}

	private static void LoadGame(int SaveSlot)
	{
		SaveAndLoad.SAVE_SLOT = SaveSlot;
		MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Base Biome 1", 3f, "", delegate
		{
			AudioManager.Instance.StopCurrentMusic();
			SaveAndLoad.Load(SaveAndLoad.SAVE_SLOT);
		});
	}

	private void OnEnable()
	{
		Instance = this;
		text.text = "";
		Cheats = Cheats.OrderBy((KeyValuePair<string, Action> x) => x.Key).ToDictionary((KeyValuePair<string, Action> x) => x.Key, (KeyValuePair<string, Action> x) => x.Value);
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void PollForKeyPresses()
	{
		if (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f || InputManager.General.GetAnyButton() || Mathf.Abs(InputManager.Gameplay.GetHorizontalAxis()) > 0.2f || Mathf.Abs(InputManager.Gameplay.GetVerticalAxis()) > 0.2f || LetterBox.IsPlaying || MMConversation.isPlaying || MMTransition.IsPlaying)
		{
			LastKeyPressTime = Time.unscaledTime;
		}
	}

	public static void ForceResetTimeSinceLastKeyPress()
	{
		LastKeyPressTime = Time.unscaledTime;
	}

	public void DisplayText(string Message, Color color)
	{
		text.text = Message;
		text.color = color;
	}

	private void OnGUI()
	{
		PollForKeyPresses();
	}

	private void UpdateAutoComplete()
	{
		for (int num = autoCompleteItems.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(autoCompleteItems[num].gameObject);
		}
		autoCompleteItems.Clear();
		if (backgroundText != null)
		{
			backgroundText.text = "";
		}
		if (!(this.text.text != ""))
		{
			return;
		}
		Cheats = Cheats.OrderBy((KeyValuePair<string, Action> x) => x.Key).ToDictionary((KeyValuePair<string, Action> x) => x.Key, (KeyValuePair<string, Action> x) => x.Value);
		foreach (KeyValuePair<string, Action> cheat in Cheats)
		{
			if (cheat.Key.StartsWith(this.text.text))
			{
				if (backgroundText != null)
				{
					backgroundText.text = cheat.Key;
				}
				Text text = UnityEngine.Object.Instantiate(autoCompleteItemText, autoCompleteItemText.transform.parent);
				text.gameObject.SetActive(true);
				text.text = cheat.Key;
				autoCompleteItems.Add(text);
			}
		}
	}

	private void Start()
	{
		AFPSCounter.AddToScene();
		AFPSCounter.Instance.enabled = false;

		
		//¹Ø±ÕPixelator
		GameObject go = GameObject.Find("Pixelator");
		if (go)
		{
			go.SetActive(false);
		}
		GameObject gameObject = GameObject.FindGameObjectWithTag("Canvas");
		if (gameObject != null)
		{
			base.transform.parent = gameObject.transform;
			base.transform.SetAsLastSibling();
		}

		//CheatConsole.UnlockAll();

		
		//CheatConsole.AllBuildingsUnlocked=true;
		//CheatConsole.ToggleDemiGodMode();
		//CheatConsole.BuildingsFree = true;
		//CheatConsole._inDemo = true;


	}

	private void Cancel()
	{
		text.text = "Invalid Cheat.";
		backgroundText.text = "";
		text.color = Color.red;
		Timer = 0f;
		CurrentPhase = Phase.RESPONSE;
	}

	private void CheatAccepted()
	{
		text.text = "Cheat Accepted!";
		text.color = Color.green;
		backgroundText.text = "";
		Timer = 0f;
		CurrentPhase = Phase.RESPONSE;
	}

	private static void AllBuildingsFree()
	{
		BuildingsFree = true;
	}

	private static void FPS()
	{
		if (!AFPSCounter.Instance.enabled)
		{
			AFPSCounter.Instance.enabled = true;
		}
		else
		{
			AFPSCounter.Instance.enabled = false;
		}
	}

	private static void SkipHour()
	{
		TimeManager.CurrentGameTime += 240f;
	}

	private static void FollowerDebug()
	{
		SimulationManager.ShowFollowerDebugInfo = !SimulationManager.ShowFollowerDebugInfo;
		SimulationManager.ShowStructureDebugInfo = false;
	}

	private static void StructureDebug()
	{
		SimulationManager.ShowFollowerDebugInfo = false;
		SimulationManager.ShowStructureDebugInfo = !SimulationManager.ShowStructureDebugInfo;
	}

	private static void Heal()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<Health>().Heal(2f);
		}
	}

	private static void AddHeart(int amount = 2)
	{
		HUD_Manager.Instance.Show(0);
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<HealthPlayer>().totalHP += amount;
		}
	}

	private static void Damage()
	{
		HUD_Manager.Instance.Show(0);
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<Health>().DealDamage(1f, gameObject, gameObject.transform.position);
		}
	}

	private static void Damage5()
	{
		HUD_Manager.Instance.Show(0);
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<Health>().DealDamage(5f, gameObject, gameObject.transform.position);
		}
	}

	private static void Die()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<Health>().DealDamage(9999f, gameObject, gameObject.transform.position);
		}
	}

	private static void Die2()
	{
		DataManager.Instance.CurrentChallengeModeXP = 30;
		DataManager.Instance.CurrentChallengeModeLevel = 1;
		Inventory.AddItem(128, 400);
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<Health>().DealDamage(9999f, gameObject, gameObject.transform.position);
		}
	}

	private static void Die3()
	{
		DataManager.Instance.CurrentChallengeModeXP = 30;
		DataManager.Instance.CurrentChallengeModeLevel = 1;
		Inventory.AddItem(119, 1);
		Inventory.AddItem(128, 200);
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<Health>().DealDamage(9999f, gameObject, gameObject.transform.position);
		}
	}

	private static void MoreHearts()
	{
		DataManager.Instance.PLAYER_TOTAL_HEALTH = 10f;
		DataManager.Instance.PLAYER_HEALTH = DataManager.Instance.PLAYER_TOTAL_HEALTH;
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<Health>().DealDamage(0f, gameObject, gameObject.transform.position);
		}
	}

	private static void NextSandboxLayer()
	{
		DungeonSandboxManager.Instance.SetDungeonType(FollowerLocation.Dungeon1_4);
		MapManager.Instance.MapGenerated = false;
		UIAdventureMapOverlayController uIAdventureMapOverlayController = MapManager.Instance.ShowMap(true);
		MapManager.Instance.StartCoroutine(uIAdventureMapOverlayController.NextSandboxLayer());
	}

	private static void BlueHearts()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<HealthPlayer>().BlueHearts += 2f;
		}
	}

	private static void SpiritHeartsFull()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<HealthPlayer>().TotalSpiritHearts += 2f;
		}
	}

	private static void Rituals()
	{
		UpgradeSystem.ClearAllCoolDowns();
		UnlockAllRituals = true;
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Ascend);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Brainwashing);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Enlightenment);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Fast);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Feast);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Fightpit);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Funeral);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Holiday);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Ressurect);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Sacrifice);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Wedding);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_ConsumeFollower);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_DonationRitual);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FasterBuilding);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FirePit);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_FishingRitual);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_HarvestRitual);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AlmsToPoor);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AssignFaithEnforcer);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_AssignTaxCollector);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_WorkThroughNight);
		GiveResources();
		GiveResources();
		GiveResources();
	}

	private static void SpiritHeartsHalf()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			gameObject.GetComponent<HealthPlayer>().TotalSpiritHearts += 1f;
		}
	}

	private static void MoreSouls()
	{
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.GetSoul(100);
		}
	}

	private static void MoreBlackSouls()
	{
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.GetBlackSoul(200);
		}
	}

	private static void MoreArrows()
	{
		if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Combat_Arrows))
		{
			UnityEngine.Object.FindObjectOfType<HUD_Ammo>().Play();
			UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Combat_Arrows);
		}
		PlayerArrows playerArrows = UnityEngine.Object.FindObjectOfType<PlayerArrows>();
		if (playerArrows != null)
		{
			playerArrows.RestockAllArrows();
		}
	}

	private static void HideUI()
	{
		Action onHideUI = OnHideUI;
		if (onHideUI != null)
		{
			onHideUI();
		}
		MMTransition.OnTransitionCompelte = (Action)Delegate.Remove(MMTransition.OnTransitionCompelte, new Action(HideUI));
		if (HideUIObject == null)
		{
			HideUIObject = new GameObject
			{
				name = "Hide UI"
			}.AddComponent<HideUI>();
			HidingUI = true;
		}
	}

	private static void ShowUI()
	{
		Action onShowUI = OnShowUI;
		if (onShowUI != null)
		{
			onShowUI();
		}
		if (HideUIObject != null)
		{
			HideUIObject.ShowUI();
			HideUIObject = null;
			HidingUI = false;
		}
	}

	private static void TurnOffResourceHighlight()
	{
		Shader.SetGlobalInt("_GlobalResourceHighlight", 0);
	}

	private static void GiveResources()
	{
		GameObject.FindWithTag("Player");
		Inventory.AddItem(1, 100);
		Inventory.AddItem(2, 100);
		Inventory.AddItem(35, 100);
		Inventory.AddItem(20, 100);
		Inventory.AddItem(9, 100);
		Inventory.AddItem(83, 100);
		Inventory.AddItem(86, 100);
		Inventory.AddItem(81, 100);
		Inventory.AddItem(82, 100);
		Inventory.AddItem(55, 100);
		Inventory.AddItem(89, 100);
		Inventory.AddItem(90, 100);
		Inventory.AddItem(117, 3);
	}

	private static void GivePoop()
	{
		Inventory.AddItem(39, 100);
	}

	private static void GiveStartingPack()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		for (int i = 0; i < 30; i++)
		{
			UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/Log"), gameObject.transform.position, Quaternion.identity);
			UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/BlackGold"), gameObject.transform.position, Quaternion.identity);
			UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/Grass"), gameObject.transform.position, Quaternion.identity);
		}
	}

	private static void GiveKeys()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		for (int i = 0; i < 3; i++)
		{
			UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/Key Piece"), gameObject.transform.position, Quaternion.identity);
		}
	}

	private static void GiveFood()
	{
		Inventory.AddItem(21, 10);
		Inventory.AddItem(105, 10);
		Inventory.AddItem(6, 10);
		Inventory.AddItem(50, 10);
		Inventory.AddItem(97, 10);
		Inventory.AddItem(102, 10);
		Inventory.AddItem(62, 5);
	}

	private static void Fish()
	{
		Inventory.AddItem(28, 5);
		Inventory.AddItem(34, 5);
		Inventory.AddItem(33, 5);
		Inventory.AddItem(94, 5);
		Inventory.AddItem(91, 5);
		Inventory.AddItem(92, 5);
		Inventory.AddItem(93, 5);
		Inventory.AddItem(96, 5);
		Inventory.AddItem(95, 5);
	}

	private static void MonsterHeart()
	{
		Inventory.AddItem(22, 5);
	}

	private static void BuildAll()
	{
		foreach (Structures_BuildSite item in StructureManager.GetAllStructuresOfType<Structures_BuildSite>())
		{
			item.BuildProgress = StructuresData.BuildDurationGameMinutes(item.Data.ToBuildType);
		}
		foreach (Structures_BuildSiteProject item2 in StructureManager.GetAllStructuresOfType<Structures_BuildSiteProject>())
		{
			item2.BuildProgress = StructuresData.BuildDurationGameMinutes(item2.Data.ToBuildType);
		}
	}

	private static void Mushroom()
	{
		Inventory.AddItem(29, 100);
	}

	private static void ReturnToBase()
	{
		GameManager.ToShip();
	}

	private static void DebugInfo()
	{
		Debug.Log("DataManager.Instance.BlueprintsChest.Count " + DataManager.Instance.PlayerBluePrints.Count);
		Debug.Log("DataManager.Instance.Blueprints.Count " + DataManager.Instance.PlayerBluePrints.Count);
	}

	private static void AllCurses()
	{
	}

	private static void AllTrinkets()
	{
		DataManager.Instance.PlayerFoundTrinkets.Clear();
		foreach (TarotCards.Card allTrinket in DataManager.AllTrinkets)
		{
			DataManager.Instance.PlayerFoundTrinkets.Add(allTrinket);
		}
	}

	private static void ImplementedTrinkets()
	{
	}

	private static void ToggleCollider()
	{
		if (PlayerFarming.Instance.circleCollider2D != null)
		{
			PlayerFarming.Instance.circleCollider2D.enabled = !PlayerFarming.Instance.circleCollider2D.enabled;
		}
	}

	private static void Left()
	{
		BiomeGenerator.ChangeRoom(new Vector2Int(-1, 0));
	}

	private static void Right()
	{
		BiomeGenerator.ChangeRoom(new Vector2Int(1, 0));
	}

	private static void Up()
	{
		BiomeGenerator.ChangeRoom(new Vector2Int(0, 1));
	}

	private static void Down()
	{
		BiomeGenerator.ChangeRoom(new Vector2Int(0, -1));
	}

	private static void ShowMap()
	{
		MiniMap miniMap = UnityEngine.Object.FindObjectOfType<MiniMap>();
		if ((object)miniMap != null)
		{
			miniMap.VisitAll();
		}
	}

	private static void UnlockCrownAbility(CrownAbilities.TYPE Type)
	{
		CrownAbilities.UnlockAbility(Type);
	}

	private static void FollowerToken()
	{
		Inventory.FollowerTokens++;
	}

	private static void FollowerTokens()
	{
		Inventory.FollowerTokens += 100;
	}

	private static void TestObjectives()
	{
		CompleteCurrentObjectives();
		Quests.IsDebug = true;
		DataManager.Instance.TimeSinceLastQuest = float.MaxValue;
	}

	private static void NameCult()
	{
		DataManager.Instance.OnboardedCultName = false;
	}

	private static void CustomObjective_Create()
	{
		ObjectiveManager.Add(new Objectives_BuildStructure("Objectives/GroupTitles/Quest", StructureBrain.TYPES.DECORATION_BONE_CANDLE, 1, 3600f));
	}

	private static void CustomObjective_Complete()
	{
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.Test);
	}

	private static void ResetCooldowns()
	{
		for (int i = 0; i < DataManager.Instance.LastUsedSermonRitualDayIndex.Length; i++)
		{
			DataManager.Instance.LastUsedSermonRitualDayIndex[i] = -1;
		}
	}

	private static void KillFollowers()
	{
		for (int num = DataManager.Instance.Followers.Count - 1; num >= 0; num--)
		{
			FollowerBrain.FindBrainByID(DataManager.Instance.Followers[num].ID).HardSwapToTask(new FollowerTask_FindPlaceToDie(NotificationCentre.NotificationType.Died));
		}
	}

	private static void RemoveFaith()
	{
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			follower.Faith -= 10f;
		}
	}

	private static void UnlockAllSermons()
	{
		for (int i = 0; i < 23; i++)
		{
			SermonsAndRituals.SermonRitualType sermonRitualType = (SermonsAndRituals.SermonRitualType)i;
			if (sermonRitualType != 0 && !DataManager.Instance.UnlockedSermonsAndRituals.Contains(sermonRitualType))
			{
				DataManager.Instance.UnlockedSermonsAndRituals.Add(sermonRitualType);
			}
		}
	}

	private static IEnumerator ResetMap()
	{
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap(true);
		while (adventureMapOverlayController.IsShowing)
		{
			yield return null;
		}
		yield return adventureMapOverlayController.RegenerateMapRoutine();
	}

	private static void UnlockAll()
	{
		UnlockAllRituals = true;
		for (int i = 0; i < Enum.GetNames(typeof(UpgradeSystem.Type)).Length; i++)
		{
			UpgradeSystem.UnlockAbility((UpgradeSystem.Type)i);
		}
		GameManager.GetInstance().StartCoroutine(UpgradeSystem.ListOfUnlocksRoutine());
	}

	private static void UnlockWeapons()
	{
		DataManager.Instance.AddWeapon(EquipmentType.Axe);
		DataManager.Instance.AddWeapon(EquipmentType.Dagger);
		DataManager.Instance.AddWeapon(EquipmentType.Gauntlet);
		DataManager.Instance.AddWeapon(EquipmentType.Hammer);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack1);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack2);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack3);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack4);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_CursePack5);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponFervor);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponGodly);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponNecromancy);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponPoison);
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.PUpgrade_WeaponCritHit);
		GameManager.GetInstance().StartCoroutine(UpgradeSystem.ListOfUnlocksRoutine());
	}

	private static void RunTrinket(TarotCards.TarotCard card)
	{
		TrinketManager.AddTrinket(card);
	}

	private static void RunTrinket(TarotCards.Card card)
	{
		TrinketManager.AddTrinket(new TarotCards.TarotCard(card, 0));
	}

	private static void EnableTarot()
	{
		DataManager.Instance.HasTarotBuilding = true;
	}

	private static void EnableBlackSouls()
	{
		DataManager.Instance.BlackSoulsEnabled = true;
		UnityEngine.Object.FindObjectOfType<HUD_BlackSoul>().RingsObject.gameObject.SetActive(true);
	}

	private static void SetResolution()
	{
		Screen.SetResolution(1920, 1080, true);
	}

	private static void ToggleFullScreen()
	{
		Screen.fullScreen = !Screen.fullScreen;
	}

	public static void SkipToPhase(DayPhase phase)
	{
		StateMachine.State prevState = PlayerFarming.Instance.state.CURRENT_STATE;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(GameObject.FindWithTag("Player Camera Bone"), 3f);
		SimulationManager.SkipToPhase(phase, delegate
		{
			PlayerFarming.Instance.state.CURRENT_STATE = prevState;
			GameManager.GetInstance().OnConversationEnd();
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", MMTransition.ResumePlay);
		});
	}

	private static void ClearRubble()
	{
		List<StructureBrain> list = new List<StructureBrain>(StructureManager.StructuresAtLocation(FollowerLocation.Base));
		for (int i = 0; i < list.Count; i++)
		{
			StructureBrain structureBrain = list[i];
			if (structureBrain.Data.Type == StructureBrain.TYPES.RUBBLE || structureBrain.Data.Type == StructureBrain.TYPES.RUBBLE_BIG)
			{
				(structureBrain as Structures_Rubble).Remove();
			}
		}
	}

	private static void ClearWeed()
	{
		List<StructureBrain> list = new List<StructureBrain>(StructureManager.StructuresAtLocation(FollowerLocation.Base));
		for (int i = 0; i < list.Count; i++)
		{
			StructureBrain structureBrain = list[i];
			if (structureBrain.Data.Type == StructureBrain.TYPES.WEEDS)
			{
				(structureBrain as Structures_Weeds).Remove();
			}
		}
	}

	private static void UnlockAllStructures()
	{
		foreach (StructureBrain.TYPES value in Enum.GetValues(typeof(StructureBrain.TYPES)))
		{
			if (!StructuresData.GetUnlocked(value))
			{
				DataManager.Instance.UnlockedStructures.Add(value);
			}
		}
	}

	private static void CompleteCurrentObjectives()
	{
		foreach (ObjectivesData objective in DataManager.Instance.Objectives)
		{
			objective.IsComplete = true;
			objective.IsFailed = false;
			DataManager.Instance.AddToCompletedQuestHistory(objective.GetFinalizedData());
		}
		ObjectiveManager.CheckObjectives();
		foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
		{
			completedObjective.IsComplete = true;
			completedObjective.IsFailed = false;
		}
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.CurrentPlayerQuest != null)
			{
				follower.CurrentPlayerQuest.IsComplete = true;
			}
		}
		DataManager.Instance.Objectives.Clear();
	}

	private static void ToggleGodMode()
	{
		if (PlayerFarming.Instance.health.GodMode == Health.CheatMode.God)
		{
			PlayerFarming.Instance.health.GodMode = Health.CheatMode.None;
		}
		else
		{
			PlayerFarming.Instance.health.GodMode = Health.CheatMode.God;
		}
	}

	private static void ToggleDemiGodMode()
	{
		if (PlayerFarming.Instance.health.GodMode == Health.CheatMode.Demigod)
		{
			PlayerFarming.Instance.health.GodMode = Health.CheatMode.None;
		}
		else
		{
			PlayerFarming.Instance.health.GodMode = Health.CheatMode.Demigod;
		}
	}

	private static void ToggleImmortalMode()
	{
		if (PlayerFarming.Instance.health.GodMode == Health.CheatMode.Immortal)
		{
			PlayerFarming.Instance.health.GodMode = Health.CheatMode.None;
		}
		else
		{
			PlayerFarming.Instance.health.GodMode = Health.CheatMode.Immortal;
		}
	}

	private static void ToggleNoClip()
	{
		Collider2D obj = ((PlayerFarming.Instance != null) ? PlayerFarming.Instance.GetComponent<Collider2D>() : PlayerPrisonerController.Instance.GetComponent<Collider2D>());
		obj.isTrigger = !obj.isTrigger;
	}

	private static void CreateFollower(int Num)
	{
		foreach (WorshipperData.SkinAndData character in WorshipperData.Instance.Characters)
		{
			if (!DataManager.GetFollowerSkinUnlocked(character.Skin[0].Skin) && !DataManager.OnBlackList(character.Skin[0].Skin))
			{
				DataManager.SetFollowerSkinUnlocked(character.Skin[0].Skin);
			}
		}
		while (--Num >= 0)
		{
			Follower follower = FollowerManager.CreateNewFollower(PlayerFarming.Location, PlayerFarming.Instance.transform.position);
			if (UnityEngine.Random.value < 0.5f)
			{
				follower.Brain.Info.FollowerRole = FollowerRole.Worshipper;
				follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
				follower.SetOutfit(FollowerOutfitType.Follower, false);
				follower.Brain.CheckChangeTask();
			}
			else
			{
				follower.Brain.Info.FollowerRole = FollowerRole.Worker;
				follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
				follower.SetOutfit(FollowerOutfitType.Follower, false);
				follower.Brain.Info.WorkerPriority = WorkerPriority.None;
				follower.Brain.Stats.WorkerBeenGivenOrders = true;
				follower.Brain.CheckChangeTask();
			}
		}
	}

	private static void EndKnucklebones()
	{
		UnityEngine.Object.FindObjectOfType<KBGameScreen>().ForceEndGame();
	}

	private static void KillAllEnemiesInRoom()
	{
		foreach (Health item in new List<Health>(Health.team2))
		{
			if (item != null)
			{
				item.invincible = false;
				item.enabled = true;
				item.DealDamage(float.PositiveInfinity, PlayerFarming.Instance.gameObject, Vector3.zero, false, Health.AttackTypes.Projectile);
			}
		}
	}
}
