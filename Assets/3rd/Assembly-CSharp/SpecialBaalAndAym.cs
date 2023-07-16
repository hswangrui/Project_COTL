using Spine;
using Spine.Unity;
using UnityEngine;

public class SpecialBaalAndAym : MonoBehaviour
{
	[SerializeField]
	private SkeletonAnimation baalSpine;

	[SerializeField]
	private SkeletonAnimation aymSpine;

	private void Start()
	{
		foreach (WorshipperData.SlotAndColor slotAndColour in WorshipperData.Instance.GetColourData("Boss Baal").SlotAndColours[0].SlotAndColours)
		{
			Slot slot = baalSpine.Skeleton.FindSlot(slotAndColour.Slot);
			if (slot != null)
			{
				slot.SetColor(slotAndColour.color);
			}
		}
		foreach (WorshipperData.SlotAndColor slotAndColour2 in WorshipperData.Instance.GetColourData("Boss Aym").SlotAndColours[0].SlotAndColours)
		{
			Slot slot2 = aymSpine.Skeleton.FindSlot(slotAndColour2.Slot);
			if (slot2 != null)
			{
				slot2.SetColor(slotAndColour2.color);
			}
		}
	}
}
