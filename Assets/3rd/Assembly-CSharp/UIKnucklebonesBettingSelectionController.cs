using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using TMPro;
using UnityEngine;

public class UIKnucklebonesBettingSelectionController : UIMenuBase
{
	public const int kBetIncrement = 5;

	public Action<int> OnConfirmBet;

	[Header("Betting")]
	[SerializeField]
	private MMSelectable_HorizontalSelector _horizontalSelectable;

	[SerializeField]
	private MMHorizontalSelector _horizontalSelector;

	[SerializeField]
	private TextMeshProUGUI _winningsText;

	private KnucklebonesPlayerConfiguration _opponent;

	private int _maxBet
	{
		get
		{
			return _opponent.MaxBet;
		}
	}

	private int _playerCoins
	{
		get
		{
			return Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD);
		}
	}

	public override void Awake()
	{
		base.Awake();
		_canvasGroup.alpha = 0f;
	}

	private void Start()
	{
		List<string> list = new List<string>();
		list.Add(KnucklebonesModel.GetLocalizedString("NoCoins"));
		for (int i = 1; i <= _maxBet / 5; i++)
		{
			list.Add(string.Format("{0} {1} / {2}", StringUtilities.Sprite("icon_blackgold"), i * 5, _maxBet));
		}
		_horizontalSelectable.Confirmable = true;
		_horizontalSelector.PrefillContent(list);
		MMHorizontalSelector horizontalSelector = _horizontalSelector;
		horizontalSelector.OnSelectionChanged = (Action<int>)Delegate.Combine(horizontalSelector.OnSelectionChanged, new Action<int>(OnSelectionChanged));
		MMSelectable_HorizontalSelector horizontalSelectable = _horizontalSelectable;
		horizontalSelectable.OnConfirm = (Action)Delegate.Combine(horizontalSelectable.OnConfirm, new Action(ConfirmBet));
		OnSelectionChanged(0);
	}

	public void Show(KnucklebonesPlayerConfiguration opponentConfiguration, bool instant = false)
	{
		_opponent = opponentConfiguration;
		Show(instant);
	}

	private void OnSelectionChanged(int selection)
	{
		if (selection > 0)
		{
			_winningsText.text = string.Format("{0} {1} {2}", ScriptLocalization.Interactions.Collect, StringUtilities.Sprite("icon_blackgold"), selection * 5);
		}
		else
		{
			_winningsText.text = "";
		}
		_horizontalSelectable.Confirmable = _playerCoins >= selection * 5;
		if (_horizontalSelectable.Confirmable)
		{
			_horizontalSelectable.HorizontalSelector.Color = StaticColors.OffWhiteColor;
		}
		else
		{
			_horizontalSelectable.HorizontalSelector.Color = StaticColors.RedColor;
		}
	}

	private void ConfirmBet()
	{
		Action<int> onConfirmBet = OnConfirmBet;
		if (onConfirmBet != null)
		{
			onConfirmBet(_horizontalSelector.ContentIndex);
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
