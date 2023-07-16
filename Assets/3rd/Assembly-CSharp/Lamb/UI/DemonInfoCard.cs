using I2.Loc;
using Spine.Unity;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class DemonInfoCard : UIInfoCardBase<FollowerInfo>
	{
		[SerializeField]
		private TextMeshProUGUI _followerName;

		[SerializeField]
		private SkeletonGraphic _followerSpine;

		[SerializeField]
		private SkeletonGraphic _demonSpine;

		[SerializeField]
		private TextMeshProUGUI _demonName;

		[SerializeField]
		private TMP_Text _description;

		[SerializeField]
		private GameObject _effectsContainer;

		[SerializeField]
		private TextMeshProUGUI _effectsText;

		[SerializeField]
		private TextMeshProUGUI _icon;

		[SerializeField]
		private TextMeshProUGUI _iconNumber;

		[SerializeField]
		private RectTransform _redOutline;

		private FollowerInfo _followerInfo;

		public FollowerInfo FollowerInfo
		{
			get
			{
				return _followerInfo;
			}
		}

		public RectTransform RedOutline
		{
			get
			{
				return _redOutline;
			}
		}

		public override void Configure(FollowerInfo followerInfo)
		{
			if (_followerInfo != followerInfo)
			{
				_followerInfo = followerInfo;
				int demonType = DemonModel.GetDemonType(followerInfo);
				_followerName.text = followerInfo.Name + " " + followerInfo.XPLevel.ToNumeral();
				_followerSpine.ConfigureFollower(followerInfo);
				_icon.text = DemonModel.GetDemonIcon(demonType);
				int demonLevel = followerInfo.GetDemonLevel();
				_iconNumber.text = string.Format("+{0}", demonLevel);
				_effectsContainer.SetActive(demonLevel > 1);
				_effectsText.text = DemonModel.GetDemonUpgradeDescription(demonType);
				string text = Interaction_DemonSummoner.DemonSkins[demonType];
				text += ((demonLevel > 1 && demonType < 6) ? "+" : "");
				_demonSpine.Skeleton.SetSkin(text);
				_demonName.text = DemonModel.GetDemonName(demonType);
				_description.text = DemonModel.GetDescription(demonType);
				if (_followerInfo.Necklace == InventoryItem.ITEM_TYPE.Necklace_Demonic)
				{
					TMP_Text description = _description;
					description.text = description.text + "<br><sprite name=\"icon_GoodTrait\"> " + string.Format(LocalizationManager.GetTranslation("UI/DemonScreen/ReasonsForLevel/DemonicNecklace"), _followerInfo.Name) + "\n";
				}
			}
		}
	}
}
