using System;
using System.Collections.Generic;
using System.IO;
using I2.Loc;
using Spine;
using Spine.Unity;
using UnityEngine;

public class WorshipperData : BaseMonoBehaviour
{
	public enum DropLocation
	{
		Dungeon1,
		Dungeon2,
		Dungeon3,
		Dungeon4,
		Other,
		DLC,
		SpecialEvents
	}

	[Serializable]
	public class SkinAndData
	{
		[SerializeField]
		public string Title = "Character Name";

		[SerializeField]
		private DropLocation _dropLocation;

		[SerializeField]
		private bool _hidden;

		[SerializeField]
		private bool _invariant;

		[SerializeField]
		private bool _lockColor;

		public bool TwitchPremium;

		public List<CharacterSkin> Skin = new List<CharacterSkin>();

		public List<SlotsAndColours> SlotAndColours = new List<SlotsAndColours>();

		public List<SlotsAndColours> StartingSlotAndColours = new List<SlotsAndColours>();

		public bool LockColor
		{
			get
			{
				return _lockColor;
			}
		}

		public DropLocation DropLocation
		{
			get
			{
				return _dropLocation;
			}
		}

		public bool Hidden
		{
			get
			{
				return _hidden;
			}
		}

		public bool Invariant
		{
			get
			{
				return _invariant;
			}
		}

		public bool Contains(string SkinName)
		{
			foreach (CharacterSkin item in Skin)
			{
				if (item.Skin == SkinName)
				{
					return true;
				}
			}
			return false;
		}

		public string GetName(CharacterSkin skin)
		{
			return skin.Skin;
		}

		private void AddNewColours()
		{
			SlotsAndColours slotsAndColours = new SlotsAndColours();
			slotsAndColours.SlotAndColours.Add(new SlotAndColor("HEAD_SKIN_TOP", Color.white));
			slotsAndColours.SlotAndColours.Add(new SlotAndColor("HEAD_SKIN_BTM", Color.white));
			slotsAndColours.SlotAndColours.Add(new SlotAndColor("MARKINGS", Color.white));
			slotsAndColours.SlotAndColours.Add(new SlotAndColor("ARM_LEFT_SKIN", Color.white));
			slotsAndColours.SlotAndColours.Add(new SlotAndColor("ARM_RIGHT_SKIN", Color.white));
			slotsAndColours.SlotAndColours.Add(new SlotAndColor("LEG_LEFT_SKIN", Color.white));
			slotsAndColours.SlotAndColours.Add(new SlotAndColor("LEG_RIGHT_SKIN", Color.white));
			SlotAndColours.Add(slotsAndColours);
		}
	}

	[Serializable]
	public class CharacterSkin
	{
		[SpineSkin("", "SkeletonData", true, false, false)]
		public string Skin;

		private void Test()
		{
			Instance.SkeletonData.gameObject.SetActive(true);
			Instance.SkeletonData.skeleton.SetSkin(Skin);
		}
	}

	[Serializable]
	public class SlotsAndColours
	{
		public List<SlotAndColor> SlotAndColours = new List<SlotAndColor>();

		public Color AllColor = Color.white;

		private void AddNewColours()
		{
			SlotAndColours.Add(new SlotAndColor("", AllColor));
		}

		private void SetAll()
		{
			foreach (SlotAndColor slotAndColour in SlotAndColours)
			{
				slotAndColour.color = AllColor;
			}
		}

