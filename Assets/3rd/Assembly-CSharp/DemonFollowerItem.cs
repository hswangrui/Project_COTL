using I2.Loc;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class DemonFollowerItem : FollowerSelectItem
{
	[SerializeField]
	private TextMeshProUGUI _followerName;

	[SerializeField]
	private SkeletonGraphic _demonSpine;

	[SerializeField]
	private TextMeshProUGUI _demonIcon;

	[SerializeField]
	private TextMeshProUGUI _iconNumber;

	[SerializeField]
	private TextMeshProUGUI _demonName;

	[SerializeField]
	private TextMeshProUGUI _demonDescription;

	[SerializeField]
	private TextMeshProUGUI _effectsText;

	[SerializeField]
	private GameObject _effectsContainer;

	protected override void ConfigureImpl()
	{
		_followerName.text = _followerInfo.GetNameFormatted();
		int demonType = DemonModel.GetDemonType(_followerInfo);
		string text = Interaction_DemonSummoner.DemonSkins[demonType];
		int demonLevel = _followerInfo.GetDemonLevel();
		text += ((demonLevel > 1 && demonType < 6) ? "+" : "");
		_demonSpine.Skeleton.SetSkin(text);
		_demonIcon.text = DemonModel.GetDemonIcon(demonType);
		_iconNumber.text = string.Format("+{0}", demonLevel);
		_demonName.text = DemonModel.GetDemonName(demonType);
		_demonDescription.text = DemonModel.GetDescription(demonType);
		_effectsContainer.SetActive(demonLevel > 1);
		_effectsText.text = DemonModel.GetDemonUpgradeDescription(demonType);
		if (_followerInfo.Necklace == InventoryItem.ITEM_TYPE.Necklace_Demonic)
		{
			TextMeshProUGUI demonDescription = _demonDescription;
			demonDescription.text = demonDescription.text + "<br><sprite name=\"icon_GoodTrait\"><sprite name=\"icon_GoodTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/DemonScreen/ReasonsForLevel/DemonicNecklace"), _followerInfo.Name) + "\n";
		}
	}
}
