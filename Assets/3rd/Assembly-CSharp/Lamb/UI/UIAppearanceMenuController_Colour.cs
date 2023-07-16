using System;
using System.Collections.Generic;
using System.Linq;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIAppearanceMenuController_Colour : UIMenuBase
	{
		public Action<int> OnColourChanged;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[Header("Colour")]
		[SerializeField]
		private RectTransform _colourContent;

		[SerializeField]
		private MMButton _randomiseColour;

		private Follower _follower;

		private int _cachedColour;

		private List<IndoctrinationColourItem> _colourItems = new List<IndoctrinationColourItem>();

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

		public void Show(Follower follower, bool instant = false)
		{
			_follower = follower;
			_randomiseColour.onClick.AddListener(ChooseRandomColour);
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			_cachedColour = _follower.Brain.Info.SkinColour;
			_scrollRect.normalizedPosition = Vector2.one;
			_scrollRect.enabled = false;
			WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[_follower.Brain.Info.SkinCharacter];
			if (_colourItems.Count > 0)
			{
				foreach (IndoctrinationColourItem colourItem in _colourItems)
				{
					UnityEngine.Object.Destroy(colourItem.gameObject);
				}
				_colourItems.Clear();
			}
			List<WorshipperData.SlotsAndColours> list = new List<WorshipperData.SlotsAndColours>();
			list.AddRange(skinAndData.SlotAndColours);
			foreach (WorshipperData.SlotsAndColours item in list)
			{
				IndoctrinationColourItem indoctrinationColourItem = MonoSingleton<UIManager>.Instance.FollowerColourItemTemplate.Spawn(_colourContent);
				indoctrinationColourItem.transform.localScale = Vector3.one;
				indoctrinationColourItem.Configure(_follower.Brain.Info.SkinVariation, skinAndData.SlotAndColours.IndexOf(item), skinAndData);
				indoctrinationColourItem.OnItemSelected = (Action<IndoctrinationColourItem>)Delegate.Combine(indoctrinationColourItem.OnItemSelected, new Action<IndoctrinationColourItem>(OnColourItemSelected));
				_colourItems.Add(indoctrinationColourItem);
			}
			UpdateColourSelection(_follower.Brain.Info.SkinColour);
			OverrideDefaultOnce(_colourItems[_follower.Brain.Info.SkinColour].Button);
			ActivateNavigation();
			_scrollRect.enabled = true;
		}

		protected override void OnHideStarted()
		{
			ApplyCachedSettings();
		}

		public void ApplyCachedSettings()
		{
			Action<int> onColourChanged = OnColourChanged;
			if (onColourChanged != null)
			{
				onColourChanged(_cachedColour);
			}
		}

		private void ChooseRandomColour()
		{
			List<IndoctrinationColourItem> list = _colourItems.Where((IndoctrinationColourItem i) => !i.Locked).ToList();
			list.Remove(_colourItems[_follower.Brain.Info.SkinColour]);
			if (list.Count > 0)
			{
				OnColourItemSelected(list.RandomElement());
			}
		}

		private void OnColourItemSelected(IndoctrinationColourItem colourItem)
		{
			int num = (_cachedColour = _colourItems.IndexOf(colourItem));
			Action<int> onColourChanged = OnColourChanged;
			if (onColourChanged != null)
			{
				onColourChanged(num);
			}
			UpdateColourSelection(num);
			Hide();
		}

		private void UpdateColourSelection(int selection)
		{
			for (int i = 0; i < _colourItems.Count; i++)
			{
				if (i == selection)
				{
					_colourItems[i].SetAsSelected();
				}
				else
				{
					_colourItems[i].SetAsDefault();
				}
			}
		}

		private void OnSelection(Selectable current)
		{
			IndoctrinationColourItem component;
			if (current.TryGetComponent<IndoctrinationColourItem>(out component))
			{
				Action<int> onColourChanged = OnColourChanged;
				if (onColourChanged != null)
				{
					onColourChanged(_colourItems.IndexOf(component));
				}
			}
		}

		private void OnSelectionChanged(Selectable current, Selectable previous)
		{
			OnSelection(current);
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
