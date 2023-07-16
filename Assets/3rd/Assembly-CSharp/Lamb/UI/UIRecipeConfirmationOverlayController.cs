using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using src.UI.InfoCards;
using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public class UIRecipeConfirmationOverlayController : UIMenuBase
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass15_0
		{
			public UIRecipeConfirmationOverlayController _003C_003E4__this;

			public bool cancel;

			internal void _003CRunMenuConfirmation_003Eg__UpdateProgress_007C0(float progress)
			{
				float num = 1f + 0.25f * progress;
				_003C_003E4__this._infoCard.RectTransform.localScale = new Vector3(num, num, num);
				_003C_003E4__this._infoCard.RectTransform.localPosition = UnityEngine.Random.insideUnitCircle * progress * _003C_003E4__this._holdInteraction.HoldTime * 2f;
				MMVibrate.RumbleContinuous(progress * 0.2f, progress * 0.2f);
			}

			internal void _003CRunMenuConfirmation_003Eg__Cancel_007C1()
			{
				cancel = true;
				MMVibrate.StopRumble();
			}
		}

		public Action OnConfirm;

		[Header("General")]
		[SerializeField]
		private RecipeInfoCard _infoCard;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[Header("Confirm Recipe")]
		[SerializeField]
		private UIHoldInteraction _holdInteraction;

		[SerializeField]
		private GameObject _background;

		[Header("New Recipe")]
		[SerializeField]
		private RectTransform _newRecipeHeaderRect;

		[SerializeField]
		private CanvasGroup _newRecipeCanvasGroup;

		[SerializeField]
		private GameObject _blurBackground;

		private bool _newRecipe;

		private InventoryItem.ITEM_TYPE _recipe;

		private Vector2 _newRecipeOrigin;

		public override void Awake()
		{
			base.Awake();
			_canvasGroup.alpha = 0f;
		}

		public void Show(InventoryItem.ITEM_TYPE recipe, bool newRecipe = false, bool instant = false)
		{
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			_controlPrompts.HideAcceptButton();
			_controlPrompts.HideCancelButton();
			_newRecipe = newRecipe;
			_recipe = recipe;
			if (!newRecipe)
			{
				ShowCard();
			}
			else
			{
				_canvasGroup.alpha = 0f;
			}
			_newRecipeOrigin = _newRecipeHeaderRect.anchoredPosition;
			_newRecipeHeaderRect.anchoredPosition = Vector3.zero;
			_newRecipeCanvasGroup.alpha = 0f;
			_holdInteraction.gameObject.SetActive(!_newRecipe);
			_newRecipeHeaderRect.gameObject.SetActive(newRecipe);
			_blurBackground.SetActive(newRecipe);
			_background.SetActive(!newRecipe);
			Show(instant);
		}

		private void ShowCard()
		{
			_infoCard.Show(_recipe);
			if (_newRecipe)
			{
				_controlPrompts.ShowAcceptButton();
			}
			else
			{
				_controlPrompts.ShowCancelButton();
			}
		}

		protected override void OnShowCompleted()
		{
			if (!_newRecipe)
			{
				StartCoroutine(RunMenuConfirmation());
			}
			else
			{
				StartCoroutine(RunMenuNewRecipe());
			}
		}

		private IEnumerator RunMenuConfirmation()
		{
			_003C_003Ec__DisplayClass15_0 _003C_003Ec__DisplayClass15_ = new _003C_003Ec__DisplayClass15_0();
			_003C_003Ec__DisplayClass15_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass15_.cancel = false;
			yield return _holdInteraction.DoHoldInteraction(_003C_003Ec__DisplayClass15_._003CRunMenuConfirmation_003Eg__UpdateProgress_007C0, _003C_003Ec__DisplayClass15_._003CRunMenuConfirmation_003Eg__Cancel_007C1);
			MMVibrate.StopRumble();
			_infoCard.RectTransform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.3f);
			if (_003C_003Ec__DisplayClass15_.cancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			else
			{
				Action onConfirm = OnConfirm;
				if (onConfirm != null)
				{
					onConfirm();
				}
			}
			Hide();
		}

		private IEnumerator RunMenuNewRecipe()
		{
			UIManager.PlayAudio("event:/ui/map_location_appear");
			_newRecipeHeaderRect.localScale = Vector3.one * 4f;
			_newRecipeHeaderRect.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			_newRecipeCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
			yield return new WaitForSecondsRealtime(1f);
			_newRecipeHeaderRect.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
			_newRecipeHeaderRect.DOAnchorPos(_newRecipeOrigin, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
			ShowCard();
			while (!InputManager.UI.GetAcceptButtonDown())
			{
				yield return null;
			}
			Hide();
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable && !_newRecipe)
			{
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			UIManager.PlayAudio("event:/unlock_building/selection_flash");
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
