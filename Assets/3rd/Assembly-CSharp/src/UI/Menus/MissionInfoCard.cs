using System;
using I2.Loc;
using Lamb.UI;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace src.UI.Menus
{
	public class MissionInfoCard : UIInfoCardBase<FollowerInfo>
	{
		public Action<InventoryItem.ITEM_TYPE> OnMissionSelected;

		[Header("Prison Info Card")]
		[SerializeField]
		private TextMeshProUGUI _followerNameText;

		[SerializeField]
		private SkeletonGraphic _followerSpine;

		[SerializeField]
		private TextMeshProUGUI _reasonForOddsText;

		[SerializeField]
		private MissionButton[] _missionButtons;

		private FollowerInfo _followerInfo;

		public SkeletonGraphic FollowerSpine
		{
			get
			{
				return _followerSpine;
			}
		}

		public MissionButton[] MissionButtons
		{
			get
			{
				return _missionButtons;
			}
		}

		private void Start()
		{
			MissionButton[] missionButtons = _missionButtons;
			foreach (MissionButton obj in missionButtons)
			{
				obj.OnMissionSelected = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(obj.OnMissionSelected, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE itemType)
				{
					Action<InventoryItem.ITEM_TYPE> onMissionSelected = OnMissionSelected;
					if (onMissionSelected != null)
					{
						onMissionSelected(itemType);
					}
				});
			}
		}

		public override void Configure(FollowerInfo config)
		{
			if (_followerInfo == config)
			{
				return;
			}
			_followerInfo = config;
			_followerNameText.text = config.Name + " " + config.XPLevel.ToNumeral();
			_followerSpine.ConfigureFollower(config);
			_reasonForOddsText.text = "";
			if (config.XPLevel == 1)
			{
				TextMeshProUGUI reasonForOddsText = _reasonForOddsText;
				reasonForOddsText.text = reasonForOddsText.text + "<sprite name=\"icon_BadTrait\"><sprite name=\"icon_BadTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/MissionaryScreen/ReasonsForProbability/NotLevelled"), config.Name) + "\n";
			}
			else if (config.XPLevel <= 2)
			{
				TextMeshProUGUI reasonForOddsText2 = _reasonForOddsText;
				reasonForOddsText2.text = reasonForOddsText2.text + "<sprite name=\"icon_BadTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/MissionaryScreen/ReasonsForProbability/LowLevel"), config.Name) + "\n";
			}
			else if (config.XPLevel <= 4)
			{
				TextMeshProUGUI reasonForOddsText3 = _reasonForOddsText;
				reasonForOddsText3.text = reasonForOddsText3.text + "<sprite name=\"icon_GoodTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/MissionaryScreen/ReasonsForProbability/MediumLevel"), config.Name) + "\n";
			}
			else if (config.XPLevel > 4)
			{
				TextMeshProUGUI reasonForOddsText4 = _reasonForOddsText;
				reasonForOddsText4.text = reasonForOddsText4.text + "<sprite name=\"icon_GoodTrait\"><sprite name=\"icon_GoodTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/MissionaryScreen/ReasonsForProbability/HighLevel"), config.Name) + "\n";
			}
			if (FollowerBrain.GetOrCreateBrain(config).CurrentState.Type == FollowerStateType.Exhausted)
			{
				TextMeshProUGUI reasonForOddsText5 = _reasonForOddsText;
				reasonForOddsText5.text = reasonForOddsText5.text + "<sprite name=\"icon_BadTrait\"><sprite name=\"icon_BadTrait\"><sprite name=\"icon_BadTrait\"><sprite name=\"icon_BadTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/MissionaryScreen/ReasonsForProbability/Exhausted"), config.Name) + "\n";
			}
			if (FollowerBrainStats.BrainWashed)
			{
				TextMeshProUGUI reasonForOddsText6 = _reasonForOddsText;
				reasonForOddsText6.text = reasonForOddsText6.text + "<sprite name=\"icon_GoodTrait\"><sprite name=\"icon_GoodTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/MissionaryScreen/ReasonsForProbability/Brainwashed"), config.Name) + "\n";
			}
			if (config.Necklace == InventoryItem.ITEM_TYPE.Necklace_Missionary)
			{
				TextMeshProUGUI reasonForOddsText7 = _reasonForOddsText;
				reasonForOddsText7.text = reasonForOddsText7.text + "<sprite name=\"icon_GoodTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/MissionaryScreen/ReasonsForProbability/MissionaryNecklace"), config.Name) + "\n";
			}
			if (DataManager.Instance.NextMissionarySuccessful)
			{
				TextMeshProUGUI reasonForOddsText8 = _reasonForOddsText;
				reasonForOddsText8.text = reasonForOddsText8.text + "<sprite name=\"icon_GoodTrait\"> <sprite name=\"icon_GoodTrait\"> <sprite name=\"icon_GoodTrait\"> " + string.Format(LocalizationManager.GetTranslation(string.Format("Manipulations/{0}/Notification", WorldManipulatorManager.Manipulations.MissionarySuccessful)), LocalizationManager.GetTranslation("UI/Twitch/ThanksTwitchChat"));
			}
			int num = 1;
			if (StructureManager.GetAllStructuresOfType<Structures_Missionary>().Count > 0)
			{
				switch (StructureManager.GetAllStructuresOfType<Structures_Missionary>()[0].Data.Type)
				{
				case StructureBrain.TYPES.MISSIONARY_II:
					num = 2;
					break;
				case StructureBrain.TYPES.MISSIONARY_III:
					num = 3;
					break;
				}
			}
			MissionButton[] missionButtons = _missionButtons;
			foreach (MissionButton missionButton in missionButtons)
			{
				if ((missionButton.Type == InventoryItem.ITEM_TYPE.FOLLOWERS && num < 2) || (missionButton.Type == InventoryItem.ITEM_TYPE.BONE && num < 2) || (missionButton.Type == InventoryItem.ITEM_TYPE.SEEDS && num < 2) || (missionButton.Type == InventoryItem.ITEM_TYPE.LOG_REFINED && num < 3) || (missionButton.Type == InventoryItem.ITEM_TYPE.STONE_REFINED && num < 3))
				{
					missionButton.gameObject.SetActive(false);
				}
				missionButton.Configure(config);
			}
		}

		public MMButton FirstAvailableButton()
		{
			MissionButton[] missionButtons = _missionButtons;
			foreach (MissionButton missionButton in missionButtons)
			{
				if (missionButton.gameObject.activeInHierarchy)
				{
					return missionButton.Button;
				}
			}
			return null;
		}
	}
}
