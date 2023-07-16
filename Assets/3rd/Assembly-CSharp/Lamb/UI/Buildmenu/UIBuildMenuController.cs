using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using src.UI.InfoCards;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI.BuildMenu
{
	public class UIBuildMenuController : UIMenuBase
	{
		[Serializable]
		public enum Category
		{
			Follower,
			Faith,
			Aesthetic
		}

		public Action<StructureBrain.TYPES> OnBuildingChosen;

		[Header("Build Menu")]
		[SerializeField]
		private BuildMenuTabNavigatorBase _tabNavigatorBase;

		[SerializeField]
		private FollowerCategory _followerCategory;

		[SerializeField]
		private FaithCategory _faithCategory;

		[SerializeField]
		private AestheticCategory _aestheticCategory;

		[SerializeField]
		private BuildInfoCardController _infoCardController;

		[Header("Prompts")]
		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private GameObject _editBuildingsText;

		private bool _didCancel;

		private StructureBrain.TYPES _revealingStructure;

		public override void Awake()
		{
			base.Awake();
			DataManager.Instance.Alerts.Structures.CheckStructureUnlocked();
			FollowerCategory followerCategory = _followerCategory;
			followerCategory.OnBuildingChosen = (Action<StructureBrain.TYPES>)Delegate.Combine(followerCategory.OnBuildingChosen, new Action<StructureBrain.TYPES>(ChosenBuilding));
			FaithCategory faithCategory = _faithCategory;
			faithCategory.OnBuildingChosen = (Action<StructureBrain.TYPES>)Delegate.Combine(faithCategory.OnBuildingChosen, new Action<StructureBrain.TYPES>(ChosenBuilding));
			AestheticCategory aestheticCategory = _aestheticCategory;
			aestheticCategory.OnBuildingChosen = (Action<StructureBrain.TYPES>)Delegate.Combine(aestheticCategory.OnBuildingChosen, new Action<StructureBrain.TYPES>(ChosenBuilding));
		}

		public void Show(StructureBrain.TYPES structureToReveal)
		{
			_revealingStructure = structureToReveal;
			if (FollowerCategory.AllStructures().Contains(structureToReveal))
			{
				_tabNavigatorBase.DefaultTabIndex = 0;
			}
			else if (FaithCategory.AllStructures().Contains(structureToReveal))
			{
				_tabNavigatorBase.DefaultTabIndex = 1;
			}
			else if (AestheticCategory.AllStructures().Contains(structureToReveal))
			{
				_tabNavigatorBase.DefaultTabIndex = 2;
			}
			_tabNavigatorBase.ClearPersistentTab();
			Show();
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_revealingStructure != 0)
			{
				_controlPrompts.HideAcceptButton();
				_controlPrompts.HideCancelButton();
				_editBuildingsText.SetActive(false);
				_tabNavigatorBase.RemoveAllAlerts();
				_tabNavigatorBase.SetNavigationVisibility(false);
				BuildMenuCategory buildMenuCategory2;
				BuildMenuCategory buildMenuCategory = (buildMenuCategory2 = _tabNavigatorBase.CurrentMenu as BuildMenuCategory);
				if ((object)buildMenuCategory2 == null)
				{
					yield break;
				}
				BuildMenuItem target = null;
				foreach (BuildMenuItem buildItem in buildMenuCategory.BuildItems)
				{
					if (buildItem.Structure == _revealingStructure)
					{
						target = buildItem;
						target.ForceLockedState();
					}
					else if (!buildItem.Locked)
					{
						buildItem.ForceIncognitoState();
					}
				}
				buildMenuCategory.ScrollRect.vertical = false;
				MonoSingleton<UINavigatorNew>.Instance.Clear();
				SetActiveStateForMenu(false);
				yield return _003C_003En__0();
				yield return new WaitForSecondsRealtime(0.1f);
				UIManager.PlayAudio("event:/sermon/scroll_sermon_menu");
				yield return buildMenuCategory.ScrollRect.DoScrollTo(target.RectTransform);
				yield return new WaitForSecondsRealtime(0.1f);
				yield return target.DoUnlock();
				yield return new WaitForSecondsRealtime(0.1f);
				_infoCardController.ShowCardWithParam(target.Structure);
				yield return new WaitForSecondsRealtime(0.1f);
				_controlPrompts.ShowAcceptButton();
				while (!InputManager.UI.GetAcceptButtonDown())
				{
					yield return null;
				}
				_controlPrompts.HideAcceptButton();
				Hide();
			}
			else
			{
				yield return _003C_003En__0();
			}
		}

		private void ChosenBuilding(StructureBrain.TYPES structure)
		{
			Action<StructureBrain.TYPES> onBuildingChosen = OnBuildingChosen;
			if (onBuildingChosen != null)
			{
				onBuildingChosen(structure);
			}
			Hide();
		}

		private void Update()
		{
			if (_canvasGroup.interactable && DataManager.Instance.HasBuiltShrine1 && DataManager.Instance.HasBuiltTemple1 && InputManager.UI.GetEditBuildingsButtonDown())
			{
				Action<StructureBrain.TYPES> onBuildingChosen = OnBuildingChosen;
				if (onBuildingChosen != null)
				{
					onBuildingChosen(StructureBrain.TYPES.EDIT_BUILDINGS);
				}
				Hide();
			}
			_editBuildingsText.SetActive(DataManager.Instance.HasBuiltShrine1 && DataManager.Instance.HasBuiltTemple1 && _revealingStructure == StructureBrain.TYPES.NONE);
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				_didCancel = true;
				Hide();
			}
		}

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			UIManager.PlayAudio("event:/ui/open_menu");
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
			_followerCategory.CanvasGroup.interactable = false;
			_faithCategory.CanvasGroup.interactable = false;
			_aestheticCategory.CanvasGroup.interactable = false;
		}

		protected override void OnHideCompleted()
		{
			if (_didCancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			Interactor.CurrentInteraction = null;
			Interactor.PreviousInteraction = null;
			UnityEngine.Object.Destroy(base.gameObject);
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.DoShowAnimation();
		}
	}
}
