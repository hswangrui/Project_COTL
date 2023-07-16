using System.Collections.Generic;
using I2.Loc;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionaryFollowerItem : FollowerSelectItem
{
	[SerializeField]
	private SkeletonGraphic _followerSpine;

	[SerializeField]
	private TextMeshProUGUI _followerName;

	[SerializeField]
	private TextMeshProUGUI _missionDescription;

	[SerializeField]
	private Image _progressBar;

	[SerializeField]
	private TextMeshProUGUI _dayText;

	[SerializeField]
	private TextMeshProUGUI _typeIcon;

	[SerializeField]
	private TextMeshProUGUI _amountText;

	[SerializeField]
	private Image _successRate;

	[SerializeField]
	private TextMeshProUGUI _successText;

	protected override void ConfigureImpl()
	{
		List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>();
		_followerSpine.ConfigureFollower(_followerInfo);
		_followerName.text = _followerInfo.GetNameFormatted();
		_button.Confirmable = false;
		_missionDescription.text = string.Format(ScriptLocalization.Objectives_Missionary.Description, InventoryItem.LocalizedName((InventoryItem.ITEM_TYPE)_followerInfo.MissionaryType).Colour(Color.yellow));
		_progressBar.fillAmount = (TimeManager.TotalElapsedGameTime - _followerInfo.MissionaryTimestamp) / _followerInfo.MissionaryDuration;
		_dayText.text = MissionaryManager.GetExpiryFormatted(_followerInfo.MissionaryTimestamp + _followerInfo.MissionaryDuration);
		_typeIcon.text = FontImageNames.GetIconByType((InventoryItem.ITEM_TYPE)_followerInfo.MissionaryType);
		_amountText.text = MissionaryManager.GetRewardRange((InventoryItem.ITEM_TYPE)_followerInfo.MissionaryType).ToString();
		float chance = MissionaryManager.GetChance((InventoryItem.ITEM_TYPE)_followerInfo.MissionaryType, _followerInfo, (allStructuresOfType.Count > 0) ? allStructuresOfType[0].Data.Type : StructureBrain.TYPES.MISSIONARY);
		_successRate.fillAmount = chance;
		_successText.text = string.Format("{0}%", (int)(chance * 100f));
	}
}
