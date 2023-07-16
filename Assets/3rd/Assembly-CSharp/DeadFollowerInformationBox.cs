using System;
using I2.Loc;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class DeadFollowerInformationBox : FollowerSelectItem, IPoolListener
{
	[SerializeField]
	private RectTransform _followerContainer;

	[SerializeField]
	private SkeletonGraphic _followerSpine;

	[SerializeField]
	private TMP_Text _followerName;

	[SerializeField]
	private TMP_Text _ageHeaderText;

	[SerializeField]
	private TMP_Text _causeOfDeathText;

	[SerializeField]
	private TMP_Text _timeOfDeathText;

	[SerializeField]
	private TMP_Text _ageText;

	protected override void ConfigureImpl()
	{
		_followerName.text = _followerInfo.GetNameFormatted();
		_ageHeaderText.text = _ageHeaderText.text.Replace(":", string.Empty);
		_ageHeaderText.text = string.Format(_ageHeaderText.text, string.Empty);
		_causeOfDeathText.text = base.FollowerInfo.GetDeathText();
		_timeOfDeathText.text = string.Format(ScriptLocalization.UI.DayNumber, _followerInfo.TimeOfDeath);
		_ageText.text = _followerInfo.Age.ToString();
		_followerSpine.ConfigureFollower(_followerInfo);
		_followerSpine.SetFaceAnimation("Emotions/emotion-normal", true);
		_button.onClick.AddListener(OnButtonClicked);
	}

	private void OnButtonClicked()
	{
		Action<FollowerInfo> onFollowerSelected = OnFollowerSelected;
		if (onFollowerSelected != null)
		{
			onFollowerSelected(base.FollowerInfo);
		}
	}

	public void OnRecycled()
	{
		_button.onClick.RemoveListener(OnButtonClicked);
	}
}
