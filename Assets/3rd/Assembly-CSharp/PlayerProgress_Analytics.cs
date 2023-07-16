using System.Collections.Generic;
using Lamb.UI;
using UnityEngine;

public class PlayerProgress_Analytics : MonoSingleton<PlayerProgress_Analytics>
{
	public List<InventoryItem.ITEM_TYPE> ItemsToCheck;

	public List<InventoryItem> itemsDungeon = new List<InventoryItem>();

	public const string _Name = "player_progress";

	public Dictionary<string, object> parameters = new Dictionary<string, object>();

	public int DeathsInDungeon;

	public int dungeonRun;

	public int FollowerCount;

	public float TimeInDungeon;

	public float TimeInGame;

	private float updateInterval = 0.5f;

	private float lastInterval;

	private int frames;

	private float fps;

	public override void Start()
	{
		for (int i = 0; i < ItemsToCheck.Count; i++)
		{
			parameters.Add(InventoryItem.Name(ItemsToCheck[i]), 0);
		}
		parameters.Add("TimeInDungeon", 0);
		parameters.Add("TimeInGame", 0);
		parameters.Add("DeathsInDungeon", 0);
		parameters.Add("FollowerCount", 0);
		parameters.Add("DungeonRun", 0);
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F10))
		{
			Dispatch(Inventory.itemsDungeon);
		}
		if (Application.isEditor)
		{
			DataManager instance = DataManager.Instance;
			dungeonRun = instance.dungeonRun;
			DeathsInDungeon = instance.playerDeaths;
			FollowerCount = instance.Followers.Count;
			TimeInDungeon = GameManager.TimeInDungeon;
			TimeInGame = instance.TimeInGame;
		}
		frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > lastInterval + updateInterval)
		{
			fps = (float)frames / (realtimeSinceStartup - lastInterval);
			frames = 0;
			lastInterval = realtimeSinceStartup;
		}
		if (MonoSingleton<UIManager>.Instance != null && !MonoSingleton<UIManager>.Instance.IsPaused)
		{
			DataManager.Instance.TimeInGame += Time.unscaledDeltaTime;
		}
	}

	public void GameComplete()
	{
		string version = Application.version;
		new Dictionary<string, object>
		{
			{
				"TimeInDungeon",
				DataManager.Instance.TimeInGame
			},
			{
				"FollowersDead",
				DataManager.Instance.Followers_Dead.Count
			},
			{
				"Followers",
				DataManager.Instance.Followers.Count
			}
		};
	}

	public void CultLeaderComplete(int _cultLeader)
	{
		string version = Application.version;
		new Dictionary<string, object>
		{
			{
				"TimeInDungeon",
				DataManager.Instance.TimeInGame
			},
			{
				"Followers",
				DataManager.Instance.Followers.Count
			},
			{ "BossCompleted", _cultLeader }
		};
	}

	public void LevelComplete(int _dungeon, int _dungeonFloor, int _woodCollected, int _goldCollected, int _foodCollected, int _kills, int _time, int damageTaken, bool _beatBoss)
	{
		string version = Application.version;
		new Dictionary<string, object>
		{
			{ "DungeonLayer", _dungeon },
			{ "DungeonNode", _dungeonFloor },
			{ "WoodCollected", _woodCollected },
			{ "GoldCollected", _goldCollected },
			{ "FoodCollected", _foodCollected },
			{ "Kills", _kills },
			{ "Time", _time },
			{ "DamageTaken", damageTaken },
			{ "BeatBoss", _beatBoss },
			{
				"TimeInDungeon",
				DataManager.Instance.TimeInGame
			}
		};
	}

	public static bool Dispatch(List<InventoryItem> _itemsDungeon)
	{
		return false;
	}
}
