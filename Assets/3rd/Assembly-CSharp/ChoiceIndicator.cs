using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.UINavigator;
using TMPro;
using UnityEngine;

public class ChoiceIndicator : BaseMonoBehaviour
{
	[Serializable]
	public struct ChoiceOption
	{
		public MMButton Button;

		public TextMeshProUGUI Text;

		public Localize TextLocalize;

		public TextMeshProUGUI SubtitleText;

		public Localize SubtitleLocalize;

		public Action Callback;
	}

	[Header("Offset")]
	public Vector3 Offset;

	[Header("Arrow")]
	public RectTransform _arrowRectTransform;

	[Header("Choices")]
	[SerializeField]
	private ChoiceOption _choice1;

	[SerializeField]
	private ChoiceOption _choice2;

	[Header("Objectives Box")]
	[SerializeField]
	private GameObject _informationBox;

	[SerializeField]
	private Localize _informationTitle;

	[SerializeField]
	private TextMeshProUGUI _informationDescription;

	[Header("Prompt")]
	[SerializeField]
	private GameObject _promptBox;

	[SerializeField]
	private Localize _prompt;

	[Header("Misc")]
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private RectTransform _rectTransform;

	private float _yLerpSpeed = 20f;

	private Camera _currentMain;

	private Vector3 _position;

	private ObjectivesData _objectivesData;

	private IMMSelectable _selectable;

	private void OnEnable()
	{
		UIMenuBase.OnFirstMenuShow = (Action)Delegate.Combine(UIMenuBase.OnFirstMenuShow, new Action(OnFirstMenuShow));
		UIMenuBase.OnFinalMenuHide = (Action)Delegate.Combine(UIMenuBase.OnFinalMenuHide, new Action(OnFinalMenuHide));
		_choice1.Button.onClick.AddListener(delegate
		{
			OnOptionClicked(_choice1);
		});
		_choice2.Button.onClick.AddListener(delegate
		{
			OnOptionClicked(_choice2);
		});
		MMButton button = _choice1.Button;
		button.OnSelected = (Action)Delegate.Combine(button.OnSelected, (Action)delegate
		{
			OnOptionSelected(_choice1.Button);
		});
		MMButton button2 = _choice2.Button;
		button2.OnSelected = (Action)Delegate.Combine(button2.OnSelected, (Action)delegate
		{
			OnOptionSelected(_choice2.Button);
		});
		MMButton button3 = _choice1.Button;
		button3.OnDeselected = (Action)Delegate.Combine(button3.OnDeselected, (Action)delegate
		{
			OnOptionDeselected(_choice1.Button);
		});
		MMButton button4 = _choice2.Button;
		button4.OnDeselected = (Action)Delegate.Combine(button4.OnDeselected, (Action)delegate
		{
			OnOptionDeselected(_choice2.Button);
		});
	}

	private void OnDisable()
	{
		UIMenuBase.OnFirstMenuShow = (Action)Delegate.Remove(UIMenuBase.OnFirstMenuShow, new Action(OnFirstMenuShow));
		UIMenuBase.OnFinalMenuHide = (Action)Delegate.Remove(UIMenuBase.OnFinalMenuHide, new Action(OnFinalMenuHide));
		_choice1.Button.onClick.RemoveAllListeners();
		_choice1.Button.OnSelected = null;
		_choice1.Button.OnDeselected = null;
		_choice2.Button.onClick.RemoveAllListeners();
		_choice2.Button.OnSelected = null;
		_choice2.Button.OnDeselected = null;
		LocalizationManager.OnLocalizeEvent -= LocalizeObjective;
	}

	public void Show(string choiceTerm1, string choiceTerm2, Action choice1Callback, Action choice2Callback, Vector3 worldPosition)
	{
		Show(choiceTerm1, string.Empty, choiceTerm2, string.Empty, choice1Callback, choice2Callback, worldPosition);
	}

