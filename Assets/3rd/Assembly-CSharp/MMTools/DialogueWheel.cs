using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace MMTools
{
	public class DialogueWheel : MonoBehaviour
	{
		[Serializable]
		public struct DialogueOption
		{
			public MMButton Button;

			public TextMeshProUGUI Text;
		}

		public delegate void GiveAnswer(int Answer);

		[SerializeField]
		private DialogueOption[] _options;

		[SerializeField]
		private RectTransform _arrowRectTransform;

		private List<Response> _responses;

		private IMMSelectable _selectable;

		public event GiveAnswer OnGiveAnswer;

		private void OnEnable()
		{
			_responses = MMConversation.CURRENT_CONVERSATION.Responses;
			UpdateLocalization();
			LocalizationManager.OnLocalizeEvent += UpdateLocalization;
			UIMenuBase.OnFirstMenuShow = (Action)Delegate.Combine(UIMenuBase.OnFirstMenuShow, new Action(OnFirstMenuShow));
			UIMenuBase.OnFinalMenuHide = (Action)Delegate.Combine(UIMenuBase.OnFinalMenuHide, new Action(OnFinalMenuHide));
			DialogueOption[] options = _options;
			for (int i = 0; i < options.Length; i++)
			{
				DialogueOption dialogueOption = options[i];
				dialogueOption.Button.onClick.AddListener(delegate
				{
					OnOptionClicked(_options.IndexOf(dialogueOption));
				});
				MMButton button = dialogueOption.Button;
				button.OnSelected = (Action)Delegate.Combine(button.OnSelected, (Action)delegate
				{
					OnOptionSelected(dialogueOption.Button);
				});
				MMButton button2 = dialogueOption.Button;
				button2.OnDeselected = (Action)Delegate.Combine(button2.OnDeselected, (Action)delegate
				{
					OnOptionDeselected(dialogueOption.Button);
				});
			}
		}

		private void OnDisable()
		{
			LocalizationManager.OnLocalizeEvent -= UpdateLocalization;
			UIMenuBase.OnFirstMenuShow = (Action)Delegate.Remove(UIMenuBase.OnFirstMenuShow, new Action(OnFirstMenuShow));
			UIMenuBase.OnFinalMenuHide = (Action)Delegate.Remove(UIMenuBase.OnFinalMenuHide, new Action(OnFinalMenuHide));
			DialogueOption[] options = _options;
			for (int i = 0; i < options.Length; i++)
			{
				DialogueOption dialogueOption = options[i];
				dialogueOption.Button.onClick.RemoveAllListeners();
				dialogueOption.Button.OnSelected = null;
				dialogueOption.Button.OnDeselected = null;
			}
			_selectable = null;
		}

		private void OnOptionSelected(MMButton button)
		{
			AudioManager.Instance.PlayOneShot("event:/ui/change_selection");
			button.transform.DOKill();
			button.transform.DOScale(Vector3.one * 1.2f, 0.15f).SetUpdate(true).SetEase(Ease.InSine);
			Quaternion endValue = Quaternion.Euler(new Vector3(0f, 0f, Mathf.Clamp(_arrowRectTransform.position.x - button.transform.position.x, -1f, 1f) * 90f));
			_arrowRectTransform.DOKill();
			_arrowRectTransform.DORotateQuaternion(endValue, 0.15f).SetUpdate(true).SetEase(Ease.InOutSine);
			_selectable = button;
		}

		private void OnOptionDeselected(MMButton button)
		{
			button.transform.DOKill();
			button.transform.DOScale(Vector3.one, 0.15f).SetUpdate(true).SetEase(Ease.OutSine);
		}

		private void OnOptionClicked(int option)
		{
			AudioManager.Instance.PlayOneShot("event:/sermon/select_upgrade");
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
			GiveAnswer onGiveAnswer = this.OnGiveAnswer;
			if (onGiveAnswer != null)
			{
				onGiveAnswer(option);
			}
			MonoSingleton<UINavigatorNew>.Instance.Clear();
		}

		private void Update()
		{
			if (UIMenuBase.ActiveMenus.Count > 0 || _selectable != null || InputManager.General.MouseInputActive)
			{
				return;
			}
			float horizontalAxis = InputManager.UI.GetHorizontalAxis();
			if (!(Mathf.Abs(horizontalAxis) > 0f))
			{
				return;
			}
			Vector2 lhs = new Vector2(horizontalAxis, 0f);
			MMButton mMButton = null;
			float num = float.MinValue;
			DialogueOption[] options = _options;
			for (int i = 0; i < options.Length; i++)
			{
				DialogueOption dialogueOption = options[i];
				Vector2 rhs = new Vector2(Mathf.Clamp(_arrowRectTransform.position.x - dialogueOption.Button.transform.position.x, -1f, 1f), 0f);
				float num2 = Vector2.Dot(lhs, rhs);
				if (num2 > num)
				{
					num = num2;
					mMButton = dialogueOption.Button;
				}
			}
			if (mMButton != null)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(mMButton);
			}
		}

		private void UpdateLocalization()
		{
			if (_responses != null && _responses.Count > 0)
			{
				for (int i = 0; i < _options.Length; i++)
				{
					_options[i].Text.text = LocalizationManager.GetTranslation(_responses[i].Term);
				}
			}
		}

		private void OnFirstMenuShow()
		{
			SetActiveStateForMenu(false);
		}

		private void OnFinalMenuHide()
		{
			SetActiveStateForMenu(true);
		}

		protected virtual void SetActiveStateForMenu(bool state)
		{
			IMMSelectable[] componentsInChildren = base.gameObject.GetComponentsInChildren<IMMSelectable>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetInteractionState(state);
			}
			if (_selectable != null && state)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_selectable);
			}
		}
	}
}
