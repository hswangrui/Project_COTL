using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI.BuildMenu
{
	public abstract class BuildMenuCategory : UISubmenuBase
	{
		public Action<StructureBrain.TYPES> OnBuildingChosen;

		[Header("Build Menu Category")]
		[SerializeField]
		protected MMScrollRect _scrollRect;

		[SerializeField]
		protected BuildMenuItem _buildMenuItemTemplate;

		protected List<BuildMenuItem> _buildItems = new List<BuildMenuItem>();

		public MMScrollRect ScrollRect
		{
			get
			{
				return _scrollRect;
			}
		}

		public List<BuildMenuItem> BuildItems
		{
			get
			{
				return _buildItems;
			}
		}

		protected override void OnShowStarted()
		{
			_scrollRect.normalizedPosition = Vector2.one;
			if (_buildItems.Count == 0)
			{
				_scrollRect.enabled = false;
				Populate();
				_scrollRect.enabled = true;
			}
			if (_buildItems.Count > 0)
			{
				OverrideDefault(_buildItems[0].Button);
				ActivateNavigation();
			}
		}

		protected abstract void Populate();

		protected void Populate(List<StructureBrain.TYPES> types, RectTransform contentContainer)
		{
			List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
			List<StructureBrain.TYPES> list2 = new List<StructureBrain.TYPES>();
			foreach (StructureBrain.TYPES type in types)
			{
				if (StructuresData.GetUnlocked(type))
				{
					list.Add(type);
				}
				else if (!StructuresData.HiddenUntilUnlocked(type))
				{
					list2.Add(type);
				}
			}
			foreach (StructureBrain.TYPES item in list)
			{
				BuildMenuItem buildMenuItem = GameObjectExtensions.Instantiate(_buildMenuItemTemplate, contentContainer);
				buildMenuItem.Configure(item);
				buildMenuItem.OnStructureSelected = (Action<StructureBrain.TYPES>)Delegate.Combine(buildMenuItem.OnStructureSelected, new Action<StructureBrain.TYPES>(ChosenBuilding));
				_buildItems.Add(buildMenuItem);
			}
			foreach (StructureBrain.TYPES item2 in list2)
			{
				BuildMenuItem buildMenuItem2 = GameObjectExtensions.Instantiate(_buildMenuItemTemplate, contentContainer);
				buildMenuItem2.Configure(item2);
				_buildItems.Add(buildMenuItem2);
			}
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_buildItems.Count == 0)
			{
				yield return _003C_003En__0();
			}
			_canvasGroup.alpha = 1f;
			yield return null;
		}

		private void ChosenBuilding(StructureBrain.TYPES structure)
		{
			Action<StructureBrain.TYPES> onBuildingChosen = OnBuildingChosen;
			if (onBuildingChosen != null)
			{
				onBuildingChosen(structure);
			}
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.DoShowAnimation();
		}
	}
}
