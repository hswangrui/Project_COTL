using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unify
{
	[Serializable]
	public class Achievements : MonoBehaviour
	{
		public delegate void PlatformAchievementsStatusDelegate(List<AchievementProgress> result);

		public TextAsset achievementsJson;

		[SerializeField]
		private List<Achievement> achievements = new List<Achievement>();

		private Dictionary<string, int> achievementLabelMap = new Dictionary<string, int>();

		private Dictionary<string, int> achievementSteamIdMap = new Dictionary<string, int>();

		private Dictionary<string, int> achievementXboxIdMap = new Dictionary<string, int>();

		private Dictionary<int, int> achievementPS4IdMap = new Dictionary<int, int>();

		public static Achievements Instance;

		public void Start()
		{
			Instance = this;
			if (achievementsJson != null)
			{
				Load(achievementsJson);
			}
			else
			{
				Logger.LogError("UNIFY: Achievements: No achievements to load on start.");
			}
		}

		public int Register(Achievement achievement)
		{
			Logger.Log("UNIFY: ACHIEVEMENTS: Register Achievement #" + achievement.id + " : " + achievement.label);
			achievements.Insert(achievement.id, achievement);
			AddToMaps(achievement);
			return achievement.id;
		}

		private void AddToMaps(Achievement achievement)
		{
			if (!string.IsNullOrEmpty(achievement.label) && !achievementLabelMap.ContainsKey(achievement.label))
			{
				achievementLabelMap.Add(achievement.label, achievement.id);
			}
			if (!string.IsNullOrEmpty(achievement.steamId) && !achievementSteamIdMap.ContainsKey(achievement.steamId))
			{
				achievementSteamIdMap.Add(achievement.steamId, achievement.id);
			}
			if (!string.IsNullOrEmpty(achievement.xboxOneId) && !achievementXboxIdMap.ContainsKey(achievement.xboxOneId))
			{
				achievementXboxIdMap.Add(achievement.xboxOneId, achievement.id);
			}
			if (!achievementPS4IdMap.ContainsKey(achievement.ps4Id))
			{
				achievementPS4IdMap.Add(achievement.ps4Id, achievement.id);
			}
		}

		public void Load(string path)
		{
			Logger.Log("UNIFY: ACHIEVEMENTS: Load Achievements: " + path);
			TextAsset sourceJson = Resources.Load<TextAsset>(path);
			Load(sourceJson);
		}

		public void Load(TextAsset sourceJson)
		{
			Logger.Log("UNIFY: ACHIEVEMENTS: Load Achievements textasset: " + sourceJson.name);
			JsonUtility.FromJsonOverwrite(sourceJson.text, this);
			foreach (Achievement achievement in achievements)
			{
				AddToMaps(achievement);
			}
			Logger.Log("UNIFY: Achievements: " + achievements.Count + " achievements loaded.");
		}

		public void Clear()
		{
			Logger.Log("UNIFY: ACHIEVEMENTS: Clear");
			achievements.Clear();
			achievementLabelMap.Clear();
		}

		public Achievement Get(int id)
		{
			return achievements[id];
		}

		public int GetCount()
		{
			return achievements.Count;
		}

		private Achievement LookupBy<T>(T key, Dictionary<T, int> map)
		{
			if (map.ContainsKey(key))
			{
				int num = map[key];
				if (num >= 0 && num < achievements.Count)
				{
					return achievements[num];
				}
			}
			Logger.LogError("UNIFY: ACHIEVEMENTS: Not found: " + key);
			return Achievement.None;
		}

		public Achievement LookupByXboxOneId(string id)
		{
			return LookupBy(id, achievementXboxIdMap);
		}

		public Achievement LookupBySteamId(string id)
		{
			return LookupBy(id, achievementSteamIdMap);
		}

		public Achievement LookupByPS4Id(int id)
		{
			return LookupBy(id, achievementPS4IdMap);
		}

		public Achievement Lookup(string label)
		{
			return LookupBy(label, achievementLabelMap);
		}
	}
}
