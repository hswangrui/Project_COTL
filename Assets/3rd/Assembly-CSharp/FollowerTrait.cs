using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.U2D;

[Serializable]
public class FollowerTrait
{
	public enum TraitType
	{
		None,
		NaturallySkeptical,
		NaturallyObedient,
		DesensitisedToDeath,
		FearOfDeath,
		Cannibal,
		GrassEater,
		Disciplinarian,
		Libertarian,
		SacrificeEnthusiast,
		AgainstSacrifice,
		Faithful,
		Faithless,
		FearOfSickPeople,
		LoveOfSickPeople,
		Sickly,
		IronStomach,
		Zealous,
		Materialistic,
		FalseIdols,
		Cynical,
		Gullible,
		Germophobe,
		Coprophiliac,
		Industrious,
		Lazy,
		SermonEnthusiast,
		ConstructionEnthusiast,
		MushroomEncouraged,
		MushroomBanned,
		LoveElderly,
		HateElderly,
		Immortal,
		DontStarve,
		ExCultLeader
	}

	public TraitType Type;

	public static Dictionary<TraitType, TraitType> ExclusiveTraits = new Dictionary<TraitType, TraitType>
	{
		{
			TraitType.Germophobe,
			TraitType.Coprophiliac
		},
		{
			TraitType.FearOfSickPeople,
			TraitType.LoveOfSickPeople
		},
		{
			TraitType.Cynical,
			TraitType.Gullible
		},
		{
			TraitType.Disciplinarian,
			TraitType.Libertarian
		},
		{
			TraitType.Sickly,
			TraitType.IronStomach
		},
		{
			TraitType.NaturallyObedient,
			TraitType.NaturallySkeptical
		},
		{
			TraitType.SacrificeEnthusiast,
			TraitType.AgainstSacrifice
		},
		{
			TraitType.Faithful,
			TraitType.Faithless
		},
		{
			TraitType.Industrious,
			TraitType.Lazy
		},
		{
			TraitType.MushroomEncouraged,
			TraitType.MushroomBanned
		},
		{
			TraitType.LoveElderly,
			TraitType.HateElderly
		},
		{
			TraitType.FearOfDeath,
			TraitType.DesensitisedToDeath
		}
	};

	public static List<TraitType> StartingTraits = new List<TraitType>
	{
		TraitType.Germophobe,
		TraitType.Coprophiliac,
		TraitType.Cynical,
		TraitType.Gullible,
		TraitType.NaturallySkeptical,
		TraitType.NaturallyObedient,
		TraitType.FearOfDeath,
		TraitType.AgainstSacrifice,
		TraitType.Sickly,
		TraitType.IronStomach,
		TraitType.Materialistic,
		TraitType.Zealous,
		TraitType.Lazy
	};

	public static List<TraitType> GoodTraits = new List<TraitType>
	{
		TraitType.Faithful,
		TraitType.Coprophiliac,
		TraitType.Gullible,
		TraitType.NaturallyObedient,
		TraitType.IronStomach,
		TraitType.Zealous,
		TraitType.Coprophiliac,
		TraitType.LoveOfSickPeople,
		TraitType.Gullible
	};

	public static List<TraitType> RareStartingTraits = new List<TraitType>
	{
		TraitType.Faithful,
		TraitType.Faithless
	};

