using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DemonSelectionButton : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private TMP_Text titleText;

	[SerializeField]
	private TMP_Text descriptionText;

	private int demonType;

	private int followerID = -1;

	public void Init(int demonType, int followerID)
	{
		this.demonType = demonType;
		this.followerID = followerID;
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (followerID != -1)
		{
			base.transform.DOScale(Vector3.one * 1.5f, 0.25f).SetUpdate(true);
			titleText.text = DemonModel.GetTitle(demonType, followerID);
			descriptionText.text = DemonModel.GetDescription(demonType, FollowerInfo.GetInfoByID(followerID));
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		base.transform.DOScale(Vector3.one * 1.25f, 0.25f).SetUpdate(true);
	}
}
