using System;
using System.Collections.Generic;
using System.Linq;
using Lamb.UI;
using Map;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using Sirenix.Utilities;
using UnityEngine;

public class DungeonSandboxManager : BaseMonoBehaviour
{
	public enum ScenarioType
	{
		DungeonMode,
		BossRushMode
	}

	[Flags]
	public enum ScenarioModifier
	{
		None = 0,
		HarderEnemies = 1,
		NoHealthDrops = 2,
		NoSpecialAttacks = 4
	}

	[Serializable]
	private struct Dungeon
	{
		public FollowerLocation Location;

		public GeneraterDecorations[] Decorations;

		public IslandPiece[] IslandPieces;

		public string[] MiniBossRooms;

		public string LeaderRooms;

		public string EntranceRoom;

		public string TempleDoorRoom;

		[Space]
		public BiomeLightingSettings AfternoonSettings;

		public BiomeLightingSettings NightSettings;

		public BiomeLightingSettings BossSettings;

		[Space]
		public string BiomeMusicPath;

		public string BiomeAtmosPath;
	}

	[Serializable]
	public class ProgressionSnapshot
	{
		public ScenarioType ScenarioType;

		public PlayerFleeceManager.FleeceType FleeceType;

		public int CompletedRuns;

		public int CompletionSeen;
	}

	public static DungeonSandboxManager Instance;

	private static ScenarioData[] scenarioData;

	private const string ScenarioPath = "Data/Scenario Data";

	private ScenarioData currentScenario;

	[SerializeField]
	private Dungeon[] dungeons;

	[SerializeField]
	private ScenarioData DEBUG_ScenarioData;

	private BiomeGenerator biomeGenerator;

	private GenerateRoom roomGenerator;

	public static ScenarioData CurrentScenario;

	public static int CurrentFleece;

	private int cachedFleece;

	public List<string> EncounteredMiniBosses = new List<string>();

	public static bool Active
	{
		get
		{
			return Instance != null;
		}
	}

	public static ScenarioData[] ScenarioData
	{
		get
		{
			if (scenarioData == null)
			{
				scenarioData = Resources.LoadAll<ScenarioData>("Data/Scenario Data");
			}
			return scenarioData;
		}
	}

	public ScenarioType CurrentScenarioType
	{
		get
		{
			return currentScenario.ScenarioType;
		}
	}

	public List<FollowerLocation> BossesCompleted { get; private set; } = new List<FollowerLocation>();


	private void Awake()
	{
		Instance = this;
		biomeGenerator = GetComponentInChildren<BiomeGenerator>();
		roomGenerator = GetComponentInChildren<GenerateRoom>();
		DataManager.Instance.SandboxModeEnabled = true;
		GameManager.CurrentDungeonLayer = 4;
	}

	private void Start()
	{
		if (CurrentScenario != null)
		{
			LoadScenario(CurrentScenario);
		}
	}

	private void OnDestroy()
	{
		TimeManager.PauseGameTime = false;
		DataManager.Instance.PlayerFleece = cachedFleece;
		SimulationManager.UnPause();
		PlayerFarming.LastLocation = FollowerLocation.Endless;
		DataManager.Instance.SandboxModeEnabled = false;
	}

	private void Update()
	{
		SimulationManager.Pause();
	}

	public void LoadScenario(ScenarioData data)
	{
		currentScenario = data;
		cachedFleece = DataManager.Instance.PlayerFleece;
		DataManager.Instance.PlayerFleece = CurrentFleece;
		MapManager.Instance.DungeonConfig = data.MapConfig;
		MMTransition.ResumePlay();
		UIAdventureMapOverlayController adventureMapOverlayController = MapManager.Instance.ShowMap();
		UIAdventureMapOverlayController uIAdventureMapOverlayController = adventureMapOverlayController;
		uIAdventureMapOverlayController.OnShow = (Action)Delegate.Combine(uIAdventureMapOverlayController.OnShow, (Action)delegate
		{
			adventureMapOverlayController.NodeFromPoint(adventureMapOverlayController.Map.path.LastElement()).gameObject.SetActive(false);
		});
		adventureMapOverlayController.Cancellable = false;
	}

	public void SetDungeonType(FollowerLocation location)
	{
		roomGenerator.StartPieces = GetIslandPieces(location).ToList();
		roomGenerator.DecorationSetList = GetDecorationList(location).ToList();
		BiomeGenerator.Instance.BiomeDecorationSet = GetDecorationList(location).ToList();
		PlayerFarming.Location = location;
		biomeGenerator.DungeonLocation = location;
		biomeGenerator.BossRoomPath = GetMiniBossRoom(location);
		biomeGenerator.LeaderRoomPath = GetLeaderRoom(location);
		biomeGenerator.EntranceRoomPath = GetEntranceRoom(location);
		biomeGenerator.BossDoorRoomPath = GetTempleDoorRoom(location);
		biomeGenerator.biomeMusicPath = GetBiomeMusicPath(location);
		biomeGenerator.biomeAtmosPath = GetBiomeAtmosPath(location);
		Dungeon dungeon = GetDungeon(location);
		LightingManager.Instance.dawnSettings = dungeon.AfternoonSettings;
		LightingManager.Instance.morningSettings = dungeon.AfternoonSettings;
		LightingManager.Instance.afternoonSettings = dungeon.AfternoonSettings;
		LightingManager.Instance.duskSettings = dungeon.AfternoonSettings;
		LightingManager.Instance.nightSettings = dungeon.NightSettings;
		LightingManager.Instance.PrepareLightingSettings();
		if (LocationManager._Instance != null)
		{
			((DungeonLocationManager)LocationManager._Instance)._location = location;
		}
		LocationManager.UpdateLocation();
	}

