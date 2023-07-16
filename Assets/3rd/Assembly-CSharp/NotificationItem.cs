using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NotificationItem : NotificationBase
{
	public const float DELAY_BETWEEN_UPDATES = 0.2f;

	private InventoryItem.ITEM_TYPE _itemType;

	private int _delta;

	private float _timer;

	private float _updateDelay;

	private bool _pendingUpdate;

	[SerializeField]
	private GameObject backgroundGO;

	private Image background;

	[SerializeField]
	private Sprite negativeSprite;

	[SerializeField]
	private Sprite positiveSprite;

	private string localizedName;

	protected override float _onScreenDuration
	{
		get
		{
			return 3f;
		}
	}

	protected override float _showHideDuration
	{
		get
		{
			return 0.4f;
		}
	}

	public InventoryItem.ITEM_TYPE ItemType
	{
		get
		{
			return _itemType;
		}
	}

	public Image backgroundImage()
	{
		if (background == null)
		{
			try
			{
				if (backgroundGO != null)
				{
					background = backgroundGO.GetComponent<Image>();
				}
			}
			catch
			{
				return null;
			}
		}
		return background;
	}

	public void Configure(InventoryItem.ITEM_TYPE itemType, int delta, Flair flair = Flair.None)
	{
		_itemType = itemType;
		_delta = delta;
		Configure(flair);
		_contentRectTransform.anchoredPosition = _offScreenPosition;
		Localize();
		Show(false, delegate
		{
			background = backgroundImage();
		});
		if (backgroundImage() != null)
		{
			backgroundImage().sprite = ((delta >= 0) ? positiveSprite : negativeSprite);
		}
	}

	private void Update()
	{
		_updateDelay += Time.deltaTime;
		if (_pendingUpdate && _updateDelay >= 0.4f)
		{
			_updateDelay = 0f;
			_pendingUpdate = false;
			DoPunchTween();
			UpdateLocalizedText();
		}
	}

	private void DoPunchTween()
	{
		_contentRectTransform.DOKill();
		_contentRectTransform.anchoredPosition = _onScreenPosition;
		_contentRectTransform.DOPunchPosition(new Vector3(25f, 0f), 1f, 5).SetEase(Ease.InExpo);
	}

	public void UpdateDelta(int delta)
	{
		if (!Shown)
		{
			Show(false, delegate
			{
				background = backgroundImage();
			});
		}
		if (backgroundImage() != null)
		{
			backgroundImage().sprite = ((delta >= 0) ? positiveSprite : negativeSprite);
		}
		_timer = _onScreenDuration;
		_delta += delta;
		if (_delta == 0)
		{
			Hide(true);
		}
		if (_updateDelay >= 0.2f)
		{
			if (Shown)
			{
				DoPunchTween();
			}
			_updateDelay = 0f;
			_pendingUpdate = false;
			UpdateLocalizedText();
		}
		else
		{
			_pendingUpdate = true;
		}
	}

	protected override IEnumerator HoldOnScreen()
	{
		_timer = _onScreenDuration;
		while (_timer > 0f)
		{
			if (HUD_Manager.Instance != null && !HUD_Manager.Instance.Hidden && !LetterBox.IsPlaying)
			{
				_timer -= Time.deltaTime;
			}
			yield return null;
		}
	}

	private void UpdateLocalizedText()
	{
		_description.text = "<size=30>" + FontImageNames.GetIconByType(_itemType) + " " + ((_delta > 0) ? "+" : "") + _delta + "</size> " + localizedName + "  <size=25><color=#6E6666>" + Inventory.GetItemQuantity((int)_itemType) + "</color></size>";
	}

	protected override void Localize()
	{
		localizedName = InventoryItem.LocalizedName(_itemType);
		_description.text = "<size=30>" + FontImageNames.GetIconByType(_itemType) + " " + ((_delta > 0) ? "+" : "") + _delta + "</size> " + localizedName + "  <size=25><color=#6E6666>" + Inventory.GetItemQuantity((int)_itemType) + "</color></size>";
	}
}
