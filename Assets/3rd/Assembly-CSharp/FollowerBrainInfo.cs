using System;
using System.Collections.Generic;

public class FollowerBrainInfo
{
	private FollowerInfo _info;

	private FollowerBrain _brain;

	public Action OnPromotion;

	public float CacheXP;

	public static FollowerBrainStats.StatChangedEvent OnReadyToLevelUp;

	public Action OnReadyToPromote;

	public const float IllnessRadius = 3f;

	public int ID
	{
		get
		{
			return _info.ID;
		}
	}

	public string Name
	{
		get
		{
			return _info.Name;
		}
		set
		{
			_info.Name = value;
		}
	}

	public int Age
	{
		get
		{
			return _info.Age;
		}
		set
		{
			_info.Age = value;
		}
	}

	public int LifeExpectancy
	{
		get
		{
			return _info.LifeExpectancy * ((_info.Necklace != InventoryItem.ITEM_TYPE.Necklace_3) ? 1 : 2);
		}
		set
		{
			_info.LifeExpectancy = value;
		}
	}

	public bool OldAge
	{
		get
		{
			return _info.OldAge;
		}
		set
		{
			_info.OldAge = value;
		}
	}

	public bool MarriedToLeader
	{
		get
		{
			return _info.MarriedToLeader;
		}
		set
		{
			_info.MarriedToLeader = value;
		}
	}

	public float PrayProgress
	{
		get
		{
			return _info.PrayProgress;
		}
		set
		{
			_info.PrayProgress = value;
		}
	}

	public bool FirstTimeSpeakingToPlayer
	{
		get
		{
			return _info.FirstTimeSpeakingToPlayer;
		}
		set
		{
			_info.FirstTimeSpeakingToPlayer = value;
		}
	}

	public bool ComplainingAboutNoHouse
	{
		get
		{
			return _info.ComplainingAboutNoHouse;
		}
		set
		{
			_info.ComplainingAboutNoHouse = value;
		}
	}

	public bool ComplainingNeedBetterHouse
	{
		get
		{
			return _info.ComplainingNeedBetterHouse;
		}
		set
		{
			_info.ComplainingNeedBetterHouse = value;
		}
	}

	public Thought CursedState
	{
		get
		{
			return _info.CursedState;
		}
		set
		{
			_info.CursedState = value;
		}
	}

	public FollowerRole FollowerRole
	{
		get
		{
			return _info.FollowerRole;
		}
		set
		{
			_info.FollowerRole = value;
		}
	}

	public WorkerPriority WorkerPriority
	{
		get
		{
			return _info.WorkerPriority;
		}
		set
		{
			_info.WorkerPriority = value;
		}
	}

	public FollowerOutfitType Outfit
	{
		get
		{
			return _info.Outfit;
		}
		set
		{
			_info.Outfit = value;
		}
	}

	public InventoryItem.ITEM_TYPE Necklace
	{
		get
		{
			return _info.Necklace;
		}
		set
		{
			_info.Necklace = value;
		}
	}

	public string SkinName
	{
		get
		{
			return _info.SkinName;
		}
		set
		{
			_info.SkinName = value;
		}
	}

	public int SkinColour
	{
		get
		{
			return _info.SkinColour;
		}
		set
		{
			_info.SkinColour = value;
		}
	}

	public int SkinCharacter
	{
		get
		{
			return _info.SkinCharacter;
		}
		set
		{
			_info.SkinCharacter = value;
		}
	}

	public int SkinVariation
	{
		get
		{
			return _info.SkinVariation;
		}
		set
		{
			_info.SkinVariation = value;
		}
	}

	public int SacrificialValue
	{
		get
		{
			return _info.SacrificialValue;
		}
	}

	public InventoryItem.ITEM_TYPE SacrificialType
	{
		get
		{
			if (!GameManager.HasUnlockAvailable() && !DataManager.Instance.DeathCatBeaten)
			{
				return InventoryItem.ITEM_TYPE.BLACK_GOLD;
			}
			return _info.SacrificialType;
		}
		set
		{
			_info.SacrificialType = value;
		}
	}

