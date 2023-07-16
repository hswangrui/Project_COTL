using System;
using System.Collections;
using DG.Tweening;
using Spine.Unity;
using src.Extensions;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UISandboxMenuController : UIMenuBase
	{
		public Action<ScenarioData> OnScenarioChosen;

		[Header("Main Content")]
		[SerializeField]
		private RectTransform _lambContainer;

		[SerializeField]
		private SkeletonGraphic _lambGraphic;

		[SerializeField]
		private SandboxCategory[] _categories;

		[SerializeField]
		private Image _backgroundImage;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[SerializeField]
		private Image _background;

		[Header("Buttons")]
		[SerializeField]
		private MMButton _leftTab;

		[SerializeField]
		private GameObject _leftTabAlert;

		[SerializeField]
		private MMButton _rightTab;

		[SerializeField]
		private GameObject _rightTabAlert;

		[Header("XP Bar")]
		[SerializeField]
		private RectTransform _xpBarTransform;

		[Header("Incognito")]
		[SerializeField]
		private CanvasGroup _incognitoLower;

		[SerializeField]
		private CanvasGroup _incognitoUpper;

		private SandboxCategory _currentMenu;

		private bool _bossRushAvailable;

		private bool _onboardingBossRush;

		private Material _backgroundMaterial;

		public override void Awake()
		{
			base.Awake();
			_onboardingBossRush = DungeonSandboxManager.GetCompletedRunCount() > 0 && !DataManager.Instance.OnboardedBossRush;
			_incognitoLower.alpha = 0f;
			_incognitoUpper.alpha = 0f;
			_backgroundMaterial = new Material(_background.material);
			_background.material = _backgroundMaterial;
			int selectedFleece = 0;
			SandboxCategory[] categories = _categories;
			foreach (SandboxCategory sandboxCategory in categories)
			{
				if (_onboardingBossRush || DataManager.Instance.OnboardedBossRush)
				{
					sandboxCategory.ShowDots();
				}
				else
				{
					sandboxCategory.HideDots();
				}
				sandboxCategory.OnFleeceSelected = (Action<PlayerFleeceManager.FleeceType>)Delegate.Combine(sandboxCategory.OnFleeceSelected, (Action<PlayerFleeceManager.FleeceType>)delegate(PlayerFleeceManager.FleeceType fleece)
				{
					selectedFleece = (int)fleece;
					if (DataManager.Instance.UnlockedFleeces.Contains((int)fleece))
					{
						_controlPrompts.ShowAcceptButton();
						_lambGraphic.Skeleton.SetSkin(string.Format("Lamb_{0}", (int)fleece));
					}
					else
					{
						_lambGraphic.Skeleton.SetSkin("Lamb_0");
						_controlPrompts.HideAcceptButton();
					}
				});
				sandboxCategory.OnScenarioChosen = (Action<ScenarioData>)Delegate.Combine(sandboxCategory.OnScenarioChosen, (Action<ScenarioData>)delegate(ScenarioData scenario)
				{
					scenario.FleeceType = selectedFleece;
					Action<ScenarioData> onScenarioChosen = OnScenarioChosen;
					if (onScenarioChosen != null)
					{
						onScenarioChosen(scenario);
					}
					Hide();
				});
			}
			_currentMenu = _categories[0];
			_canvasGroup.alpha = 0f;
			_rightTab.interactable = DataManager.Instance.OnboardedBossRush;
			_leftTab.interactable = DataManager.Instance.OnboardedBossRush;
			_leftTab.gameObject.SetActive(DataManager.Instance.OnboardedBossRush);
			_rightTab.gameObject.SetActive(DataManager.Instance.OnboardedBossRush);
			_rightTab.onClick.AddListener(NavigatePageRight);
			_leftTab.onClick.AddListener(NavigatePageLeft);
			_rightTabAlert.SetActive(false);
			_leftTabAlert.SetActive(false);
		}

		private void OnEnable()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnPageNavigateLeft = (Action)Delegate.Combine(instance.OnPageNavigateLeft, new Action(NavigatePageLeft));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnPageNavigateRight = (Action)Delegate.Combine(instance2.OnPageNavigateRight, new Action(NavigatePageRight));
			CheckTabsShouldBeVisible();
			if (SettingsManager.Settings.Accessibility.DyslexicFont)
			{
				_xpBarTransform.anchoredPosition = new Vector2(0f, -50f);
			}
		}

		private void OnDisable()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnPageNavigateLeft = (Action)Delegate.Remove(instance.OnPageNavigateLeft, new Action(NavigatePageLeft));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnPageNavigateRight = (Action)Delegate.Remove(instance2.OnPageNavigateRight, new Action(NavigatePageRight));
		}

		protected override IEnumerator DoShowAnimation()
		{
			_canvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
			Vector3 localScale = _lambContainer.localScale;
			_lambContainer.localScale = localScale * 1.5f;
			_lambContainer.DOScale(localScale, 0.5f).SetUpdate(true);
			_lambGraphic.SetAnimation("sermons/doctrine-loop", true);
			_currentMenu.Show();
			if (_onboardingBossRush)
			{
				yield return null;
				SetActiveStateForMenu(false);
				MonoSingleton<UINavigatorNew>.Instance.Clear();
				_controlPrompts.HideAcceptButton();
				_controlPrompts.HideCancelButton();
				Vector2 rightOrigin = _rightTab.transform.localPosition;
				Vector2 leftOrigin = _leftTab.transform.localPosition;
				_rightTab.transform.localPosition += new Vector3(200f, 0f, 0f);
				_leftTab.transform.localPosition -= new Vector3(200f, 0f, 0f);
				SandboxCategory[] categories = _categories;
				for (int i = 0; i < categories.Length; i++)
				{
					categories[i].SetIncognitoMode();
				}
				_incognitoUpper.alpha = 0.65f;
				yield return _currentMenu.YieldUntilShown();
				yield return new WaitForSecondsRealtime(0.25f);
				_rightTab.gameObject.SetActive(true);
				_rightTab.transform.DOLocalMove(rightOrigin, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
				yield return new WaitForSecondsRealtime(0.5f);
				yield return new WaitForSecondsRealtime(0.1f);
				_rightTabAlert.transform.localScale = Vector3.zero;
				_rightTabAlert.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
				_rightTabAlert.gameObject.SetActive(true);
				UIManager.PlayAudio("event:/ui/glass_ball_ding");
				yield return new WaitForSecondsRealtime(0.5f);
				SetActiveStateForMenu(_rightTab.gameObject, true);
				do
				{
					yield return null;
				}
				while (!InputManager.UI.GetPageNavigateRightDown());
				_rightTab.interactable = false;
				_rightTab.gameObject.SetActive(false);
				_rightTabAlert.SetActive(false);
				DoPageNavigateRight();
				yield return _currentMenu.YieldUntilShown();
				_leftTab.gameObject.SetActive(true);
				_leftTab.transform.DOLocalMove(leftOrigin, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
				yield return new WaitForSecondsRealtime(0.5f);
				yield return new WaitForSecondsRealtime(0.1f);
				_leftTabAlert.transform.localScale = Vector3.zero;
				_leftTabAlert.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
				_leftTabAlert.gameObject.SetActive(true);
				UIManager.PlayAudio("event:/ui/glass_ball_ding");
				yield return new WaitForSecondsRealtime(0.5f);
				categories = _categories;
				for (int i = 0; i < categories.Length; i++)
				{
					categories[i].RemoveIncognitoMode();
				}
				_incognitoUpper.DOFade(0f, 0.25f).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.25f);
				_leftTab.interactable = true;
				_rightTab.interactable = true;
				_onboardingBossRush = false;
				_controlPrompts.ShowAcceptButton();
				_controlPrompts.ShowCancelButton();
				_currentMenu.SetCurrentSelectable((PlayerFleeceManager.FleeceType)DataManager.Instance.PlayerFleece);
				_currentMenu.Activate();
				SetActiveStateForMenu(true);
				DataManager.Instance.OnboardedBossRush = true;
			}
			else
			{
				_currentMenu.SetCurrentSelectable((PlayerFleeceManager.FleeceType)DataManager.Instance.PlayerFleece);
			}
			yield return new WaitForSecondsRealtime(0.5f);
		}

		private void CheckTabsShouldBeVisible()
		{
			if (DataManager.Instance.OnboardedBossRush)
			{
				_leftTab.gameObject.SetActive(true);
				_rightTab.gameObject.SetActive(true);
				if (_categories.IndexOf(_currentMenu) == 0)
				{
					_leftTab.gameObject.SetActive(false);
				}
				if (_categories.IndexOf(_currentMenu) == _categories.Length - 1)
				{
					_rightTab.gameObject.SetActive(false);
				}
			}
		}

		private void NavigatePageLeft()
		{
			if (_leftTab.interactable)
			{
				DoNavigatePageLeft();
			}
		}

		private void DoNavigatePageLeft()
		{
			_leftTabAlert.SetActive(false);
			_leftTab.gameObject.transform.DOKill();
			_leftTab.gameObject.transform.DOShakePosition(1f, new Vector3(10f, 0f));
			if (_categories.IndexOf(_currentMenu) == 0)
			{
				UIManager.PlayAudio("event:/ui/negative_feedback");
			}
			else
			{
				UIManager.PlayAudio("event:/ui/conversation_change_page");
				PerformMenuTransition(_currentMenu, _categories[_categories.IndexOf(_currentMenu) - 1], SandboxCategory.TransitionType.MoveLeft);
			}
			CheckTabsShouldBeVisible();
		}

		private void NavigatePageRight()
		{
			if (_rightTab.interactable)
			{
				DoPageNavigateRight();
			}
		}

		private void DoPageNavigateRight()
		{
			_rightTabAlert.SetActive(false);
			_rightTab.gameObject.transform.DOKill();
			_rightTab.gameObject.transform.DOShakePosition(1f, new Vector3(10f, 0f));
			if (_categories.IndexOf(_currentMenu) == _categories.Length - 1)
			{
				UIManager.PlayAudio("event:/ui/negative_feedback");
			}
			else
			{
				PerformMenuTransition(_currentMenu, _categories[_categories.IndexOf(_currentMenu) + 1], SandboxCategory.TransitionType.MoveRight);
				UIManager.PlayAudio("event:/ui/conversation_change_page");
			}
			CheckTabsShouldBeVisible();
		}

		private void PerformMenuTransition(SandboxCategory from, SandboxCategory to, SandboxCategory.TransitionType transitionType)
		{
			_currentMenu = to;
			if (MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable != null)
			{
				to.SetCurrentSelectable(MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable.Selectable.GetComponent<SandboxFleeceItem>());
			}
			from.Hide(transitionType);
			to.Show(transitionType);
			_backgroundMaterial.DOKill();
			_backgroundImage.DOKill();
			_backgroundMaterial.DOColor(to._backgroundColor, "_Color1", 0.5f).SetUpdate(true);
			_backgroundImage.DOColor(to._backgroundColor, 0.5f).SetUpdate(true);
		}

		private void EndSelection()
		{
			Hide();
		}

		protected override IEnumerator DoHideAnimation()
		{
			_canvasGroup.DOFade(0f, 0.5f).SetUpdate(true);
			yield return new WaitForSeconds(1f);
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
