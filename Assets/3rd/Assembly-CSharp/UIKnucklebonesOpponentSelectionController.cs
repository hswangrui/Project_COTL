using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIKnucklebonesOpponentSelectionController : UIMenuBase
{
	public Action OnConfirmOpponent;

	public Action<int> OnOpponentSelectionChanged;

	[Header("Opponent Selection")]
	[SerializeField]
	private MMSelectable_HorizontalSelector _horizontalSelectable;

	[SerializeField]
	private MMHorizontalSelector _horizontalSelector;

	[SerializeField]
	private Image _difficultyFill;

	[SerializeField]
	private MMPageIndicators _mmPageIndicators;

	[SerializeField]
	private GameObject _maxBetContainer;

	[SerializeField]
	private TextMeshProUGUI _maxBetText;

	private KnucklebonesPlayerConfiguration[] _opponents;

	public override void Awake()
	{
		base.Awake();
		_canvasGroup.alpha = 0f;
	}

	public void Show(KnucklebonesPlayerConfiguration[] opponents, bool instant = false)
	{
		_opponents = opponents;
		Show(instant);
	}

	private void Start()
	{
		List<string> list = new List<string>();
		KnucklebonesPlayerConfiguration[] opponents = _opponents;
		foreach (KnucklebonesPlayerConfiguration knucklebonesPlayerConfiguration in opponents)
		{
			list.Add(LocalizationManager.GetTermTranslation(knucklebonesPlayerConfiguration.OpponentName));
		}
		_horizontalSelectable.Confirmable = true;
		_horizontalSelector.PrefillContent(list);
		_horizontalSelector.ContentIndex = list.Count - 1;
		MMHorizontalSelector horizontalSelector = _horizontalSelector;
		horizontalSelector.OnSelectionChanged = (Action<int>)Delegate.Combine(horizontalSelector.OnSelectionChanged, new Action<int>(OnSelectionChanged));
		MMSelectable_HorizontalSelector horizontalSelectable = _horizontalSelectable;
		horizontalSelectable.OnConfirm = (Action)Delegate.Combine(horizontalSelectable.OnConfirm, new Action(ConfirmOpponent));
		_mmPageIndicators.SetNumPages(list.Count);
		_mmPageIndicators.gameObject.SetActive(list.Count > 1);
		OnSelectionChanged(list.Count - 1);
	}

	private void OnSelectionChanged(int selection)
	{
		Action<int> onOpponentSelectionChanged = OnOpponentSelectionChanged;
		if (onOpponentSelectionChanged != null)
		{
			onOpponentSelectionChanged(selection);
		}
		_difficultyFill.fillAmount = (float)_opponents[selection].Difficulty / 10f;
		if (_mmPageIndicators.gameObject.activeSelf)
		{
			_mmPageIndicators.SetPage(selection);
		}
		_maxBetContainer.SetActive(DataManager.Instance.GetVariable(_opponents[selection].VariableToChangeOnWin));
		_maxBetText.text = string.Format("{0} {1}", "<sprite name=\"icon_blackgold\">", _opponents[selection].MaxBet);
	}

	private void ConfirmOpponent()
	{
		Action onConfirmOpponent = OnConfirmOpponent;
		if (onConfirmOpponent != null)
		{
			onConfirmOpponent();
		}
		Hide();
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

	protected override IEnumerator DoShowAnimation()
	{
		while (_canvasGroup.alpha < 1f)
		{
			_canvasGroup.alpha += Time.unscaledDeltaTime * 2f;
			yield return null;
		}
	}

	protected override IEnumerator DoHideAnimation()
	{
		while (_canvasGroup.alpha > 0f)
		{
			_canvasGroup.alpha -= Time.unscaledDeltaTime * 2f;
			yield return null;
		}
	}
}
