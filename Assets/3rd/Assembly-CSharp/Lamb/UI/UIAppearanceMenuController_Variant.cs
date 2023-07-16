using System;
using System.Collections.Generic;
using System.Linq;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIAppearanceMenuController_Variant : UIMenuBase
	{
		public Action<int> OnVariantChanged;

		[Header("Variant")]
		[SerializeField]
		private RectTransform _variantContent;

		[SerializeField]
		private MMButton _randomiseVariant;

		private Follower _follower;

		private int _cachedVariant;

		private List<IndoctrinationVariantItem> _variantItems = new List<IndoctrinationVariantItem>();

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
			_randomiseVariant.onClick.AddListener(ChooseRandomVariant);
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			_cachedVariant = _follower.Brain.Info.SkinVariation;
			WorshipperData.SkinAndData skinAndData = WorshipperData.Instance.Characters[_follower.Brain.Info.SkinCharacter];
			if (_variantItems.Count > 0)
			{
				foreach (IndoctrinationVariantItem variantItem in _variantItems)
				{
					UnityEngine.Object.Destroy(variantItem.gameObject);
				}
				_variantItems.Clear();
			}
			foreach (WorshipperData.CharacterSkin item in skinAndData.Skin)
			{
				IndoctrinationVariantItem indoctrinationVariantItem = MonoSingleton<UIManager>.Instance.FollowerVariantItemTemplate.Spawn(_variantContent);
				indoctrinationVariantItem.transform.localScale = Vector3.one;
				indoctrinationVariantItem.Configure(skinAndData.Skin.IndexOf(item), _follower.Brain.Info.SkinColour, skinAndData);
				indoctrinationVariantItem.OnItemSelected = (Action<IndoctrinationVariantItem>)Delegate.Combine(indoctrinationVariantItem.OnItemSelected, new Action<IndoctrinationVariantItem>(OnVariantItemSelected));
				_variantItems.Add(indoctrinationVariantItem);
			}
			UpdateVariantSelection(_follower.Brain.Info.SkinVariation);
			OverrideDefaultOnce(_variantItems[_follower.Brain.Info.SkinVariation].Button);
			ActivateNavigation();
		}

		protected override void OnHideStarted()
		{
			ApplyCachedSettings();
		}

		public void ApplyCachedSettings()
		{
			Action<int> onVariantChanged = OnVariantChanged;
			if (onVariantChanged != null)
			{
				onVariantChanged(_cachedVariant);
			}
		}

		private void ChooseRandomVariant()
		{
			List<IndoctrinationVariantItem> list = _variantItems.Where((IndoctrinationVariantItem i) => !i.Locked).ToList();
			list.Remove(_variantItems[_follower.Brain.Info.SkinVariation]);
			if (list.Count > 0)
			{
				OnVariantItemSelected(list.RandomElement());
			}
		}

		private void OnVariantItemSelected(IndoctrinationVariantItem variantItem)
		{
			int num = (_cachedVariant = _variantItems.IndexOf(variantItem));
			Action<int> onVariantChanged = OnVariantChanged;
			if (onVariantChanged != null)
			{
				onVariantChanged(num);
			}
			UpdateVariantSelection(num);
			Hide();
		}

		private void UpdateVariantSelection(int selection)
		{
			for (int i = 0; i < _variantItems.Count; i++)
			{
				if (i == selection)
				{
					_variantItems[i].SetAsSelected();
				}
				else
				{
					_variantItems[i].SetAsDefault();
				}
			}
		}

		private void OnSelection(Selectable current)
		{
			IndoctrinationVariantItem component;
			if (current.TryGetComponent<IndoctrinationVariantItem>(out component))
			{
				Action<int> onVariantChanged = OnVariantChanged;
				if (onVariantChanged != null)
				{
					onVariantChanged(_variantItems.IndexOf(component));
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
