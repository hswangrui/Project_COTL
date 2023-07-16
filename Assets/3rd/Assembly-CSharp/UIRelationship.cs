using I2.Loc;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRelationship : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private SkeletonGraphic followerSkinGraphic;

	[SerializeField]
	private GameObject selectedBorder;

	[SerializeField]
	private GameObject descriptionContainer;

	[SerializeField]
	private TMP_Text relationshipTitle;

	[SerializeField]
	private TMP_Text relationshipDescription;

	[SerializeField]
	private Image relationshipIcon;

	[Space]
	[SerializeField]
	private Sprite enemiesIcon;

	[SerializeField]
	private Sprite friendsIcon;

	[SerializeField]
	private Sprite loversIcon;

	public void Play(FollowerInfo followerInfo, IDAndRelationship relationship)
	{
		followerSkinGraphic.Skeleton.SetSkin(WorshipperData.Instance.Characters[followerInfo.SkinCharacter].Skin[followerInfo.SkinVariation].Skin);
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(followerInfo.SkinName);
		if (colourData != null)
		{
			foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(followerInfo.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
			{
				Slot slot = followerSkinGraphic.Skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
		}
		string text = "";
		if (relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Friends)
		{
			relationshipIcon.sprite = friendsIcon;
			relationshipIcon.color = Color.green;
			text = LocalizationManager.GetTranslation("UI/Friends");
		}
		else if (relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Enemies)
		{
			relationshipIcon.sprite = enemiesIcon;
			relationshipIcon.color = Color.red;
			text = LocalizationManager.GetTranslation("UI/Enemies");
		}
		else if (relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Lovers)
		{
			relationshipIcon.sprite = loversIcon;
			text = LocalizationManager.GetTranslation("UI/Lovers");
		}
		else
		{
			relationshipIcon.gameObject.SetActive(false);
		}
		relationshipTitle.text = text;
		relationshipDescription.text = string.Format(LocalizationManager.GetTranslation("UI/RelationshipDescription"), followerInfo.Name);
	}

	public void OnSelect(BaseEventData eventData)
	{
		descriptionContainer.SetActive(true);
		selectedBorder.SetActive(true);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		descriptionContainer.SetActive(false);
		selectedBorder.SetActive(false);
	}
}
