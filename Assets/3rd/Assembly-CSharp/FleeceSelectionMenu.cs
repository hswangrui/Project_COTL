using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.UINavigator;
using TMPro;
using Unify;
using UnityEngine;
using UnityEngine.UI;

public class FleeceSelectionMenu : UISubmenuBase
{
	[Header("Buttons")]
	[SerializeField]
	private List<Button> _fleeceButtons;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private TextMeshProUGUI _descriptionText;

	[SerializeField]
	private TextMeshProUGUI _keyText;

	[SerializeField]
	private TextMeshProUGUI _equippedText;

	private bool PlayGrowAndFade;

	private bool _hasInitialised;

	protected override void OnShowStarted()
	{
		if (_hasInitialised)
		{
			return;
		}
		int num = -1;
		while (++num < _fleeceButtons.Count)
		{
			Button b = _fleeceButtons[num];
			FleeceMenuIcon f = b.GetComponent<FleeceMenuIcon>();
			f.FleeceNumber = num;
			f.Init();
			b.onClick.AddListener(delegate
			{
				SetFleece(b, f);
			});
			if (num == DataManager.Instance.PlayerFleece)
			{
				OverrideDefault(b);
			}
		}
		_keyText.text = Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.TALISMAN) + "x <sprite name=\"icon_key\">";
		_hasInitialised = true;
	}

	public static string LocalisedName(string type)
	{
		return LocalizationManager.GetTranslation("TarotCards/Fleece" + type + "/Name");
	}

	public static string LocalisedDescription(string type)
	{
		return LocalizationManager.GetTranslation("TarotCards/Fleece" + type + "/Description");
	}

	private void OnEnable()
	{
		UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
		instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
		UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
		instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
	}

	private void OnDisable()
	{
		UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
		instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
		UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
		instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
	}

	private void OnSelection(Selectable obj)
	{
		FleeceMenuIcon component = obj.GetComponent<FleeceMenuIcon>();
		_titleText.text = LocalisedName(component.FleeceNumber.ToString());
		_descriptionText.text = LocalisedDescription(component.FleeceNumber.ToString());
		switch (component.State)
		{
		case FleeceMenuIcon.States.Available:
			_equippedText.text = ScriptLocalization.UpgradeSystem.Unlock + " <sprite name=\"icon_key\"> x1";
			break;
		case FleeceMenuIcon.States.Equipped:
			_equippedText.text = ScriptLocalization.Interactions.Equipped;
			break;
		case FleeceMenuIcon.States.Locked:
			_equippedText.text = ScriptLocalization.Interactions.Requires + " <sprite name=\"icon_key\"> x1";
			break;
		case FleeceMenuIcon.States.Unlocked:
			_equippedText.text = ScriptLocalization.Interactions.Equip;
			break;
		}
	}

	private void OnSelectionChanged(Selectable arg1, Selectable arg2)
	{
		OnSelection(arg1);
	}

	private void SetFleece(Selectable obj, FleeceMenuIcon f)
	{
		switch (f.State)
		{
		case FleeceMenuIcon.States.Available:
		{
			Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.TALISMAN, -1);
			_keyText.text = Inventory.TempleKeys + "x <sprite name=\"icon_key\">";
			if (!DataManager.Instance.UnlockedFleeces.Contains(f.FleeceNumber))
			{
				DataManager.Instance.UnlockedFleeces.Add(f.FleeceNumber);
			}
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("UNLOCK_TUNIC"));
			int num = DataManager.Instance.UnlockedFleeces.Count;
			if (DataManager.Instance.UnlockedFleeces.Contains(999))
			{
				num--;
			}
			if (DataManager.Instance.UnlockedFleeces.Contains(1000))
			{
				num--;
			}
			if (num >= 10)
			{
				AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("UNLOCK_ALL_TUNICS"));
			}
			Equip(f.FleeceNumber);
			break;
		}
		case FleeceMenuIcon.States.Locked:
			f.Container.DOShakePosition(0.5f, new Vector3(10f, 0f), 10, 0f).SetUpdate(true);
			break;
		case FleeceMenuIcon.States.Unlocked:
			Equip(f.FleeceNumber);
			break;
		}
		foreach (Button fleeceButton in _fleeceButtons)
		{
			fleeceButton.GetComponent<FleeceMenuIcon>().Init();
		}
		OnSelection(obj);
		if (f.State == FleeceMenuIcon.States.Equipped)
		{
			f.transform.localScale = Vector3.one * 1.5f;
			f.transform.DOKill();
			f.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
		}
	}

	private void Equip(int FleeceNumber)
	{
		PlayGrowAndFade = true;
		DataManager.Instance.PlayerFleece = FleeceNumber;
		if (PlayerFarming.Instance != null)
		{
			SimpleSpineAnimator simpleSpineAnimator = PlayerFarming.Instance.simpleSpineAnimator;
			if ((object)simpleSpineAnimator != null)
			{
				simpleSpineAnimator.SetSkin("Lamb_" + DataManager.Instance.PlayerFleece);
			}
		}
	}

	protected override void OnHideCompleted()
	{
		base.OnHideCompleted();
		if (PlayGrowAndFade)
		{
			PlayerFarming.Instance.growAndFade.Play();
		}
	}
}
