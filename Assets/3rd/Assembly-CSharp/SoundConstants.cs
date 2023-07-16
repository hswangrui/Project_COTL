public static class SoundConstants
{
	public enum SoundEventType
	{
		None,
		OneShot2D,
		OneShotAtPosition,
		OneShotAttached,
		Loop
	}

	public enum RoomID
	{
		StandardRoom = 0,
		CultLeaderAmbience = 1,
		OfferingCombat = 2,
		Shop = 3,
		Sozo = 4,
		SpecialCombat = 5,
		Healing = 6,
		StandardAmbience = 7,
		BossEntryAmbience = 8,
		MainBossA = 9,
		MainBossB = 10,
		FollowerAmbience = 11,
		EndRoomAmbience = 12,
		BeholderBattle = 13,
		AltStandardRoom = 15,
		Chemach = 16,
		NoMusic = 9999
	}

	public enum BaseID
	{
		StandardAmbience = 0,
		NoFollowers = 1,
		BigEnergy = 2,
		Temple = 3,
		bongos_singing = 4,
		fight_pit_drums = 5,
		DungeonDoor = 6,
		blood_moon = 7,
		NoMusic = 9999
	}

	public enum SoundMaterial
	{
		None,
		Bone,
		Glass,
		Stone,
		Wood,
		WoodBarrel,
		Tree,
		Grass,
		Coins
	}

	public static string GetImpactSoundPathForMaterial(SoundMaterial soundMaterial)
	{
		switch (soundMaterial)
		{
		case SoundMaterial.Bone:
			return "event:/material/bone_impact";
		case SoundMaterial.Glass:
			return "event:/material/stained_glass_impact";
		case SoundMaterial.Stone:
			return "event:/material/stone_impact";
		case SoundMaterial.Wood:
			return "event:/material/wood_impact";
		case SoundMaterial.WoodBarrel:
			return "event:/material/wood_barrel_impact";
		case SoundMaterial.Tree:
			return "event:/material/tree_chop";
		case SoundMaterial.Grass:
			return "event:/player/tall_grass_cut";
		case SoundMaterial.Coins:
			return "event:/rituals/coins";
		default:
			return string.Empty;
		}
	}

	public static string GetBreakSoundPathForMaterial(SoundMaterial soundMaterial)
	{
		switch (soundMaterial)
		{
		case SoundMaterial.Bone:
			return "event:/material/bone_break";
		case SoundMaterial.Glass:
			return "event:/material/stained_glass_break";
		case SoundMaterial.Stone:
			return "event:/material/stone_break";
		case SoundMaterial.Wood:
			return "event:/material/wood_break";
		case SoundMaterial.WoodBarrel:
			return "event:/material/wood_barrel_break";
		case SoundMaterial.Tree:
			return "event:/material/tree_break";
		case SoundMaterial.Grass:
			return "event:/player/tall_grass_cut";
		case SoundMaterial.Coins:
			return "event:/rituals/coins";
		default:
			return string.Empty;
		}
	}
}