		private void Test()
		{
			Instance.SkeletonData.gameObject.SetActive(true);
			foreach (SlotAndColor slotAndColour in SlotAndColours)
			{
				Slot slot = Instance.SkeletonData.skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
		}

		private void AddAllSlots()
		{
			SlotAndColours.Add(new SlotAndColor("HEAD_SKIN_TOP", Color.white));
			SlotAndColours.Add(new SlotAndColor("HEAD_SKIN_BTM", Color.white));
			SlotAndColours.Add(new SlotAndColor("MARKINGS", Color.white));
			SlotAndColours.Add(new SlotAndColor("ARM_LEFT_SKIN", Color.white));
			SlotAndColours.Add(new SlotAndColor("ARM_RIGHT_SKIN", Color.white));
			SlotAndColours.Add(new SlotAndColor("LEG_LEFT_SKIN", Color.white));
			SlotAndColours.Add(new SlotAndColor("LEG_RIGHT_SKIN", Color.white));
		}
	}

	[Serializable]
	public class SlotAndColor
	{
		[SpineSlot("", "SkeletonData", false, true, false)]
		public string Slot;

		public Color color = Color.white;

		public SlotAndColor(string Slot, Color color)
		{
			this.Slot = Slot;
			this.color = color;
		}
	}

	[Serializable]
	public class Character
	{
		public string skinName;

		public bool premium;

		public int colorOptionIndex;

		public string HEAD_SKIN_TOP;

		public string HEAD_SKIN_BTM;

		public string ARM_LEFT_SKIN;

		public string ARM_RIGHT_SKIN;

		public string LEG_LEFT_SKIN;

		public string LEG_RIGHT_SKIN;

		public string MARKINGS;
	}

	public SkeletonAnimation SkeletonData;

	public List<SlotsAndColours> GlobalColourList = new List<SlotsAndColours>();

	public List<SkinAndData> Characters = new List<SkinAndData>();

	private static WorshipperData _Instance;

	public static WorshipperData Instance
	{
		get
		{
			if (Application.isPlaying)
			{
				if (_Instance == null)
				{
					_Instance = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Worshipper Data")) as GameObject).GetComponent<WorshipperData>();
				}
				return _Instance;
			}
			return _Instance = UnityEngine.Object.FindObjectOfType<WorshipperData>();
		}
		set
		{
			_Instance = value;
		}
	}

	private void Awake()
	{
		foreach (SkinAndData character in Characters)
		{
			character.StartingSlotAndColours = new List<SlotsAndColours>(character.SlotAndColours);
			character.SlotAndColours.AddRange(GlobalColourList);
		}
	}

	public int GetSkinIndexFromName(string skinName)
	{
		foreach (SkinAndData character in Characters)
		{
			if (character.Skin[0].Skin == skinName)
			{
				return Characters.IndexOf(character);
			}
		}
		return 0;
	}

	public string GetSkinNameFromIndex(int index)
	{
		return Characters[index].Skin[0].Skin;
	}

	public int GetRandomAvailableSkin(bool includeBlacklist = false, bool includeBosses = false, bool includeInvariant = false)
	{
		List<string> list = new List<string>();
		foreach (string item in DataManager.Instance.FollowerSkinsUnlocked)
		{
			if ((includeBosses || !item.Contains("Boss")) && (includeBlacklist || !DataManager.OnBlackList(item)))
			{
				list.Add(item);
			}
		}
		string text = list[UnityEngine.Random.Range(0, list.Count)];
		int num = -1;
		while (++num < Characters.Count)
		{
			if (Characters[num].Title == text && (includeInvariant || !Characters[num].Invariant))
			{
				return num;
			}
		}
		return 0;
	}

