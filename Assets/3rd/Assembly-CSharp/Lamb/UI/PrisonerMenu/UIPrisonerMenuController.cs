using System;
using DG.Tweening;
using I2.Loc;
using Spine.Unity;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.PrisonerMenu
{
	public class UIPrisonerMenuController : UIMenuBase
	{
		public Action<FollowerInfo> OnFollowerReleased;

		public Action<FollowerInfo> OnReEducate;

		[Header("Prisoner Menu")]
		[SerializeField]
		private TextMeshProUGUI _nameText;

		[SerializeField]
		private SkeletonGraphic _followerSkeleton;

		[Header("Buttons")]
		[SerializeField]
		private MMButton _readMindButton;

		[SerializeField]
		private MMButton _releaseButton;

		[SerializeField]
		private MMButton _reeducateButton;

		[SerializeField]
		private TextMeshProUGUI _reeducateButtonText;

		[Header("Stats")]
		[SerializeField]
		private Image _faithProgressBar;

		[SerializeField]
		private Image _faithProgressBarWhite;

		[SerializeField]
		private TextMeshProUGUI _durationText;

		[Header("Dissenter")]
		[SerializeField]
		private TextMeshProUGUI _dissenterText;

		[SerializeField]
		private GameObject _dissentingContainer;

		[SerializeField]
		private TextMeshProUGUI _noLongerDissentingText;

		[SerializeField]
		private GameObject _noLongerDissentingContainer;

		[SerializeField]
		private GameObject _wasNotDissentingContainer;

		[SerializeField]
		private GameObject _wasNotDissentingOriginalSinContainer;

		[Header("Other")]
		[SerializeField]
		private GameObject _reeducateAlert;

		[SerializeField]
		private RectTransform _reeducatedTextTransform;

		private FollowerInfo _followerInfo;

		private FollowerBrain _followerBrain;

		private StructuresData _structuresData;

		private bool _didCancel;

		public void Show(FollowerInfo followerInfo, StructuresData structuresData, bool instant = false)
		{
			_followerInfo = followerInfo;
			_structuresData = structuresData;
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			_followerBrain = FollowerBrain.GetOrCreateBrain(_followerInfo);
			_nameText.text = _followerInfo.Name;
			_followerSkeleton.ConfigureFollower(_followerInfo);
			_followerSkeleton.ConfigurePrison(_followerInfo, _structuresData, true);
			_readMindButton.onClick.AddListener(OnReadMindButtonClicked);
			_releaseButton.onClick.AddListener(OnReleaseClicked);
			_reeducateButton.onClick.AddListener(OnReeducatePressed);
			MMButton reeducateButton = _reeducateButton;
			reeducateButton.OnSelected = (Action)Delegate.Combine(reeducateButton.OnSelected, new Action(OnReeducateSelected));
			MMButton reeducateButton2 = _reeducateButton;
			reeducateButton2.OnDeselected = (Action)Delegate.Combine(reeducateButton2.OnDeselected, new Action(OnReeducateDeselected));
			MMButton reeducateButton3 = _reeducateButton;
			reeducateButton3.OnConfirmDenied = (Action)Delegate.Combine(reeducateButton3.OnConfirmDenied, new Action(ShakeReeducate));
			if (_followerBrain != null)
			{
				_reeducateButton.Confirmable = !_followerBrain.Stats.ReeducatedAction && _followerBrain.Info.CursedState == Thought.Dissenter;
			}
			_reeducateAlert.SetActive(_reeducateButton.Confirmable);
			UpdateStats();
		}

		private void UpdateStats()
		{
			if (_followerBrain != null)
			{
				_dissenterText.text = string.Format(_dissenterText.text, _followerBrain.Info.Name);
				_dissentingContainer.SetActive(_followerBrain.Info.CursedState == Thought.Dissenter);
				_noLongerDissentingText.text = string.Format(_noLongerDissentingText.text, _followerBrain.Info.Name);
				_noLongerDissentingContainer.SetActive(_followerBrain.Info.CursedState != Thought.Dissenter && _followerBrain._directInfoAccess.Reeducation <= 0f);
				_wasNotDissentingContainer.SetActive(!_noLongerDissentingContainer.activeSelf && _followerBrain.Info.CursedState != Thought.Dissenter && !DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Disciplinarian));
				_wasNotDissentingOriginalSinContainer.SetActive(!_noLongerDissentingContainer.activeSelf && _followerBrain.Info.CursedState != Thought.Dissenter && DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Disciplinarian));
				_reeducatedTextTransform.gameObject.SetActive(_followerBrain.Stats.ReeducatedAction);
				if (_followerBrain.Info.CursedState == Thought.Dissenter)
				{
					_faithProgressBar.fillAmount = 1f - _followerBrain.Stats.Reeducation / 100f;
					_faithProgressBarWhite.fillAmount = _faithProgressBar.fillAmount;
				}
				else
				{
					_faithProgressBar.fillAmount = 1f;
				}
				_faithProgressBar.color = StaticColors.ColorForThreshold(_faithProgressBar.fillAmount);
			}
			if (_structuresData != null)
			{
				_durationText.text = GetExpiryFormatted(TimeManager.TotalElapsedGameTime - _structuresData.FollowerImprisonedTimestamp);
			}
		}

		private string GetExpiryFormatted(float timeStamp)
		{
			int num = Mathf.RoundToInt(timeStamp / 1200f);
			return string.Format(ScriptLocalization.UI_Generic.Days, num);
		}

		private void OnReadMindButtonClicked()
		{
			UIFollowerSummaryMenuController followerSummaryMenu = MonoSingleton<UIManager>.Instance.FollowerSummaryMenuTemplate.Instantiate();
			UIFollowerSummaryMenuController uIFollowerSummaryMenuController = followerSummaryMenu;
			uIFollowerSummaryMenuController.OnHidden = (Action)Delegate.Combine(uIFollowerSummaryMenuController.OnHidden, (Action)delegate
			{
				followerSummaryMenu = null;
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.ReadMind);
			});
			followerSummaryMenu.Show(FollowerManager.FindFollowerByID(_followerInfo.ID));
			PushInstance(followerSummaryMenu);
		}

		private void OnReleaseClicked()
		{
			Action<FollowerInfo> onFollowerReleased = OnFollowerReleased;
			if (onFollowerReleased != null)
			{
				onFollowerReleased(_followerInfo);
			}
			Hide();
		}

		private void OnReeducateSelected()
		{
			if (!_followerBrain.Stats.ReeducatedAction)
			{
				_faithProgressBarWhite.DOFillAmount(1f - (_followerBrain.Stats.Reeducation - 25f) / 100f, 0.25f).SetEase(Ease.OutSine).SetUpdate(true);
			}
		}

		private void OnReeducateDeselected()
		{
			if (_followerBrain.Info.CursedState == Thought.Dissenter)
			{
				_faithProgressBarWhite.DOFillAmount(_faithProgressBar.fillAmount, 0.25f).SetEase(Ease.OutSine).SetUpdate(true);
			}
		}

		private void OnReeducatePressed()
		{
			Action<FollowerInfo> onReEducate = OnReEducate;
			if (onReEducate != null)
			{
				onReEducate(_followerInfo);
			}
			UpdateStats();
		}

		private void ShakeReeducate()
		{
			_reeducateButtonText.DOKill();
			_reeducateButtonText.rectTransform.anchoredPosition = Vector2.zero;
			_reeducateButtonText.rectTransform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
			_reeducatedTextTransform.DOKill();
			_reeducatedTextTransform.anchoredPosition = Vector2.zero;
			_reeducatedTextTransform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		public override void OnCancelButtonInput()
		{
			_didCancel = true;
			if (_canvasGroup.interactable)
			{
				Hide();
			}
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
	}
}
