using System;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Menus
{
	public class MissionButton : BaseMonoBehaviour
	{
		public Action<InventoryItem.ITEM_TYPE> OnMissionSelected;

		[SerializeField]
		private InventoryItem.ITEM_TYPE _type;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private TextMeshProUGUI _chanceText;

		[SerializeField]
		private TextMeshProUGUI _icon;

		[SerializeField]
		private TextMeshProUGUI _titleText;

		[SerializeField]
		private TextMeshProUGUI _amountText;

		[SerializeField]
		private TextMeshProUGUI _durationText;

		[SerializeField]
		private Image _chanceWheel;

		public InventoryItem.ITEM_TYPE Type
		{
			get
			{
				return _type;
			}
		}

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public void Start()
		{
			_button.onClick.AddListener(OnMissionClicked);
		}

		public void Configure(FollowerInfo followerInfo)
		{
			List<Structures_Missionary> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_Missionary>();
			if (_type == InventoryItem.ITEM_TYPE.SEEDS)
			{
				_type = (InventoryItem.ITEM_TYPE)MissionaryManager.GetSeedsReward(_type).type;
			}
			float chance = MissionaryManager.GetChance(_type, followerInfo, (allStructuresOfType.Count > 0) ? allStructuresOfType[0].Data.Type : StructureBrain.TYPES.MISSIONARY);
			_chanceWheel.fillAmount = chance;
			_chanceText.text = string.Format("{0}%", (int)(chance * 100f));
			_chanceText.color = StaticColors.ColorForThreshold(chance);
			_icon.text = FontImageNames.GetIconByType(_type);
			_titleText.text = InventoryItem.LocalizedName(_type);
			_amountText.text = string.Join(" ", MissionaryManager.GetRewardRange(_type).ToString(), string.Format("({0})", Inventory.GetItemQuantity(_type)).Colour(StaticColors.GreyColor));
			_durationText.text = string.Format(ScriptLocalization.UI_Generic.Days, MissionaryManager.GetDurationDeterministic(followerInfo, _type));
		}

		private void OnMissionClicked()
		{
			Action<InventoryItem.ITEM_TYPE> onMissionSelected = OnMissionSelected;
			if (onMissionSelected != null)
			{
				onMissionSelected(_type);
			}
		}
	}
}
