using Spine;
using Spine.Unity;
using UnityEngine;

public class FollowerOutfit
{
	private FollowerInfo _info;

	public bool IsHooded { get; private set; }

	public FollowerOutfitType CurrentOutfit { get; private set; }

	public HatType CurrentHat { get; private set; }

	public FollowerOutfit(FollowerInfo info)
	{
		_info = info;
	}

	public void SetInfo(FollowerInfo info)
	{
		_info = info;
	}

	public void SetOutfit(SkeletonAnimation spine, bool hooded)
	{
		if (_info != null)
		{
			SetOutfit(spine, _info.Outfit, _info.Necklace, hooded, Thought.None, CurrentHat);
		}
	}

	public void SetOutfit(SkeletonAnimation spine, FollowerOutfitType outfit, InventoryItem.ITEM_TYPE necklace, bool hooded, Thought overrideCursedState = Thought.None, HatType hat = HatType.None)
	{
		Skin skin = new Skin("New Skin");
	
		Skin skin2 = spine.Skeleton.Data.FindSkin(_info.SkinName);
		if (skin2 != null)
		{
			skin.AddSkin(skin2);
		}
		else
		{
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Cat"));
			_info.SkinName = "Cat";
		}
		string outfitSkinName = GetOutfitSkinName(outfit);
		if (!string.IsNullOrEmpty(outfitSkinName))
		{
			skin.AddSkin(spine.skeleton.Data.FindSkin(outfitSkinName));
		}
		if (_info.ID == FollowerManager.BaalID)
		{
			skin.AddSkin(spine.skeleton.Data.FindSkin("Clothes/Robes_Baal"));
		}
		else if (_info.ID == FollowerManager.AymID)
		{
			skin.AddSkin(spine.skeleton.Data.FindSkin("Clothes/Robes_Aym"));
		}
		if (necklace != 0)
		{
			skin.AddSkin(spine.skeleton.Data.FindSkin("Necklaces/" + necklace));
		}
		switch (outfit)
		{
		case FollowerOutfitType.Old:
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Other/Old"));
			break;
		case FollowerOutfitType.HorseTown:
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Clothes/HorseTown"));
			break;
		case FollowerOutfitType.Undertaker:
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Clothes/Undertaker"));
			break;
		}
		if (!hooded)
		{
			if (_info.TaxEnforcer)
			{
				hat = HatType.TaxEnforcer;
			}
			else if (_info.FaithEnforcer)
			{
				hat = HatType.FaithEnforcer;
			}
		}
		if (hooded)
		{
			string skinName = "Clothes/Hooded_Lvl1";
			if (_info.XPLevel == 2)
			{
				skinName = "Clothes/Hooded_Lvl2";
			}
			else if (_info.XPLevel == 3)
			{
				skinName = "Clothes/Hooded_Lvl3";
			}
			else if (_info.XPLevel == 4)
			{
				skinName = "Clothes/Hooded_Lvl4";
			}
			else if (_info.XPLevel >= 5)
			{
				skinName = "Clothes/Hooded_Lvl5";
			}
			if (_info.CursedState == Thought.OldAge)
			{
				skinName = "Clothes/Hooded_HorseTown";
			}
			skin.AddSkin(spine.Skeleton.Data.FindSkin(skinName));
			IsHooded = true;
		}
		else
		{
			IsHooded = false;
			switch (hat)
			{
			case HatType.Chef:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Chef"));
				break;
			case HatType.FaithEnforcer:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/FaithEnforcer"));
				break;
			case HatType.TaxEnforcer:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Enforcer"));
				break;
			case HatType.Farm:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Farm"));
				break;
			case HatType.Lumberjack:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Lumberjack"));
				break;
			case HatType.Miner:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Miner"));
				break;
			case HatType.Refiner:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Refinery"));
				break;
			}
		}
		if (FollowerBrainStats.BrainWashed)
		{
			skin.AddSkin(spine.skeleton.Data.FindSkin("Other/Brainwashed"));
		}
		else if (_info.CursedState == Thought.Dissenter || overrideCursedState == Thought.Dissenter)
		{
			skin.AddSkin(spine.skeleton.Data.FindSkin("Other/Dissenter"));
		}
		if (outfit == FollowerOutfitType.Ghost)
		{
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Other/Ghost"));
		}
		spine.Skeleton.SetSkin(skin);
		spine.skeleton.SetSlotsToSetupPose();
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(_info.SkinName);
		if (colourData != null)
		{
			foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(_info.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
			{
				Slot slot = spine.skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
		}
		CurrentOutfit = outfit;
		CurrentHat = hat;
	}

	public static string GetHatByFollowerRole(FollowerRole FollowerRole)
	{
		return "";
	}

	public void SetOutfit(SkeletonGraphic spine, bool hooded)
	{
		SetOutfit(spine, _info.Outfit, _info.Necklace, hooded);
	}

	public void SetOutfit(SkeletonGraphic spine, FollowerOutfitType outfit, InventoryItem.ITEM_TYPE necklace, bool hooded, HatType hat = HatType.None)
	{
		Skin skin = new Skin("New Skin");
		Skin skin2 = spine.Skeleton.Data.FindSkin(_info.SkinName);
		if (skin2 != null)
		{
			skin.AddSkin(skin2);
		}
		else
		{
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Cat"));
			_info.SkinName = "Cat";
		}
		string outfitSkinName = GetOutfitSkinName(outfit);
		if (!string.IsNullOrEmpty(outfitSkinName))
		{
			skin.AddSkin(spine.Skeleton.Data.FindSkin(outfitSkinName));
		}
		if (_info.ID == FollowerManager.BaalID)
		{
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Clothes/Robes_Baal"));
		}
		else if (_info.ID == FollowerManager.AymID)
		{
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Clothes/Robes_Aym"));
		}
		if (necklace != 0)
		{
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Necklaces/" + necklace));
		}
		switch (outfit)
		{
		case FollowerOutfitType.Old:
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Other/Old"));
			break;
		case FollowerOutfitType.Undertaker:
			skin.AddSkin(spine.Skeleton.Data.FindSkin("Clothes/Undertaker"));
			break;
		}
		if (hooded)
		{
			string skinName = "Clothes/Hooded_Lvl1";
			if (_info.XPLevel == 2)
			{
				skinName = "Clothes/Hooded_Lvl2";
			}
			else if (_info.XPLevel == 3)
			{
				skinName = "Clothes/Hooded_Lvl3";
			}
			else if (_info.XPLevel == 4)
			{
				skinName = "Clothes/Hooded_Lvl4";
			}
			else if (_info.XPLevel >= 5)
			{
				skinName = "Clothes/Hooded_Lvl5";
			}
			if (_info.CursedState == Thought.OldAge)
			{
				skinName = "Clothes/Hooded_HorseTown";
			}
			skin.AddSkin(spine.Skeleton.Data.FindSkin(skinName));
			IsHooded = true;
		}
		else
		{
			IsHooded = false;
			switch (hat)
			{
			case HatType.Chef:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Chef"));
				break;
			case HatType.FaithEnforcer:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/FaithEnforcer"));
				break;
			case HatType.TaxEnforcer:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Enforcer"));
				break;
			case HatType.Farm:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Farm"));
				break;
			case HatType.Lumberjack:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Lumberjack"));
				break;
			case HatType.Miner:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Miner"));
				break;
			case HatType.Refiner:
				skin.AddSkin(spine.Skeleton.Data.FindSkin("Hats/Refinery"));
				break;
			}
		}
		spine.Skeleton.SetSkin(skin);
		spine.Skeleton.SetSlotsToSetupPose();
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(_info.SkinName);
		if (colourData == null)
		{
			return;
		}
		foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(_info.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
		{
			Slot slot = spine.Skeleton.FindSlot(slotAndColour.Slot);
			if (slot != null)
			{
				slot.SetColor(slotAndColour.color);
			}
		}
	}

	public string GetHoodedSkinName(FollowerOutfitType outfit)
	{
		string text = "";
		switch (_info.FollowerRole)
		{
		case FollowerRole.Worshipper:
			return "Clothes/Hooded_Lvl3";
		case FollowerRole.Monk:
			return "Clothes/Hooded_Lvl5";
		default:
			return "Clothes/Hooded_Lvl1";
		}
	}

	public string GetOutfitSkinName(FollowerOutfitType outfit)
	{
		string result = "";
		if (CheatConsole.Robes)
		{
			return "Clothes/Robes_Lvl5";
		}
		switch (outfit)
		{
		case FollowerOutfitType.Rags:
			result = "Clothes/Rags";
			break;
		case FollowerOutfitType.Old:
			result = "Clothes/HorseTown";
			break;
		case FollowerOutfitType.Sherpa:
			result = "Clothes/Sherpa";
			break;
		case FollowerOutfitType.Warrior:
			result = "Clothes/Warrior";
			break;
		case FollowerOutfitType.Worshipper:
			result = "Clothes/Robes_Lvl3";
			break;
		case FollowerOutfitType.Worker:
			result = "Clothes/Robes_Lvl1";
			break;
		case FollowerOutfitType.Holiday:
			result = "Clothes/Holiday";
			break;
		case FollowerOutfitType.Follower:
			switch (_info.FollowerRole)
			{
			case FollowerRole.Worshipper:
				result = "Clothes/Robes_Lvl3";
				break;
			case FollowerRole.Monk:
				result = "Clothes/Robes_Lvl5";
				break;
			default:
				result = "Clothes/Robes_Lvl1";
				break;
			}
			break;
		}
		return result;
	}
}
