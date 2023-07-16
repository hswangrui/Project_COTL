using System;
using UnityEngine;

public class ShareHouseItem : FollowerInformationBox
{
	public Action<ShareHouseItem> OnShareHouseItemSelected;

	[Header("Share House Item")]
	[SerializeField]
	private GameObject _vacancyContainer;

	[SerializeField]
	private GameObject _contentContainer;

	private void Awake()
	{
		MMButton button = base.Button;
		button.OnSelected = (Action)Delegate.Combine(button.OnSelected, new Action(OnSelfSelected));
	}

	protected override void ConfigureImpl()
	{
		if (_followerInfo == null)
		{
			_vacancyContainer.SetActive(true);
			_contentContainer.SetActive(false);
		}
		else
		{
			_vacancyContainer.SetActive(false);
			_contentContainer.SetActive(true);
			base.ConfigureImpl();
		}
	}

	private void OnSelfSelected()
	{
		Action<ShareHouseItem> onShareHouseItemSelected = OnShareHouseItemSelected;
		if (onShareHouseItemSelected != null)
		{
			onShareHouseItemSelected(this);
		}
	}
}
