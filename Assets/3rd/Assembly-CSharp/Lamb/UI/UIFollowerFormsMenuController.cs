using System.Collections.Generic;
using I2.Loc;
using src.Extensions;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UIFollowerFormsMenuController : UIMenuBase
	{
		[Header("Follower Forms menu")]
		[SerializeField]
		private IndoctrinationFormItem _formItemTemplate;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[Header("Content")]
		[SerializeField]
		private RectTransform _miscContent;

		[SerializeField]
		private RectTransform _specialEventsContent;

		[SerializeField]
		private RectTransform _dlcContent;

		[SerializeField]
		private RectTransform _darkwoodContent;

		[SerializeField]
		private RectTransform _anuraContent;

		[SerializeField]
		private RectTransform _anchorDeepContent;

		[SerializeField]
		private RectTransform _silkCradleContent;

		[Header("Counts")]
		[SerializeField]
		private TextMeshProUGUI _miscUnlocked;

		[SerializeField]
		private TextMeshProUGUI _specialEventsUnlocked;

		[SerializeField]
		private TextMeshProUGUI _dlcUnlocked;

		[SerializeField]
		private TextMeshProUGUI _darkwoodUnlocked;

		[SerializeField]
		private TextMeshProUGUI _anuraUnlocked;

		[SerializeField]
		private TextMeshProUGUI _anchorDeepUnlocked;

		[SerializeField]
		private TextMeshProUGUI _silkCradleUnlocked;

		[Header("Headers")]
		[SerializeField]
		private GameObject _dlcHeader;

		[SerializeField]
		private GameObject _specialEventsHeader;

		private List<IndoctrinationFormItem> _formItems = new List<IndoctrinationFormItem>();

		public static readonly string[] DLCOrder = new string[20]
		{
			"Cthulhu", "Bee", "Tapir", "Turtle", "Monkey", "Narwal", "Moose", "Gorilla", "Mosquito", "Goldfish",
			"Possum", "TwitchMouse", "TwitchCat", "TwitchDog", "TwitchDogAlt", "TwitchPoggers", "Lion", "Penguin", "Pelican", "Kiwi"
		};

		public static readonly string[] MiscOrder = new string[21]
		{
			"Deer", "Pig", "Dog", "Cat", "Fox", "Night Wolf", "Fish", "Pangolin", "Shrew", "Unicorn",
			"Axolotl", "Starfish", "Red Panda", "Poop", "Massive Monster", "Crab", "Snail", "Owl", "Butterfly", "Koala",
			"Shrimp"
		};

		public static readonly string[] DarkwoodOrder = new string[12]
		{
			"Cow", "Horse", "Deer_ritual", "Hedgehog", "Rabbit", "Chicken", "Squirrel", "Boss Mama Worm", "Boss Mama Maggot", "Boss Burrow Worm",
			"Boss Beholder 1", "CultLeader 1"
		};

		public static readonly string[] AnuraOrder = new string[12]
		{
			"Giraffe", "Bison", "Frog", "Capybara", "Fennec Fox", "Rhino", "Eagle", "Boss Flying Burp Frog", "Boss Egg Hopper", "Boss Mortar Hopper",
			"Boss Beholder 2", "CultLeader 2"
		};

		public static readonly string[] AnchordeepOrder = new string[11]
		{
			"Crocodile", "Elephant", "Hippo", "Otter", "Seahorse", "Duck", "Boss Spiker", "Boss Charger", "Boss Scuttle Turret", "Boss Beholder 3",
			"CultLeader 3"
		};

		public static readonly string[] SilkCradleOrder = new string[10] { "Bear", "Bat", "Beetle", "Raccoon", "Badger", "Boss Spider Jump", "Boss Millipede Poisoner", "Boss Scorpion", "Boss Beholder 4", "CultLeader 4" };

		public static readonly string[] SpecialEventsOrder = new string[4] { "Crow", "DeerSkull", "BatDemon", "StarBunny" };

		protected override void OnShowStarted()
		{
			UIManager.PlayAudio("event:/followers/appearance_menu_appear");
			_scrollRect.normalizedPosition = Vector2.one;
			_scrollRect.enabled = false;
			if (_formItems.Count == 0)
			{
				_scrollRect.enabled = false;
				bool active = false;
				foreach (string dLCSkin in DataManager.Instance.DLCSkins)
				{
					if (DataManager.GetFollowerSkinUnlocked(dLCSkin))
					{
						active = true;
						break;
					}
				}
				_dlcContent.gameObject.SetActive(active);
				_dlcHeader.SetActive(active);
				bool active2 = false;
				string[] specialEventSkins = DataManager.Instance.SpecialEventSkins;
				for (int i = 0; i < specialEventSkins.Length; i++)
				{
					if (DataManager.GetFollowerSkinUnlocked(specialEventSkins[i]))
					{
						active2 = true;
						break;
					}
				}
				_specialEventsContent.gameObject.SetActive(active2);
				_specialEventsHeader.SetActive(active2);
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Other), _miscContent, MiscOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.SpecialEvents), _specialEventsContent, SpecialEventsOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Dungeon1), _darkwoodContent, DarkwoodOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Dungeon2), _anuraContent, AnuraOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Dungeon3), _anchorDeepContent, AnchordeepOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Dungeon4), _silkCradleContent, SilkCradleOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.DLC), _dlcContent, DLCOrder));
				SetUnlockedText(_miscUnlocked, WorshipperData.DropLocation.Other);
				SetUnlockedText(_specialEventsUnlocked, WorshipperData.DropLocation.SpecialEvents);
				SetUnlockedText(_darkwoodUnlocked, WorshipperData.DropLocation.Dungeon1);
				SetUnlockedText(_anuraUnlocked, WorshipperData.DropLocation.Dungeon2);
				SetUnlockedText(_anchorDeepUnlocked, WorshipperData.DropLocation.Dungeon3);
				SetUnlockedText(_silkCradleUnlocked, WorshipperData.DropLocation.Dungeon4);
				SetUnlockedText(_dlcUnlocked, WorshipperData.DropLocation.DLC);
				foreach (IndoctrinationFormItem formItem in _formItems)
				{
					formItem.SetAsDefault();
					formItem.Button.OnConfirmDenied = null;
					formItem.Button.Confirmable = false;
					formItem.Button._confirmDeniedSFX = "";
				}
			}
			_scrollRect.enabled = true;
			OverrideDefaultOnce(_formItems[0].Button);
			ActivateNavigation();
		}

		private List<IndoctrinationFormItem> Populate(List<WorshipperData.SkinAndData> types, RectTransform contentContainer, string[] order = null)
		{
			if (order != null)
			{
				types.Sort((WorshipperData.SkinAndData a, WorshipperData.SkinAndData b) => order.IndexOf(a.Title).CompareTo(order.IndexOf(b.Title)));
			}
			List<IndoctrinationFormItem> result = new List<IndoctrinationFormItem>();
			foreach (WorshipperData.SkinAndData type in types)
			{
				IndoctrinationFormItem indoctrinationFormItem = GameObjectExtensions.Instantiate(_formItemTemplate, contentContainer);
				indoctrinationFormItem.Configure(type);
				indoctrinationFormItem.Button.Confirmable = false;
				_formItems.Add(indoctrinationFormItem);
			}
			return result;
		}

		private void SetUnlockedText(TextMeshProUGUI target, WorshipperData.DropLocation dropLocation)
		{
			int num = 0;
			List<WorshipperData.SkinAndData> skinsFromLocation = WorshipperData.Instance.GetSkinsFromLocation(dropLocation);
			foreach (WorshipperData.SkinAndData item in skinsFromLocation)
			{
				if (DataManager.GetFollowerSkinUnlocked(item.Skin[0].Skin))
				{
					num++;
				}
			}
			target.text = string.Format(LocalizationManager.GetTranslation("UI/Collected"), string.Format("{0}/{1}", num, skinsFromLocation.Count));
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			_scrollRect.enabled = false;
			UIManager.PlayAudio("event:/upgrade_statue/upgrade_statue_close");
		}

		protected override void OnHideCompleted()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
