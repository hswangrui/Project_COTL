using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class DemonModel
{
	public const int kDemonArchetypeLimit = 1;

	public const int Shooty = 0;

	public const int Chompy = 1;

	public const int Arrows = 2;

	public const int Collector = 3;

	public const int Exploder = 4;

	public const int Spirit = 5;

	public const int Baal = 6;

	public const int Aym = 7;

	public static List<Follower> AvailableFollowersForDemonConversion()
	{
		List<Follower> list = new List<Follower>(FollowerManager.FollowersAtLocation(FollowerLocation.Base));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			int iD = list[num].Brain.Info.ID;
			if (list[num].Brain.Info.CursedState != 0 || FollowerManager.FollowerLocked(iD))
			{
				list.RemoveAt(num);
			}
		}
		return list;
	}

	public static int GetDemonType(FollowerInfo followerInfo)
	{
		return GetDemonType(followerInfo.ID);
	}

	public static int GetDemonType(int id)
	{
		Random.InitState(id);
		if (id == FollowerManager.BaalID)
		{
			return 6;
		}
		if (id == FollowerManager.AymID)
		{
			return 7;
		}
		return Random.Range(0, 6);
	}

	public static string GetTitle(int demonID, int followerID)
	{
		if (followerID == 0)
		{
			return "";
		}
		int demonLevel = FollowerInfo.GetInfoByID(followerID).GetDemonLevel();
		return GetDemonName(demonID) + " " + demonLevel.ToNumeral();
	}

	public static string GetDemonName(int demonID)
	{
		switch (demonID)
		{
		case 0:
			return LocalizationManager.GetTranslation("Interactions/Demon/Projectile");
		case 1:
			return LocalizationManager.GetTranslation("Interactions/Demon/Chomper");
		case 2:
			return LocalizationManager.GetTranslation("Interactions/Demon/Arrows");
		case 3:
			return LocalizationManager.GetTranslation("Interactions/Demon/Collector");
		case 4:
			return LocalizationManager.GetTranslation("Interactions/Demon/Exploder");
		case 5:
		case 6:
		case 7:
			return LocalizationManager.GetTranslation("Interactions/Demon/Spirit");
		default:
			return "";
		}
	}

	public static string GetDescription(int demonID, FollowerInfo follower)
	{
		int demonLevel = follower.GetDemonLevel();
		switch (demonID)
		{
		case 0:
			return string.Concat(LocalizationManager.GetTranslation("Interactions/Demon/Projectile/Description") + ((demonLevel > 1) ? ("\n<color=#FFD201>" + LocalizationManager.GetTranslation("Interactions/Demon/Projectile/Upgrade") + "</color>") : ""), " ", string.Format("\n\n<sprite name=\"icon_Demon_Shooty\"><size=24>+{0}</size>", demonLevel));
		case 1:
			return string.Concat(LocalizationManager.GetTranslation("Interactions/Demon/Chomper/Description") + ((demonLevel > 1) ? ("\n<color=#FFD201>" + LocalizationManager.GetTranslation("Interactions/Demon/Chomper/Upgrade") + "</color>") : ""), " ", string.Format("\n\n<sprite name=\"icon_Demon_Chomp\"><size=24>+{0}</size>", demonLevel));
		case 2:
			return string.Concat(LocalizationManager.GetTranslation("Interactions/Demon/Arrows/Description") + ((demonLevel > 1) ? ("\n<color=#FFD201>" + LocalizationManager.GetTranslation("Interactions/Demon/Arrows/Upgrade") + "</color>") : ""), " ", string.Format("\n\n<sprite name=\"icon_Demon_Arrows\"><size=24>+{0}</size>", demonLevel));
		case 3:
			return string.Concat(LocalizationManager.GetTranslation("Interactions/Demon/Collector/Description") + ((demonLevel > 1) ? ("\n<color=#FFD201>" + LocalizationManager.GetTranslation("Interactions/Demon/Collector/Upgrade") + "</color>") : ""), " ", string.Format("\n\n<sprite name=\"icon_Demon_Collector\"><size=24>+{0}</size>", demonLevel));
		case 4:
			return string.Concat(LocalizationManager.GetTranslation("Interactions/Demon/Exploder/Description") + ((demonLevel > 1) ? ("\n<color=#FFD201>" + LocalizationManager.GetTranslation("Interactions/Demon/Exploder/Upgrade") + "</color>") : ""), " ", string.Format("\n\n<sprite name=\"icon_Demon_Exploder\"><size=24>+{0}</size>", demonLevel));
		case 5:
		case 6:
		case 7:
			return string.Concat(LocalizationManager.GetTranslation("Interactions/Demon/Spirit/Description") + ((demonLevel > 1) ? ("\n<color=#FFD201>" + LocalizationManager.GetTranslation("Interactions/Demon/Spirit/Upgrade") + "</color>") : ""), " ", string.Format("\n\n<sprite name=\"icon_Demon_Hearts\"><size=24>+{0}</size>", demonLevel));
		default:
			return "";
		}
	}

	public static string GetDescription(int demonID)
	{
		switch (demonID)
		{
		case 0:
			return LocalizationManager.GetTranslation("Interactions/Demon/Projectile/Description");
		case 1:
			return LocalizationManager.GetTranslation("Interactions/Demon/Chomper/Description");
		case 2:
			return LocalizationManager.GetTranslation("Interactions/Demon/Arrows/Description");
		case 3:
			return LocalizationManager.GetTranslation("Interactions/Demon/Collector/Description");
		case 4:
			return LocalizationManager.GetTranslation("Interactions/Demon/Exploder/Description");
		case 5:
		case 6:
		case 7:
			return LocalizationManager.GetTranslation("Interactions/Demon/Spirit/Description");
		default:
			return "";
		}
	}

	public static string GetDemonUpgradeDescription(int demonID)
	{
		switch (demonID)
		{
		case 0:
			return LocalizationManager.GetTranslation("Interactions/Demon/Projectile/Upgrade");
		case 1:
			return LocalizationManager.GetTranslation("Interactions/Demon/Chomper/Upgrade");
		case 2:
			return LocalizationManager.GetTranslation("Interactions/Demon/Arrows/Upgrade");
		case 3:
			return LocalizationManager.GetTranslation("Interactions/Demon/Collector/Upgrade");
		case 4:
			return LocalizationManager.GetTranslation("Interactions/Demon/Exploder/Upgrade");
		case 5:
		case 6:
		case 7:
			return LocalizationManager.GetTranslation("Interactions/Demon/Spirit/Upgrade");
		default:
			return "";
		}
	}

	public static string GetDemonIcon(int demonID)
	{
		switch (demonID)
		{
		case 0:
			return "<sprite name=\"icon_Demon_Shooty\">";
		case 1:
			return "<sprite name=\"icon_Demon_Chomp\">";
		case 2:
			return "<sprite name=\"icon_Demon_Arrows\">";
		case 3:
			return "<sprite name=\"icon_Demon_Collector\">";
		case 4:
			return "<sprite name=\"icon_Demon_Exploder\">";
		case 5:
		case 6:
		case 7:
			return "<sprite name=\"icon_Demon_Hearts\">";
		default:
			return "";
		}
	}
}
