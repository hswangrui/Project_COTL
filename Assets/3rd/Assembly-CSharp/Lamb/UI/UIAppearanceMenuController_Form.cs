using System;
using System.Collections.Generic;
using I2.Loc;
using src.UINavigator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIAppearanceMenuController_Form : UIMenuBase
	{
		public Action<int> OnFormChanged;

		[Header("Forms Menu")]
		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private Transform _lockIcon;

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

		[Header("Randomisation")]
		[SerializeField]
		private MMButton _randomiseButtonMisc;

		[SerializeField]
		private MMButton _randomiseButtonSpecialEvents;

		[SerializeField]
		private MMButton _randomiseButtonDLC;

		[SerializeField]
		private MMButton _randomiseButtonDungeon1;

		[SerializeField]
		private MMButton _randomiseButtonDungeon2;

		[SerializeField]
		private MMButton _randomiseButtonDungeon3;

		[SerializeField]
		private MMButton _randomiseButtonDungeon4;

		[Header("Headers")]
		[SerializeField]
		private GameObject _dlcHeader;

		[SerializeField]
		private GameObject _specialEventsHeader;

		private List<IndoctrinationFormItem> _formItems = new List<IndoctrinationFormItem>();

		private Follower _follower;

		private int _cachedForm;

		public override void Awake()
		{
			_lockIcon.gameObject.SetActive(false);
			base.Awake();
		}

		public void Show(Follower follower, bool instant = false)
		{
			_follower = follower;
			_randomiseButtonMisc.onClick.AddListener(delegate
			{
				ChooseRandomForm(WorshipperData.DropLocation.Other);
			});
			_randomiseButtonSpecialEvents.onClick.AddListener(delegate
			{
				ChooseRandomForm(WorshipperData.DropLocation.SpecialEvents);
			});
			_randomiseButtonDLC.onClick.AddListener(delegate
			{
				ChooseRandomForm(WorshipperData.DropLocation.DLC);
			});
			_randomiseButtonDungeon1.onClick.AddListener(delegate
			{
				ChooseRandomForm(WorshipperData.DropLocation.Dungeon1);
			});
			_randomiseButtonDungeon2.onClick.AddListener(delegate
			{
				ChooseRandomForm(WorshipperData.DropLocation.Dungeon2);
			});
			_randomiseButtonDungeon3.onClick.AddListener(delegate
			{
				ChooseRandomForm(WorshipperData.DropLocation.Dungeon3);
			});
			_randomiseButtonDungeon4.onClick.AddListener(delegate
			{
				ChooseRandomForm(WorshipperData.DropLocation.Dungeon4);
			});
			Show(instant);
		}

		private void OnEnable()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance2.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
		}

		private void OnDisable()
		{
			if (MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance2.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
			}
		}

		protected override void OnShowStarted()
		{
			_cachedForm = _follower.Brain.Info.SkinCharacter;
			_scrollRect.normalizedPosition = Vector2.one;
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
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Other), _miscContent, UIFollowerFormsMenuController.MiscOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.SpecialEvents), _specialEventsContent, UIFollowerFormsMenuController.SpecialEventsOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.DLC), _dlcContent, UIFollowerFormsMenuController.DLCOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Dungeon1), _darkwoodContent, UIFollowerFormsMenuController.DarkwoodOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Dungeon2), _anuraContent, UIFollowerFormsMenuController.AnuraOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Dungeon3), _anchorDeepContent, UIFollowerFormsMenuController.AnchordeepOrder));
				_formItems.AddRange(Populate(WorshipperData.Instance.GetSkinsFromLocation(WorshipperData.DropLocation.Dungeon4), _silkCradleContent, UIFollowerFormsMenuController.SilkCradleOrder));
				SetUnlockedText(_miscUnlocked, WorshipperData.DropLocation.Other);
				SetUnlockedText(_specialEventsUnlocked, WorshipperData.DropLocation.SpecialEvents);
				SetUnlockedText(_dlcUnlocked, WorshipperData.DropLocation.DLC);
				SetUnlockedText(_darkwoodUnlocked, WorshipperData.DropLocation.Dungeon1);
				SetUnlockedText(_anuraUnlocked, WorshipperData.DropLocation.Dungeon2);
				SetUnlockedText(_anchorDeepUnlocked, WorshipperData.DropLocation.Dungeon3);
				SetUnlockedText(_silkCradleUnlocked, WorshipperData.DropLocation.Dungeon4);
				_scrollRect.enabled = true;
			}
			WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[_follower.Brain.Info.SkinCharacter];
			foreach (IndoctrinationFormItem formItem in _formItems)
			{
				if (formItem.Skin == skinAndData.Skin[0].Skin)
				{
					OverrideDefaultOnce(formItem.Button);
					UpdateSelection(formItem);
					break;
				}
			}
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
				IndoctrinationFormItem indoctrinationFormItem = MonoSingleton<UIManager>.Instance.FollowerFormItemTemplate.Spawn(contentContainer);
				indoctrinationFormItem.transform.localScale = Vector3.one;
				indoctrinationFormItem.Configure(type);
				indoctrinationFormItem.OnItemSelected = (Action<IndoctrinationFormItem>)Delegate.Combine(indoctrinationFormItem.OnItemSelected, new Action<IndoctrinationFormItem>(OnFormSelected));
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
				if (!item.Invariant && DataManager.GetFollowerSkinUnlocked(item.Skin[0].Skin))
				{
					num++;
				}
			}
			target.text = string.Format(LocalizationManager.GetTranslation("UI/Collected"), string.Format("{0}/{1}", num, skinsFromLocation.Count));
		}

		protected override void OnHideStarted()
		{
			_scrollRect.enabled = false;
			ApplyCachedSettings();
		}

		public void ApplyCachedSettings()
		{
			Action<int> onFormChanged = OnFormChanged;
			if (onFormChanged != null)
			{
				onFormChanged(_cachedForm);
			}
			_lockIcon.gameObject.SetActive(false);
		}

		private void ChooseRandomForm(WorshipperData.DropLocation dropLocation)
		{
			List<IndoctrinationFormItem> list = new List<IndoctrinationFormItem>();
			foreach (IndoctrinationFormItem formItem in _formItems)
			{
				if (!formItem.Locked && formItem.DropLocation == dropLocation && formItem.Skin != _follower.Brain.Info.SkinName.StripNumbers())
				{
					list.Add(formItem);
				}
			}
			if (list.Count > 0)
			{
				OnFormSelected(list.RandomElement());
			}
		}

		private void OnFormSelected(IndoctrinationFormItem formItem)
		{
			_cachedForm = GetFormItemIndex(formItem);
			UpdateSelection(formItem);
			Action<int> onFormChanged = OnFormChanged;
			if (onFormChanged != null)
			{
				onFormChanged(_cachedForm);
			}
			Hide();
		}

		private void UpdateSelection(IndoctrinationFormItem formItem)
		{
			foreach (IndoctrinationFormItem formItem2 in _formItems)
			{
				if (formItem2 == formItem)
				{
					formItem2.SetAsSelected();
				}
				else
				{
					formItem2.SetAsDefault();
				}
			}
		}

		private void OnSelection(Selectable current)
		{
			IndoctrinationFormItem component;
			if (current.TryGetComponent<IndoctrinationFormItem>(out component))
			{
				_lockIcon.gameObject.SetActive(component.Locked);
				Action<int> onFormChanged = OnFormChanged;
				if (onFormChanged != null)
				{
					onFormChanged(GetFormItemIndex(component));
				}
			}
		}

		private void OnSelectionChanged(Selectable current, Selectable previous)
		{
			OnSelection(current);
		}

		private int GetFormItemIndex(IndoctrinationFormItem formItem)
		{
			foreach (WorshipperData.SkinAndData character in WorshipperData.Instance.Characters)
			{
				if (formItem.Skin == character.Skin[0].Skin)
				{
					return WorshipperData.Instance.Characters.IndexOf(character);
				}
			}
			return -1;
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
