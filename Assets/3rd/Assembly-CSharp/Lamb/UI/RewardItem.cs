using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class RewardItem : MonoBehaviour
	{
		private enum RewardState
		{
			Locked,
			Unlocked,
			Obtained,
			Hidden
		}

		[SerializeField]
		private RectTransform _container;

		[SerializeField]
		private GameObject _dropShadow;

		[Header("Selected")]
		[SerializeField]
		private CanvasGroup _selectedContainerCanvasGroup;

		[SerializeField]
		private RectTransform _selectedContainer;

		[SerializeField]
		private GameObject _tick;

		[Header("Available")]
		[SerializeField]
		private GameObject _availableContainer;

		[Header("Unlocked")]
		[SerializeField]
		private GameObject _godTear;

		[SerializeField]
		private GameObject _unlockedContainer;

		[Header("Locked")]
		[SerializeField]
		private GameObject _lockedContainer;

		[Header("Incognito")]
		[SerializeField]
		private Image _godTearImage;

		[SerializeField]
		private GameObject _incognitoContainer;

		private RewardState _state;

		private MMUILineRenderer.Branch _progressionBranch;

		private Tween _lineTween;

		public void Configure(ScenarioData scenarioData, DungeonSandboxManager.ProgressionSnapshot progressionSnapshot, bool left)
		{
			_selectedContainerCanvasGroup.alpha = 0f;
			RemoveIncognitoMode();
			_tick.SetActive(false);
			if (DataManager.Instance.UnlockedFleeces.Contains((int)progressionSnapshot.FleeceType))
			{
				if (progressionSnapshot.FleeceType != 0 && !DungeonSandboxManager.HasFinishedAnyWithDefaultFleece())
				{
					SetAsHidden();
				}
				else if (progressionSnapshot.CompletedRuns == scenarioData.ScenarioIndex)
				{
					SetAsAvailable();
				}
				else if (progressionSnapshot.CompletedRuns > scenarioData.ScenarioIndex)
				{
					SetAsUnlocked();
				}
				else
				{
					SetAsHidden();
				}
			}
			else if (scenarioData.ScenarioIndex == 0 && DungeonSandboxManager.HasFinishedAnyWithDefaultFleece())
			{
				SetAsLocked();
			}
			else
			{
				SetAsHidden();
			}
			if (left)
			{
				_container.pivot = new Vector2(1f, 0.5f);
			}
			else
			{
				_container.pivot = new Vector2(0f, 0.5f);
			}
		}

		public void ConfigureLine(MMUILineRenderer.Branch branch)
		{
			if (_state == RewardState.Locked)
			{
				branch.Color = StaticColors.DarkGreyColor;
			}
			else if (_state == RewardState.Unlocked)
			{
				branch.Color = StaticColors.RedColor;
			}
			else if (_state == RewardState.Hidden)
			{
				branch.Color = new Color(1f, 1f, 1f, 0f);
			}
			else
			{
				branch.Color = StaticColors.GreenColor;
			}
		}

		public void ConfigureSecondaryLine(MMUILineRenderer.Branch branch)
		{
			_progressionBranch = branch;
			_progressionBranch.Color = StaticColors.OffWhiteColor;
			_progressionBranch.Fill = 0f;
		}

		public void SetAsLocked()
		{
			_state = RewardState.Locked;
			_availableContainer.SetActive(false);
			_unlockedContainer.SetActive(false);
			_lockedContainer.SetActive(true);
		}

		private void SetAsAvailable()
		{
			_state = RewardState.Unlocked;
			_availableContainer.SetActive(true);
			_unlockedContainer.SetActive(false);
			_lockedContainer.SetActive(false);
		}

		private void SetAsHidden()
		{
			_dropShadow.SetActive(false);
			_state = RewardState.Hidden;
			_godTear.SetActive(false);
			_availableContainer.SetActive(false);
			_unlockedContainer.SetActive(false);
			_lockedContainer.SetActive(false);
		}

		private void SetAsUnlocked()
		{
			_state = RewardState.Obtained;
			_availableContainer.SetActive(false);
			_unlockedContainer.SetActive(true);
			_lockedContainer.SetActive(false);
			_godTear.SetActive(false);
			_tick.SetActive(true);
		}

		public void SetIncognitoMode()
		{
			if (_state != RewardState.Hidden)
			{
				_godTearImage.color = new Color(0f, 1f, 1f, 1f);
				_incognitoContainer.SetActive(true);
			}
		}

		public void RemoveIncognitoMode()
		{
			_godTearImage.color = new Color(1f, 1f, 1f, 1f);
			_incognitoContainer.SetActive(false);
		}

		public void Highlight()
		{
			if (_lineTween != null)
			{
				_lineTween.Kill();
			}
			_lineTween = DOTween.To(() => _progressionBranch.Fill, delegate(float f)
			{
				_progressionBranch.Fill = f;
			}, 0.65f, 0.15f).SetEase(Ease.Linear).SetUpdate(true);
			_container.DOKill();
			_container.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true)
				.SetDelay(0.15f);
			_selectedContainerCanvasGroup.DOKill();
			_selectedContainerCanvasGroup.DOFade(1f, 0.3f).SetEase(Ease.Linear).SetUpdate(true)
				.SetDelay(0.15f);
		}

		public void UnHighlight()
		{
			_container.DOKill();
			_container.DOScale(1f, 0.3f).SetEase(Ease.InBack).SetUpdate(true);
			_selectedContainerCanvasGroup.DOKill();
			_selectedContainerCanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.Linear).SetUpdate(true);
			if (_lineTween != null)
			{
				_lineTween.Kill();
			}
			_lineTween = DOTween.To(() => _progressionBranch.Fill, delegate(float f)
			{
				_progressionBranch.Fill = f;
			}, 0f, 0.15f).SetDelay(0.3f).SetEase(Ease.Linear)
				.SetUpdate(true);
		}
	}
}