	public bool TaxEnforcer
	{
		get
		{
			return _info.TaxEnforcer;
		}
		set
		{
			_info.TaxEnforcer = value;
		}
	}

	public bool FaithEnforcer
	{
		get
		{
			return _info.FaithEnforcer;
		}
		set
		{
			_info.FaithEnforcer = value;
		}
	}

	public string ViewerID
	{
		get
		{
			return _info.ViewerID;
		}
		set
		{
			_info.ViewerID = value;
		}
	}

	public float ProductivityMultiplier
	{
		get
		{
			float num = 1f;
			num += (float)(_brain.ThoughtExists(Thought.Intimidated) ? 1 : 0);
			num += (_brain.HasTrait(FollowerTrait.TraitType.Industrious) ? 0.15f : 0f);
			num -= (_brain.HasTrait(FollowerTrait.TraitType.Lazy) ? 0.1f : 0f);
			num += (_brain.HasTrait(FollowerTrait.TraitType.MushroomBanned) ? 0.05f : 0f);
			num += ((_brain.HasTrait(FollowerTrait.TraitType.Libertarian) && DataManager.Instance.Followers_Imprisoned_IDs.Count <= 0) ? 0.05f : 0f);
			num += (_brain.ThoughtExists(Thought.PropogandaSpeakers) ? 0.2f : 0f);
			return (1f + ((float)XPLevel - 1f) / 5f) * num;
		}
	}

	public int XPLevel
	{
		get
		{
			return _info.XPLevel;
		}
		set
		{
			_info.XPLevel = value;
		}
	}

	public IEnumerable<IDAndRelationship> Relationships
	{
		get
		{
			foreach (IDAndRelationship relationship in _info.Relationships)
			{
				yield return relationship;
			}
		}
	}

	public List<FollowerTrait.TraitType> Traits
	{
		get
		{
			return _info.Traits;
		}
	}

	public FollowerBrainInfo(FollowerInfo info, FollowerBrain brain)
	{
		_info = info;
		_brain = brain;
	}

	public bool HasTrait(FollowerTrait.TraitType type)
	{
		bool result = false;
		foreach (FollowerTrait.TraitType trait in Traits)
		{
			if (trait == type)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public IDAndRelationship GetOrCreateRelationship(int ID)
	{
		foreach (IDAndRelationship relationship in Relationships)
		{
			if (relationship.ID == ID)
			{
				return relationship;
			}
		}
		IDAndRelationship iDAndRelationship = new IDAndRelationship();
		iDAndRelationship.ID = ID;
		iDAndRelationship.Relationship = 0;
		_info.Relationships.Add(iDAndRelationship);
		return iDAndRelationship;
	}

	public void NextSkin()
	{
		List<WorshipperData.CharacterSkin> skin = WorshipperData.Instance.GetCharacters(SkinName).Skin;
		_info.SkinVariation++;
		if (_info.SkinVariation > skin.Count - 1)
		{
			_info.SkinVariation = 0;
		}
		_info.SkinName = skin[_info.SkinVariation].Skin;
	}

	public void PrevSkin()
	{
		List<WorshipperData.CharacterSkin> skin = WorshipperData.Instance.GetCharacters(SkinName).Skin;
		_info.SkinVariation--;
		if (_info.SkinVariation < 0)
		{
			_info.SkinVariation = skin.Count - 1;
		}
		_info.SkinName = skin[_info.SkinVariation].Skin;
	}

	public void NextSkinColor()
	{
		_info.SkinColour++;
		if (_info.SkinColour > WorshipperData.Instance.GetColourData(SkinName).SlotAndColours.Count - 1)
		{
			_info.SkinColour = 0;
		}
	}

	public void PrevSkinColor()
	{
		_info.SkinColour--;
		if (_info.SkinColour < 0)
		{
			_info.SkinColour = WorshipperData.Instance.GetColourData(SkinName).SlotAndColours.Count - 1;
		}
	}
}