	public static TraitType GetStartingTrait()
	{
		List<TraitType> list = new List<TraitType>(StartingTraits);
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.HasTrait(TraitType.FearOfDeath) && list.Contains(TraitType.FearOfDeath))
			{
				list.Remove(TraitType.FearOfDeath);
			}
			if (allBrain.HasTrait(TraitType.AgainstSacrifice) && list.Contains(TraitType.AgainstSacrifice))
			{
				list.Remove(TraitType.AgainstSacrifice);
			}
		}
		int num = 0;
		while (++num < 100)
		{
			TraitType traitType = list[UnityEngine.Random.Range(0, list.Count)];
			if (!DataManager.Instance.CultTraits.Contains(traitType))
			{
				return traitType;
			}
		}
		return TraitType.None;
	}

	public static TraitType GetRareTrait()
	{
		int num = 0;
		while (++num < 100)
		{
			TraitType traitType = RareStartingTraits[UnityEngine.Random.Range(0, RareStartingTraits.Count)];
			if (!DataManager.Instance.CultTraits.Contains(traitType))
			{
				return traitType;
			}
		}
		return TraitType.None;
	}

	public static string GetLocalizedTitle(TraitType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Traits/{0}", Type));
	}

	public static string GetLocalizedDescription(TraitType Type)
	{
		return LocalizationManager.GetTranslation(string.Format("Traits/{0}/Description", Type));
	}

	public static Sprite GetIcon(TraitType Type)
	{
		//return Resources.Load<SpriteAtlas>("Atlases/TraitIcons").GetSprite(string.Format("Icon_Trait_{0}", Type.ToString()));
		return Resources.Load<Sprite>(string.Format("sprite/Icon_Trait_{0}", Type.ToString()));
	}

	public static void RemoveExclusiveTraits(FollowerBrain Brain, TraitType TraitType)
	{
		foreach (KeyValuePair<TraitType, TraitType> exclusiveTrait in ExclusiveTraits)
		{
			if (exclusiveTrait.Key == TraitType)
			{
				Brain.RemoveTrait(exclusiveTrait.Value, false);
			}
			if (exclusiveTrait.Value == TraitType)
			{
				Brain.RemoveTrait(exclusiveTrait.Key, false);
			}
		}
	}

	public static void AddCultTrait(TraitType TraitType)
	{
		if (!DataManager.Instance.CultTraits.Contains(TraitType))
		{
			DataManager.Instance.CultTraits.Add(TraitType);
		}
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			foreach (KeyValuePair<TraitType, TraitType> exclusiveTrait in ExclusiveTraits)
			{
				if (exclusiveTrait.Key == TraitType)
				{
					allBrain.RemoveTrait(exclusiveTrait.Value, false);
				}
				if (exclusiveTrait.Value == TraitType)
				{
					allBrain.RemoveTrait(exclusiveTrait.Key, false);
				}
			}
		}
	}

	public static bool Contains(List<FollowerTrait> List, TraitType TraitType)
	{
		foreach (FollowerTrait item in List)
		{
			if (item.Type == TraitType)
			{
				return true;
			}
		}
		return false;
	}

	public static FollowerTrait GetTrait(List<FollowerTrait> List, TraitType TraitType)
	{
		foreach (FollowerTrait item in List)
		{
			if (item.Type == TraitType)
			{
				return item;
			}
		}
		return null;
	}

	public static bool IsPositiveTrait(TraitType traitType)
	{
		switch (traitType)
		{
		case TraitType.NaturallyObedient:
		case TraitType.DesensitisedToDeath:
		case TraitType.Cannibal:
		case TraitType.GrassEater:
		case TraitType.Disciplinarian:
		case TraitType.Libertarian:
		case TraitType.SacrificeEnthusiast:
		case TraitType.Faithful:
		case TraitType.LoveOfSickPeople:
		case TraitType.IronStomach:
		case TraitType.Zealous:
		case TraitType.Materialistic:
		case TraitType.FalseIdols:
		case TraitType.Gullible:
		case TraitType.Coprophiliac:
		case TraitType.Industrious:
		case TraitType.SermonEnthusiast:
		case TraitType.ConstructionEnthusiast:
		case TraitType.MushroomEncouraged:
		case TraitType.MushroomBanned:
		case TraitType.LoveElderly:
		case TraitType.HateElderly:
		case TraitType.Immortal:
		case TraitType.DontStarve:
			return true;
		default:
			return false;
		}
	}
}
