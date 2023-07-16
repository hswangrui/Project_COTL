using Lamb.UI;
using MMTools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDoctrineChoiceInfoBox : UIChoiceInfoCard<DoctrineResponse>, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TextMeshProUGUI _unlockName;

	[SerializeField]
	private TextMeshProUGUI _unlockDescription;

	[SerializeField]
	private TextMeshProUGUI _unlockType;

	[SerializeField]
	private TextMeshProUGUI _unlockTypeIcon;

	protected override void ConfigureImpl(DoctrineResponse info)
	{
		DoctrineUpgradeSystem.DoctrineType sermonReward = DoctrineUpgradeSystem.GetSermonReward(info.SermonCategory, info.RewardLevel, info.isFirstChoice);
		_icon.sprite = DoctrineUpgradeSystem.GetIcon(sermonReward);
		_unlockName.text = DoctrineUpgradeSystem.GetLocalizedName(sermonReward);
		_unlockDescription.text = DoctrineUpgradeSystem.GetLocalizedDescription(sermonReward);
		_unlockType.text = DoctrineUpgradeSystem.GetDoctrineUnlockString(sermonReward);
		if (string.IsNullOrEmpty(_unlockType.text))
		{
			_unlockType.gameObject.SetActive(false);
		}
		_unlockTypeIcon.text = DoctrineUpgradeSystem.GetDoctrineUnlockIcon(sermonReward);
	}
}
