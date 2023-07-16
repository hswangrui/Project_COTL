using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.BuildMenu
{
	public class BuildInfoCard : UIInfoCardBase<StructureBrain.TYPES>
	{
		[Header("Copy")]
		[SerializeField]
		private TextMeshProUGUI _headerText;

		[SerializeField]
		private TextMeshProUGUI _descriptionText;

		[SerializeField]
		private TextMeshProUGUI _requirementsText;

		[SerializeField]
		private GameObject _singleBuildContainer;

		[SerializeField]
		private TextMeshProUGUI _buildStatusText;

		[SerializeField]
		private TextMeshProUGUI _buildCountBadge;

		[Header("Costs")]
		[SerializeField]
		private GameObject _costHeader;

		[SerializeField]
		private GameObject _costContainer;

		[SerializeField]
		private TextMeshProUGUI[] _costTexts;

		[Header("Graphics")]
		[SerializeField]
		private Image _icon;

		public override void Configure(StructureBrain.TYPES structureType)
		{
			_icon.sprite = TypeAndPlacementObjects.GetByType(structureType).IconImage;
			_headerText.text = StructuresData.LocalizedName(structureType);
			_descriptionText.text = StructuresData.LocalizedDescription(structureType);
			_requirementsText.gameObject.SetActive(false);
			_buildStatusText.gameObject.SetActive(false);
			_costHeader.SetActive(true);
			_costContainer.SetActive(true);
			List<StructuresData.ItemCost> cost = StructuresData.GetCost(structureType);
			for (int i = 0; i < _costTexts.Length; i++)
			{
				if (i >= cost.Count)
				{
					_costTexts[i].gameObject.SetActive(false);
					continue;
				}
				_costTexts[i].text = cost[i].ToStringShowQuantity();
				_costTexts[i].gameObject.SetActive(true);
			}
			_costHeader.SetActive(cost.Count > 0);
			_costContainer.SetActive(cost.Count > 0);
			if (StructuresData.IsUpgradeStructure(structureType))
			{
				StructureBrain.TYPES upgradePrerequisite = StructuresData.GetUpgradePrerequisite(structureType);
				if (StructureManager.GetAllStructuresOfType(upgradePrerequisite).Count <= 0)
				{
					_requirementsText.text = ScriptLocalization.Interactions.Requires + " <color=#FFD201>" + StructuresData.LocalizedName(upgradePrerequisite);
					_requirementsText.gameObject.SetActive(true);
				}
			}
			if (StructuresData.RequiresTempleToBuild(structureType) && !StructuresData.HasTemple())
			{
				_requirementsText.text = ScriptLocalization.Interactions.Requires + " <color=#FFD201>" + StructuresData.LocalizedName(StructureBrain.TYPES.TEMPLE);
				_requirementsText.gameObject.SetActive(true);
			}
			bool buildOnlyOne = StructuresData.GetBuildOnlyOne(structureType);
			_singleBuildContainer.gameObject.SetActive(buildOnlyOne);
			if (buildOnlyOne)
			{
				if (StructureManager.IsBuilding(structureType))
				{
					_buildStatusText.gameObject.SetActive(true);
					_buildStatusText.text = ScriptLocalization.UI_Settings_Controls_Header.Building.Colour(StaticColors.YellowColorHex);
					_costHeader.SetActive(false);
					_costContainer.SetActive(false);
				}
				else if (StructureManager.IsBuilt(structureType) || StructureManager.IsAnyUpgradeBuiltOrBuilding(structureType))
				{
					_buildStatusText.gameObject.SetActive(true);
					_buildStatusText.text = ScriptLocalization.UI_BuildingMenu.AlreadyBuilt.Colour(StaticColors.YellowColorHex);
					_costHeader.SetActive(false);
					_costContainer.SetActive(false);
				}
				_buildCountBadge.text = "1";
			}
			else
			{
				_buildCountBadge.text = "<sprite name=\"icon_Infinity\">";
			}
		}
	}
}
