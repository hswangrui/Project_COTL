using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using Unify;
using UnityEngine;

public class AchievementsWrapper : MonoBehaviour
{
	private static List<int> unlockedAchievements;

	public static MonoBehaviour instance;

	public static Achievements.PlatformAchievementsStatusDelegate XBAchievementsCheck = XBGetAchievementProgress;

	public static List<string> Achievements = new List<string>
	{
		"platinum", "ALL_SKINS_UNLOCKED", "ALL_TAROTS_UNLOCKED", "FULLY_UPGRADED_SHRINE", "FEED_FOLLOWER_MEAT", "FIND_ALL_LOCATIONS", "UPGRADE_ALL_SERMONS", "KILL_BOSS_5", "KILL_BOSS_4", "KILL_BOSS_3",
		"KILL_BOSS_2", "KILL_BOSS_1", "UNLOCK_TUNIC", "UNLOCK_ALL_TUNICS", "FIRST_FOLLOWER", "GAIN_FIVE_FOLLOWERS", "TEN_FOLLOWERS", "TWENTY_FOLLOWERS", "TAKE_CONFESSION", "FISH_ALL_TYPES",
		"WIN_KNUCKLEBONES", "WIN_KNUCKLEBONES_ALL", "FIX_LIGHTHOUSE", "FIRST_RITUAL", "FIRST_SACRIFICE", "SACRIFICE_FOLLOWERS", "DEAL_WITH_THE_DEVIL", "666_GOLD", "KILL_FIRST_BOSS", "KILL_BOSS_1_NODAMAGE",
		"KILL_BOSS_2_NODAMAGE", "KILL_BOSS_3_NODAMAGE", "KILL_BOSS_4_NODAMAGE", "DELIVER_FIRST_SERMON", "FIRST_DEATH", "ALL_WEAPONS_UNLOCKED", "ALL_CURSES_UNLOCKED", "ALL_RELICS_UNLOCKED", "BEAT_UP_MIDAS", "RETURN_BAAL_AYM",
		"COMPLETE_CHALLENGE_ROW", "ALL_LEADER_FOLLOWERS"
	};

	private void Awake()
	{
		instance = this;
		string @string = Unify.PlayerPrefs.GetString("unlockedAchievements", "");
		try
		{
			unlockedAchievements = @string.Split(',').Select(int.Parse).ToList();
		}
		catch
		{
			unlockedAchievements = new List<int>();
		}
		compareAchievements();
	}

	public static void UnlockAchievement(Achievement achievementId)
	{
		SessionManager.instance.UnlockAchievement(achievementId);
		if (!unlockedAchievements.Contains(achievementId.id))
		{
			unlockedAchievements.Add(achievementId.id);
			Unify.PlayerPrefs.SetString("unlockedAchievements", unlockedAchievements.ToString());
			compareAchievements();
		}
	}

	public static void XBGetAchievementProgress(List<AchievementProgress> result)
	{
		if (result == null || result.Count == 0)
		{
			return;
		}
		foreach (AchievementProgress item in result)
		{
			if (item.progress >= 100 && !unlockedAchievements.Contains(item.id))
			{
				unlockedAchievements.Add(item.id);
				Unify.PlayerPrefs.SetString("unlockedAchievements", unlockedAchievements.ToString());
			}
		}
	}

	public static void compareAchievements()
	{
		try
		{
			int num = 0;
			foreach (string achievement in Achievements)
			{
				bool pbAchieved = false;
				SteamUserStats.GetAchievement(achievement, out pbAchieved);
				if (pbAchieved)
				{
					num++;
				}
			}
			if (num >= Achievements.Count - 1)
			{
				instance.StartCoroutine(UnlockPlatinum());
			}
		}
		catch (Exception)
		{
		}
	}

	private static IEnumerator UnlockPlatinum()
	{
		yield return new WaitForSeconds(1f);
		Achievement achievement = Unify.Achievements.Instance.Lookup("platinum");
		SessionManager.instance.UnlockAchievement(achievement);
		yield return null;
	}
}