	public string GetRandomAvailableSkinName()
	{
		List<string> list = new List<string>();
		foreach (string item in DataManager.Instance.FollowerSkinsUnlocked)
		{
			if (!item.Contains("Boss"))
			{
				list.Add(item);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public int GetRandomSkinAny()
	{
		List<string> list = DataManager.AvailableSkins();
		string text = list[UnityEngine.Random.Range(0, list.Count)];
		int num = -1;
		while (++num < Characters.Count)
		{
			if (Characters[num].Title == text)
			{
				return num;
			}
		}
		return 0;
	}

	private void OnEnable()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		Instance = this;
	}

	private void Start()
	{
		int num = -1;
		while (++num < base.transform.childCount)
		{
			base.transform.GetChild(num).gameObject.SetActive(false);
		}
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public List<SkinAndData> GetCharacters()
	{
		return Characters;
	}

	public SkinAndData GetCharacters(string SkinName)
	{
		foreach (SkinAndData character in Characters)
		{
			if (character.Contains(SkinName))
			{
				return character;
			}
		}
		return null;
	}

	public SkinAndData GetColourData(string Skin)
	{
		foreach (SkinAndData character in Characters)
		{
			if (character.Contains(Skin))
			{
				return character;
			}
		}
		return null;
	}

	public List<SkinAndData> GetSkinsFromFollowerLocation(FollowerLocation location)
	{
		switch (location)
		{
		case FollowerLocation.Base:
		case FollowerLocation.HubShore:
		case FollowerLocation.Dungeon1_1:
		case FollowerLocation.IntroDungeon:
			return GetSkinsFromLocation(DropLocation.Dungeon1);
		case FollowerLocation.Dungeon1_2:
		case FollowerLocation.Sozo_Cave:
			return GetSkinsFromLocation(DropLocation.Dungeon2);
		case FollowerLocation.Dungeon1_3:
		case FollowerLocation.Dungeon_Decoration_Shop1:
			return GetSkinsFromLocation(DropLocation.Dungeon3);
		case FollowerLocation.Dungeon1_4:
		case FollowerLocation.Dungeon_Location_3:
			return GetSkinsFromLocation(DropLocation.Dungeon4);
		default:
			return Characters;
		}
	}

	public List<SkinAndData> GetSkinsFromLocation(DropLocation dropLocation, bool ignoreHidden = false)
	{
		List<SkinAndData> list = new List<SkinAndData>();
		foreach (SkinAndData character in Characters)
		{
			if (!character.Invariant && (ignoreHidden || DataManager.GetFollowerSkinUnlocked(character.Skin[0].Skin) || !character.Hidden) && character.DropLocation == dropLocation)
			{
				list.Add(character);
			}
		}
		return list;
	}

	public List<SkinAndData> GetSkinsAll(bool ignoreHidden = false)
	{
		List<SkinAndData> list = new List<SkinAndData>();
		foreach (SkinAndData character in Characters)
		{
			if (!character.Invariant && (ignoreHidden || DataManager.GetFollowerSkinUnlocked(character.Skin[0].Skin) || !character.Hidden))
			{
				list.Add(character);
			}
		}
		return list;
	}

	public string GetSkinsLocationString(SkinAndData skin)
	{
		switch (skin.DropLocation)
		{
		case DropLocation.Other:
			return ScriptLocalization.UI_Generic.General;
		case DropLocation.Dungeon1:
			return ScriptLocalization.NAMES_Places.Dungeon1_1;
		case DropLocation.Dungeon2:
			return ScriptLocalization.NAMES_Places.Dungeon1_2;
		case DropLocation.Dungeon3:
			return ScriptLocalization.NAMES_Places.Dungeon1_3;
		case DropLocation.Dungeon4:
			return ScriptLocalization.NAMES_Places.Dungeon1_4;
		default:
			Debug.Log("Couldn't find drop location: " + skin.DropLocation);
			return "";
		}
	}

	public void ExportCSV()
	{
		string path = Application.persistentDataPath + "/CotL Skin Variations.csv";
		TextWriter textWriter = new StreamWriter(path, false);
		textWriter.WriteLine("skinName, premium, colorOptionIndex, HEAD_SKIN_TOP, HEAD_SKIN_BTM, ARM_LEFT_SKIN, ARM_RIGHT_SKIN, LEG_LEFT_SKIN, LEG_RIGHT_SKIN, MARKINGS");
		textWriter.Close();
		textWriter = new StreamWriter(path, true);
		foreach (SkinAndData character in Characters)
		{
			List<SlotsAndColours> list = new List<SlotsAndColours>(character.SlotAndColours);
			list.AddRange(GlobalColourList);
			for (int i = 0; i < list.Count; i++)
			{
				string text = "";
				text = text + character.Skin[0].Skin + ",";
				text = text + (character.TwitchPremium ? "TRUE" : "FALSE") + ",";
				text = text + i + ",";
				Color colorFromSlot = GetColorFromSlot(list[i].SlotAndColours, "HEAD_SKIN_TOP");
				Color colorFromSlot2 = GetColorFromSlot(list[i].SlotAndColours, "HEAD_SKIN_BTM");
				Color colorFromSlot3 = GetColorFromSlot(list[i].SlotAndColours, "ARM_LEFT_SKIN");
				Color colorFromSlot4 = GetColorFromSlot(list[i].SlotAndColours, "ARM_RIGHT_SKIN");
				Color colorFromSlot5 = GetColorFromSlot(list[i].SlotAndColours, "LEG_LEFT_SKIN");
				Color colorFromSlot6 = GetColorFromSlot(list[i].SlotAndColours, "LEG_RIGHT_SKIN");
				Color colorFromSlot7 = GetColorFromSlot(list[i].SlotAndColours, "MARKINGS");
				text = text + string.Format("\"{0}, {1}, {2}\"", Mathf.RoundToInt(colorFromSlot.r * 255f).ToString(), Mathf.RoundToInt(colorFromSlot.g * 255f).ToString(), Mathf.RoundToInt(colorFromSlot.b * 255f).ToString()) + ",";
				text = text + string.Format("\"{0}, {1}, {2}\"", Mathf.RoundToInt(colorFromSlot2.r * 255f).ToString(), Mathf.RoundToInt(colorFromSlot2.g * 255f).ToString(), Mathf.RoundToInt(colorFromSlot2.b * 255f).ToString()) + ",";
				text = text + string.Format("\"{0}, {1}, {2}\"", Mathf.RoundToInt(colorFromSlot3.r * 255f).ToString(), Mathf.RoundToInt(colorFromSlot3.g * 255f).ToString(), Mathf.RoundToInt(colorFromSlot3.b * 255f).ToString()) + ",";
				text = text + string.Format("\"{0}, {1}, {2}\"", Mathf.RoundToInt(colorFromSlot4.r * 255f).ToString(), Mathf.RoundToInt(colorFromSlot4.g * 255f).ToString(), Mathf.RoundToInt(colorFromSlot4.b * 255f).ToString()) + ",";
				text = text + string.Format("\"{0}, {1}, {2}\"", Mathf.RoundToInt(colorFromSlot5.r * 255f).ToString(), Mathf.RoundToInt(colorFromSlot5.g * 255f).ToString(), Mathf.RoundToInt(colorFromSlot5.b * 255f).ToString()) + ",";
				text = text + string.Format("\"{0}, {1}, {2}\"", Mathf.RoundToInt(colorFromSlot6.r * 255f).ToString(), Mathf.RoundToInt(colorFromSlot6.g * 255f).ToString(), Mathf.RoundToInt(colorFromSlot6.b * 255f).ToString()) + ",";
				text = text + string.Format("\"{0}, {1}, {2}\"", Mathf.RoundToInt(colorFromSlot7.r * 255f).ToString(), Mathf.RoundToInt(colorFromSlot7.g * 255f).ToString(), Mathf.RoundToInt(colorFromSlot7.b * 255f).ToString()) + ",";
				textWriter.WriteLine(text);
			}
		}
		textWriter.Close();
	}

	private static Color GetColorFromSlot(List<SlotAndColor> SlotAndColours, string slot)
	{
		for (int i = 0; i < SlotAndColours.Count; i++)
		{
			if (SlotAndColours[i].Slot == slot)
			{
				return SlotAndColours[i].color;
			}
		}
		return Color.blue;
	}
}
