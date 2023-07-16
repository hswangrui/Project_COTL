using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using src.Extensions;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI.Rituals
{
	public class UIRitualsMenuController : UIMenuBase
	{
		public Action<UpgradeSystem.Type> OnRitualSelected;

		[Header("Rituals Menu")]
		[SerializeField]
		private RitualInfoCardController _infoCardController;

		[SerializeField]
		private RitualItem _ritualItemTemplate;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private RectTransform _ritualsContent;

		[Header("Special Rituals")]
		[SerializeField]
		private GameObject _specialRitualsContainer;

		[SerializeField]
		private RitualItem _specialRitualItem;

		private List<RitualItem> _ritualItems = new List<RitualItem>();

		private bool _didCancel;

		private UpgradeSystem.Type _showRitual = UpgradeSystem.Type.Count;

		public void Show(UpgradeSystem.Type showRitual, bool instant = false)
		{
			_showRitual = showRitual;
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			_specialRitualsContainer.SetActive(false);
			UIManager.PlayAudio("event:/ui/open_menu");
			UpgradeSystem.Type[] secondaryRituals = UpgradeSystem.SecondaryRituals;
			foreach (UpgradeSystem.Type ritual in secondaryRituals)
			{
				AddRitual(ritual);
			}
			UpgradeSystem.Type[] secondaryRitualPairs = UpgradeSystem.SecondaryRitualPairs;
			for (int j = 0; j < secondaryRitualPairs.Length; j += 2)
			{
				if (CheatConsole.UnlockAllRituals || DataManager.Instance.OnboardedCrystalDoctrine)
				{
					AddRitual(secondaryRitualPairs[j]);
					AddRitual(secondaryRitualPairs[j + 1]);
				}
				else if (UpgradeSystem.GetUnlocked(secondaryRitualPairs[j]))
				{
					AddRitual(secondaryRitualPairs[j]);
				}
				else
				{
					AddRitual(secondaryRitualPairs[j + 1]);
				}
			}
			secondaryRituals = UpgradeSystem.SingleRituals;
			foreach (UpgradeSystem.Type type in secondaryRituals)
			{
				if (UpgradeSystem.GetUnlocked(type) && !UpgradeSystem.IsSpecialRitual(type))
				{
					AddRitual(type);
				}
			}
			secondaryRituals = UpgradeSystem.SpecialRituals;
			foreach (UpgradeSystem.Type type2 in secondaryRituals)
			{
				if (UpgradeSystem.GetUnlocked(type2))
				{
					ConfigureItem(_specialRitualItem, type2);
					_specialRitualsContainer.SetActive(true);
					break;
				}
			}
			if (_specialRitualItem.gameObject.activeInHierarchy)
			{
				OverrideDefault(_specialRitualItem.Button);
			}
			else
			{
				OverrideDefault(_ritualItems[0].Button);
			}
			ActivateNavigation();
		}

		private void ConfigureItem(RitualItem ritualItem, UpgradeSystem.Type ritual)
		{
			ritualItem.Configure(ritual);
			ritualItem.OnRitualItemSelected = (Action<UpgradeSystem.Type>)Delegate.Combine(ritualItem.OnRitualItemSelected, new Action<UpgradeSystem.Type>(RitualItemSelected));
			_ritualItems.Add(ritualItem);
		}

		private void AddRitual(UpgradeSystem.Type ritual)
		{
			if (ritual != UpgradeSystem.Type.Ritual_Blank)
			{
				ConfigureItem(GameObjectExtensions.Instantiate(_ritualItemTemplate, _ritualsContent), ritual);
			}
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_showRitual != UpgradeSystem.Type.Count)
			{
				_controlPrompts.HideAcceptButton();
				_controlPrompts.HideCancelButton();
				RitualItem target = null;
				foreach (RitualItem ritualItem in _ritualItems)
				{
					if (ritualItem.RitualType == _showRitual)
					{
						target = ritualItem;
					}
					else if (!ritualItem.Locked)
					{
						ritualItem.ForceIncognitoState();
					}
				}
				target.ForceLockedState();
				MonoSingleton<UINavigatorNew>.Instance.Clear();
				SetActiveStateForMenu(false);
				yield return _003C_003En__0();
				yield return new WaitForSecondsRealtime(0.1f);
				yield return target.DoUnlock();
				yield return new WaitForSecondsRealtime(0.1f);
				_infoCardController.ShowCardWithParam(target.RitualType);
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

		private void RitualItemSelected(UpgradeSystem.Type ritual)
		{
			Action<UpgradeSystem.Type> onRitualSelected = OnRitualSelected;
			if (onRitualSelected != null)
			{
				onRitualSelected(ritual);
			}
			Hide();
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				_didCancel = true;
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
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