	public void UpdateDungeonType()
	{
		FollowerLocation followerLocation = FollowerLocation.Dungeon1_1;
		followerLocation += BossesCompleted.Count;
		if (BossesCompleted.Count >= 4)
		{
			followerLocation = FollowerLocation.Dungeon1_4;
		}
		SetDungeonType(followerLocation);
	}

	private Dungeon GetDungeon(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon result = array[i];
			if (result.Location == location)
			{
				return result;
			}
		}
		return dungeons[0];
	}

	public GeneraterDecorations[] GetDecorationList(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon dungeon = array[i];
			if (dungeon.Location == location)
			{
				return dungeon.Decorations;
			}
		}
		return new GeneraterDecorations[0];
	}

	public IslandPiece[] GetIslandPieces(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon dungeon = array[i];
			if (dungeon.Location == location)
			{
				return dungeon.IslandPieces;
			}
		}
		return new IslandPiece[0];
	}

	public string GetMiniBossRoom(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon dungeon = array[i];
			if (dungeon.Location == location)
			{
				return dungeon.MiniBossRooms[UnityEngine.Random.Range(0, dungeon.MiniBossRooms.Length)];
			}
		}
		return dungeons[0].MiniBossRooms[UnityEngine.Random.Range(0, dungeons[0].MiniBossRooms.Length)];
	}

	public string GetLeaderRoom(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon dungeon = array[i];
			if (dungeon.Location == location)
			{
				return dungeon.LeaderRooms;
			}
		}
		return dungeons[0].LeaderRooms;
	}

	public string GetEntranceRoom(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon dungeon = array[i];
			if (dungeon.Location == location)
			{
				return dungeon.EntranceRoom;
			}
		}
		return dungeons[0].EntranceRoom;
	}

	public string GetTempleDoorRoom(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon dungeon = array[i];
			if (dungeon.Location == location)
			{
				return dungeon.TempleDoorRoom;
			}
		}
		return dungeons[0].TempleDoorRoom;
	}

	public string GetBiomeMusicPath(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon dungeon = array[i];
			if (dungeon.Location == location)
			{
				return dungeon.BiomeMusicPath;
			}
		}
		return dungeons[0].BiomeMusicPath;
	}

	public string GetBiomeAtmosPath(FollowerLocation location)
	{
		Dungeon[] array = dungeons;
		for (int i = 0; i < array.Length; i++)
		{
			Dungeon dungeon = array[i];
			if (dungeon.Location == location)
			{
				return dungeon.BiomeAtmosPath;
			}
		}
		return dungeons[0].BiomeAtmosPath;
	}

	public static List<ScenarioData> GetDataForScenarioType(ScenarioType scenarioType)
	{
		List<ScenarioData> list = new List<ScenarioData>();
		ScenarioData[] array = ScenarioData;
		foreach (ScenarioData scenarioData in array)
		{
			if (scenarioData.ScenarioType == scenarioType)
			{
				list.Add(scenarioData);
			}
		}
		DungeonSandboxManager.scenarioData.Sort((ScenarioData x, ScenarioData y) => x.ScenarioIndex.CompareTo(y.ScenarioIndex));
		return list;
	}

	public static ProgressionSnapshot GetProgressionForScenario(ScenarioType scenarioType, PlayerFleeceManager.FleeceType fleeceType)
	{
		foreach (ProgressionSnapshot item in DataManager.Instance.SandboxProgression)
		{
			if (scenarioType == item.ScenarioType && fleeceType == item.FleeceType)
			{
				return item;
			}
		}
		ProgressionSnapshot progressionSnapshot = new ProgressionSnapshot
		{
			ScenarioType = scenarioType,
			FleeceType = fleeceType
		};
		DataManager.Instance.SandboxProgression.Add(progressionSnapshot);
		return progressionSnapshot;
	}

	public static int GetCompletedRunCount()
	{
		int num = 0;
		foreach (ProgressionSnapshot item in DataManager.Instance.SandboxProgression)
		{
			num += item.CompletedRuns;
		}
		return num;
	}

	public static bool HasFinishedAnyWithDefaultFleece()
	{
		foreach (ProgressionSnapshot item in DataManager.Instance.SandboxProgression)
		{
			if (item.FleeceType == PlayerFleeceManager.FleeceType.Default)
			{
				return item.CompletedRuns > 0;
			}
		}
		return false;
	}
}
