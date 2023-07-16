using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class SandboxCategory : UISubmenuBase
	{
		public enum TransitionType
		{
			Scale,
			MoveLeft,
			MoveRight
		}

		private const float kTransitionDuration = 0.35f;

		public Action<PlayerFleeceManager.FleeceType> OnFleeceSelected;

		public Action<ScenarioData> OnScenarioChosen;

		[Header("Scenario")]
		[SerializeField]
		private DungeonSandboxManager.ScenarioType _scenarioType;

		[SerializeField]
		private TMP_Text _scenarioHeader;

		[SerializeField]
		private TMP_Text _scenarioDescription;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		public Color _backgroundColor;

		[SerializeField]
		private RewardPath[] _rewardPaths;

		[SerializeField]
		private GameObject _dotsContainer;

		[Header("Fleeces")]
		[SerializeField]
		private TMP_Text _fleeceHeader;

		[SerializeField]
		private TMP_Text _fleeceDescription;

		[SerializeField]
		private TMP_Text _fleeceLocked;

		[Header("XP Bar")]
		[SerializeField]
		private UIGodTearBar xpbar;

		private TransitionType _transitionType;

		private void Start()
		{
			_scenarioHeader.text = LocalizationManager.GetTranslation(string.Format("UI/SandboxMenu/{0}", _scenarioType));
			_scenarioDescription.text = LocalizationManager.GetTranslation(string.Format("UI/SandboxMenu/{0}/Description", _scenarioType));
			_fleeceHeader.text = string.Empty;
			_fleeceDescription.text = string.Empty;
			_fleeceLocked.gameObject.SetActive(false);
			xpbar.gameObject.SetActive(false);
			RewardPath[] rewardPaths = _rewardPaths;
			foreach (RewardPath obj in rewardPaths)
			{
				obj.OnSelected = (Action<PlayerFleeceManager.FleeceType>)Delegate.Combine(obj.OnSelected, (Action<PlayerFleeceManager.FleeceType>)delegate(PlayerFleeceManager.FleeceType fleece)
				{
					Action<PlayerFleeceManager.FleeceType> onFleeceSelected = OnFleeceSelected;
					if (onFleeceSelected != null)
					{
						onFleeceSelected(fleece);
					}
					if (DataManager.Instance.UnlockedFleeces.Contains((int)fleece))
					{
						xpbar.gameObject.SetActive(true);
						xpbar.Show(DataManager.Instance.CurrentChallengeModeXP);
						_fleeceHeader.gameObject.SetActive(true);
						_fleeceDescription.gameObject.SetActive(true);
						_fleeceLocked.gameObject.SetActive(false);
						_fleeceHeader.text = LocalizationManager.GetTranslation(string.Format("TarotCards/Fleece{0}/Name", (int)fleece));
						_fleeceDescription.text = LocalizationManager.GetTranslation(string.Format("TarotCards/Fleece{0}/Description", (int)fleece));
						if (SettingsManager.Settings.Accessibility.DyslexicFont)
						{
							Vector2 sizeDelta = _fleeceDescription.rectTransform.sizeDelta;
							sizeDelta.x = 840f;
							_fleeceDescription.rectTransform.sizeDelta = sizeDelta;
						}
					}
					else
					{
						xpbar.gameObject.SetActive(true);
						xpbar.Hide();
						_fleeceHeader.gameObject.SetActive(false);
						_fleeceDescription.gameObject.SetActive(false);
						_fleeceLocked.gameObject.SetActive(true);
					}
				});
				obj.OnConfirmed = (Action<ScenarioData>)Delegate.Combine(obj.OnConfirmed, (Action<ScenarioData>)delegate(ScenarioData scenario)
				{
					Action<ScenarioData> onScenarioChosen = OnScenarioChosen;
					if (onScenarioChosen != null)
					{
						onScenarioChosen(scenario);
					}
				});
				obj.Configure(DungeonSandboxManager.GetDataForScenarioType(_scenarioType));
			}
		}

		public void SetIncognitoMode()
		{
			RewardPath[] rewardPaths = _rewardPaths;
			for (int i = 0; i < rewardPaths.Length; i++)
			{
				rewardPaths[i].SetIncognitoMode();
			}
		}

		public void RemoveIncognitoMode()
		{
			RewardPath[] rewardPaths = _rewardPaths;
			for (int i = 0; i < rewardPaths.Length; i++)
			{
				rewardPaths[i].RemoveIncognitoMode();
			}
		}

		public void ShowDots()
		{
			_dotsContainer.SetActive(true);
		}

		public void HideDots()
		{
			_dotsContainer.SetActive(false);
		}

		public void Show(TransitionType transitionType, bool immediate = false)
		{
			_transitionType = transitionType;
			Show(immediate);
		}

		public void Hide(TransitionType transitionType, bool immediate = false)
		{
			_transitionType = transitionType;
			Hide(immediate);
		}

		protected override IEnumerator DoShowAnimation()
		{
			_canvasGroup.DOKill();
			_rectTransform.DOKill();
			_canvasGroup.alpha = 0f;
			_canvasGroup.DOFade(1f, 0.35f).SetUpdate(true);
			if (_transitionType == TransitionType.MoveLeft)
			{
				_rectTransform.localPosition = new Vector2(100f, 0f);
				_rectTransform.DOLocalMove(Vector3.zero, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
			}
			else if (_transitionType == TransitionType.MoveRight)
			{
				_rectTransform.localPosition = new Vector2(-100f, 0f);
				_rectTransform.DOLocalMove(Vector3.zero, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
			}
			else
			{
				_rectTransform.localScale = Vector2.one * 1.15f;
				_rectTransform.DOScale(1f, 0.35f).SetUpdate(true);
			}
			yield return new WaitForSecondsRealtime(0.35f);
		}

		protected override IEnumerator DoHideAnimation()
		{
			_canvasGroup.DOKill();
			_rectTransform.DOKill();
			_canvasGroup.DOFade(0f, 0.35f).SetUpdate(true);
			if (_transitionType == TransitionType.MoveLeft)
			{
				_rectTransform.localPosition = Vector2.zero;
				_rectTransform.DOLocalMove(new Vector2(-100f, 0f), 0.35f).SetUpdate(true).SetEase(Ease.InSine);
			}
			else if (_transitionType == TransitionType.MoveRight)
			{
				_rectTransform.localPosition = Vector2.zero;
				_rectTransform.DOLocalMove(new Vector2(100f, 0f), 0.35f).SetUpdate(true).SetEase(Ease.InSine);
			}
			else
			{
				_rectTransform.DOScale(0.85f, 0.35f).SetUpdate(true);
			}
			yield return new WaitForSecondsRealtime(0.35f);
		}

		public void SetCurrentSelectable(SandboxFleeceItem sandboxFleeceItem)
		{
			SetCurrentSelectable((PlayerFleeceManager.FleeceType)sandboxFleeceItem.FleeceIndex);
		}

		public void SetCurrentSelectable(PlayerFleeceManager.FleeceType fleeceType)
		{
			RewardPath[] rewardPaths = _rewardPaths;
			foreach (RewardPath rewardPath in rewardPaths)
			{
				if (rewardPath.Fleece == fleeceType)
				{
					OverrideDefault(rewardPath.FleeceItem.Button);
					return;
				}
			}
			OverrideDefault(_rewardPaths[0].FleeceItem.Button);
		}

		public void Activate()
		{
			ActivateNavigation();
		}
	}
}
