using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Villager_Info
{
	public enum Faction
	{
		Generic,
		Fundamentalist,
		Utopianist,
		Misfit
	}

	public enum StatusState
	{
		Off,
		On,
		Kill
	}

	public delegate void StatusEffectEvent(StatusState State);

	public int ID;

	public string Name;

	public int SkinVariation;

	public int SkinColour;

	public int Age;

	public float color_r;

	public float color_g;

	public float color_b;

	private int RandomSkin;

	public string SkinName;

	public Faction MyFaction;

	public WorshipperInfoManager.Outfit Outfit;

	public string WorkPlace;

	public int WorkPlaceSlot;

	public string Dwelling = "-1";

	public int DwellingSlot;

	public bool DwellingClaimed;

	public float HP;

	public float TotalHP;

	public bool SleptOutside;

	public Vector3 SleptOutsidePosition;

	public static Action OnFaithChanged;

	private float _Faith = 70f;

	public float FearLove = 50f;

	public static float FearThreshold = 20f;

	public static float LoveThreshold = 80f;

	public static float IllnessThreshold = 50f;

	public float Hunger = 100f;

	public static StatusEffectEvent OnStarveChanged;

	[XmlIgnore]
	public StatusEffectEvent OnStarve;

	public float _Starve;

	public static int StarveDeath = 300;

	public static float LowFaithThreshold = 30f;

	public float Sleep;

	public int DevotionGiven;

	public float Devotion;

	public int Level = 1;

	public static StatusEffectEvent OnIllnessChanged;

	[XmlIgnore]
	public StatusEffectEvent OnIllness;

	private float _Illness;

	public static StatusEffectEvent OnDissenterChanged;

	[XmlIgnore]
	public StatusEffectEvent OnDissenter;

	public bool isDissenter;

	private float _Dissentor = 50f;

	public static float DissentorThreshold = 80f;

	public static float RelationshipHateThreshold = -10f;

	public static float RelationshipFriendThreshold = 5f;

	public static float RelationshipLoveThreshold = 10f;

	public static List<int> LevelTargets = new List<int> { 50, 100, 200, 300, 400, 500 };

	public bool Complaint_House;

	public bool Complaint_Food;

	public int GuaranteedGoodInteractionsUntil = -1;

	public int FastingUntil = -1;

	public int IncreasedDevotionOutputUntil = -1;

	public int BrainwashedUntil = -1;

	public int MotivatedUntil = -1;

	public List<IDAndRelationship> Relationships = new List<IDAndRelationship>();

	public float Faith
	{
		get
		{
			return _Faith;
		}
		set
		{
			float num = Mathf.Clamp(value, 0f, 100f);
			if (Brainwashed)
			{
				num = 100f;
			}
			if (num != _Faith)
			{
				_Faith = num;
				Action onFaithChanged = OnFaithChanged;
				if (onFaithChanged != null)
				{
					onFaithChanged();
				}
			}
		}
	}

	public float Starve
	{
		get
		{
			return _Starve;
		}
		set
		{
			if (_Starve == value)
			{
				return;
			}
			float starve = _Starve;
			_Starve = value;
			if (value <= 0f && starve > 0f)
			{
				StatusEffectEvent onStarve = OnStarve;
				if (onStarve != null)
				{
					onStarve(StatusState.Off);
				}
				StatusEffectEvent onStarveChanged = OnStarveChanged;
				if (onStarveChanged != null)
				{
					onStarveChanged(StatusState.Off);
				}
			}
			if (value > 0f && starve <= 0f)
			{
				StatusEffectEvent onStarve2 = OnStarve;
				if (onStarve2 != null)
				{
					onStarve2(StatusState.On);
				}
				StatusEffectEvent onStarveChanged2 = OnStarveChanged;
				if (onStarveChanged2 != null)
				{
					onStarveChanged2(StatusState.On);
				}
			}
			if (value >= (float)StarveDeath && starve < (float)StarveDeath)
			{
				StatusEffectEvent onStarve3 = OnStarve;
				if (onStarve3 != null)
				{
					onStarve3(StatusState.Kill);
				}
				StatusEffectEvent onStarveChanged3 = OnStarveChanged;
				if (onStarveChanged3 != null)
				{
					onStarveChanged3(StatusState.Kill);
				}
			}
		}
	}

	public float Illness
	{
		get
		{
			return _Illness;
		}
		set
		{
			if (value != _Illness)
			{
				if (value >= IllnessThreshold && _Illness < IllnessThreshold)
				{
					Debug.Log("Illness on!");
					StatusEffectEvent onIllness = OnIllness;
					if (onIllness != null)
					{
						onIllness(StatusState.On);
					}
					StatusEffectEvent onIllnessChanged = OnIllnessChanged;
					if (onIllnessChanged != null)
					{
						onIllnessChanged(StatusState.On);
					}
				}
				if (value < IllnessThreshold && _Illness >= IllnessThreshold)
				{
					Debug.Log("Illness off!");
					StatusEffectEvent onIllness2 = OnIllness;
					if (onIllness2 != null)
					{
						onIllness2(StatusState.Off);
					}
					StatusEffectEvent onIllnessChanged2 = OnIllnessChanged;
					if (onIllnessChanged2 != null)
					{
						onIllnessChanged2(StatusState.Off);
					}
				}
				else if (value >= 100f)
				{
					Debug.Log("Illness kill");
					StatusEffectEvent onIllness3 = OnIllness;
					if (onIllness3 != null)
					{
						onIllness3(StatusState.Kill);
					}
					StatusEffectEvent onIllnessChanged3 = OnIllnessChanged;
					if (onIllnessChanged3 != null)
					{
						onIllnessChanged3(StatusState.Kill);
					}
				}
			}
			_Illness = Mathf.Clamp(value, 0f, 100f);
		}
	}

	public float Dissentor
	{
		get
		{
			return _Dissentor;
		}
		set
		{
			if (value != _Dissentor)
			{
				if (value >= DissentorThreshold && _Dissentor < DissentorThreshold)
				{
					isDissenter = true;
					StatusEffectEvent onDissenter = OnDissenter;
					if (onDissenter != null)
					{
						onDissenter(StatusState.On);
					}
					StatusEffectEvent onDissenterChanged = OnDissenterChanged;
					if (onDissenterChanged != null)
					{
						onDissenterChanged(StatusState.On);
					}
				}
				else if (value < DissentorThreshold && _Dissentor >= DissentorThreshold)
				{
					isDissenter = false;
					StatusEffectEvent onDissenter2 = OnDissenter;
					if (onDissenter2 != null)
					{
						onDissenter2(StatusState.Off);
					}
					StatusEffectEvent onDissenterChanged2 = OnDissenterChanged;
					if (onDissenterChanged2 != null)
					{
						onDissenterChanged2(StatusState.Off);
					}
				}
			}
			_Dissentor = Mathf.Clamp(value, 0f, 100f);
		}
	}

	public bool Fasting
	{
		get
		{
			return FastingUntil >= DataManager.Instance.CurrentDayIndex;
		}
	}

	public bool Brainwashed
	{
		get
		{
			return BrainwashedUntil >= DataManager.Instance.CurrentDayIndex;
		}
	}

	public int GetDevotionLimit()
	{
		switch (Level)
		{
		default:
			return 5;
		case 2:
			return 15;
		case 3:
			return 30;
		case 4:
			return 50;
		}
	}

	public void DecreaseHunger(float SpeedMultiplier)
	{
		if (!Fasting)
		{
			Hunger = Mathf.Clamp(Hunger - Time.deltaTime * 1f * SpeedMultiplier, -100f, 100f);
		}
		Debug.Log(Name + "  " + Hunger);
	}

	public void IncreaseDevotion(float SpeedMultiplier)
	{
		Devotion += SpeedMultiplier * Time.deltaTime / 60f;
		Devotion = Mathf.Clamp(Devotion, 0f, GetDevotionLimit());
	}

	public static float TotalFaithNormalised()
	{
		if (DataManager.Instance.Followers.Count <= 0)
		{
			return 0f;
		}
		float num = 0f;
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			num += follower.Faith;
		}
		return num / (100f * (float)DataManager.Instance.Followers.Count);
	}

	public void DecreaseCultFaith(float Speed)
	{
		Faith = 50f;
	}

	public void IncreaseCultFaith(float Speed)
	{
		Faith = 50f;
	}

	public void Brainwash(int durationDays)
	{
		bool brainwashed = Brainwashed;
		BrainwashedUntil = DataManager.Instance.CurrentDayIndex + (durationDays - 1);
		Faith = 100f;
	}

	public static FollowerInfo GetVillagerInfoByID(int ID)
	{
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.ID == ID)
			{
				return follower;
			}
		}
		return null;
	}

	public static Villager_Info NewCharacter(string ForceSkin = "")
	{
		Villager_Info villager_Info = new Villager_Info();
		villager_Info.Name = GenerateName();
		villager_Info.Age = UnityEngine.Random.Range(15, 30);
		villager_Info.ID = ++DataManager.Instance.FollowerID;
		if (ForceSkin == "")
		{
			int index = UnityEngine.Random.Range(0, WorshipperData.Instance.Characters.Count);
			villager_Info.SkinVariation = UnityEngine.Random.Range(0, WorshipperData.Instance.Characters[index].Skin.Count);
			villager_Info.SkinName = WorshipperData.Instance.Characters[index].Skin[villager_Info.SkinVariation].Skin;
			villager_Info.SkinColour = UnityEngine.Random.Range(0, WorshipperData.Instance.GetColourData(villager_Info.SkinName).SlotAndColours.Count);
		}
		else
		{
			villager_Info.SkinName = ForceSkin;
		}
		villager_Info.Sleep = UnityEngine.Random.Range(30, 60);
		villager_Info.Hunger = 100f;
		villager_Info.MyFaction = (Faction)UnityEngine.Random.Range(1, 4);
		villager_Info.Devotion = villager_Info.GetDevotionLimit();
		villager_Info.WorkPlace = "-1";
		villager_Info.Dwelling = "-1";
		villager_Info.DwellingClaimed = false;
		villager_Info.HP = (villager_Info.TotalHP = 25f);
		return villager_Info;
	}

	public bool CanLevelUp()
	{
		return DevotionGiven >= LevelTargets[Level - 1];
	}

	public static string GenerateName()
	{
		string text = "";
		List<string> list = new List<string>
		{
			"Ja", "Jul", "Na", "No", "Gre", "Bre", "Tre", "Mer", "Ty", "Ar",
			"An", "Yar", "Fe", "Fi", "The", "Thor"
		};
		text += list[UnityEngine.Random.Range(0, list.Count)];
		List<string> list2 = new List<string>
		{
			"na", "ya", "len", "lay", "no", "tha", "ka", "ki", "ko", "li",
			"lo"
		};
		if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
		{
			text += list2[UnityEngine.Random.Range(0, list2.Count)];
		}
		List<string> list3 = new List<string> { "on", "y", "an", "yen", "son", "ryn", "nor", "mar" };
		return text + list3[UnityEngine.Random.Range(0, list3.Count)];
	}
}