	public void Show(string choice1Term, string choice1SubtitleTerm, string choice2Term, string choice2SubtitleTerm, Action choice1Callback, Action choice2Callback, Vector3 worldPosition)
	{
		_choice1.TextLocalize.Term = choice1Term;
		if (!string.IsNullOrEmpty(choice1SubtitleTerm))
		{
			_choice1.SubtitleLocalize.Term = choice1SubtitleTerm;
		}
		else
		{
			_choice1.SubtitleLocalize.gameObject.SetActive(false);
		}
		_choice1.Callback = choice1Callback;
		_choice2.TextLocalize.Term = choice2Term;
		if (!string.IsNullOrEmpty(choice2SubtitleTerm))
		{
			_choice2.SubtitleLocalize.Term = choice2SubtitleTerm;
		}
		else
		{
			_choice2.SubtitleLocalize.gameObject.SetActive(false);
		}
		_choice2.Callback = choice2Callback;
		Show(worldPosition);
	}

	private void Show(Vector3 worldPosition)
	{
		_currentMain = Camera.main;
		_rectTransform.position = _currentMain.WorldToScreenPoint(worldPosition) - Offset;
		_informationBox.SetActive(false);
		_promptBox.SetActive(false);
		StartCoroutine(DoShow());
	}

	public void ShowObjectivesBox(ObjectivesData objectivesData)
	{
		_objectivesData = objectivesData;
		_informationBox.SetActive(true);
		LocalizationManager.OnLocalizeEvent += LocalizeObjective;
		LocalizeObjective();
	}

	private void LocalizeObjective()
	{
		if (!string.IsNullOrEmpty(_objectivesData.GroupId))
		{
			_informationTitle.Term = _objectivesData.GroupId;
		}
		else
		{
			_informationTitle.gameObject.SetActive(false);
		}
		if (!string.IsNullOrEmpty(_objectivesData.Text))
		{
			_informationDescription.text = _objectivesData.Text;
		}
		else
		{
			_informationDescription.gameObject.SetActive(false);
		}
	}

	public void ShowPrompt(string prompt)
	{
		_promptBox.SetActive(true);
		_prompt.Term = prompt;
	}

	private void OnOptionSelected(MMButton button)
	{
		AudioManager.Instance.PlayOneShot("event:/ui/change_selection");
		button.transform.DOKill();
		button.transform.DOScale(Vector3.one * 1.1f, 0.15f).SetUpdate(true).SetEase(Ease.InSine);
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

	private void OnOptionClicked(ChoiceOption choiceOption)
	{
		AudioManager.Instance.PlayOneShot("event:/sermon/select_upgrade");
		MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		Action callback = choiceOption.Callback;
		if (callback != null)
		{
			callback();
		}
		Hide();
	}

	public void UpdatePosition(Vector3 worldPosition)
	{
		_position = _currentMain.WorldToScreenPoint(worldPosition) - Offset;
	}

	private void LateUpdate()
	{
		_rectTransform.position = Vector3.Lerp(_rectTransform.position, _position, Time.unscaledDeltaTime * _yLerpSpeed);
	}

	private void Update()
	{
		if (!_canvasGroup.interactable || _selectable != null || InputManager.General.MouseInputActive)
		{
			return;
		}
		float horizontalAxis = InputManager.UI.GetHorizontalAxis();
		if (Mathf.Abs(horizontalAxis) > 0.2f)
		{
			if (horizontalAxis < 0f)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_choice1.Button);
			}
			else
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_choice2.Button);
			}
		}
	}

	private void Hide()
	{
		StartCoroutine(DoHide());
	}

	private IEnumerator DoShow()
	{
		_canvasGroup.interactable = false;
		float progress = 0f;
		float duration = 0.5f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.unscaledDeltaTime);
			if (!(num < duration))
			{
				break;
			}
			_canvasGroup.alpha = progress / duration;
			_rectTransform.localScale = Vector3.one * Mathf.SmoothStep(1.2f, 1f, progress / duration);
			yield return null;
		}
		_canvasGroup.alpha = 1f;
		_rectTransform.localScale = Vector3.one;
		_canvasGroup.interactable = true;
	}

	private IEnumerator DoHide()
	{
		_canvasGroup.interactable = false;
		SetActiveStateForMenu(false);
		MonoSingleton<UINavigatorNew>.Instance.Clear();
		_canvasGroup.DOKill();
		_canvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
		yield return new WaitForSecondsRealtime(0.2f);
		UnityEngine.Object.Destroy(base.gameObject);
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
