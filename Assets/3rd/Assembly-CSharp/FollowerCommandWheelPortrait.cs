using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class FollowerCommandWheelPortrait : BaseMonoBehaviour
{
	public SkeletonGraphic Spine;

	private float ScaleSpeed;

	public void Play(FollowerBrainInfo f)
	{
		Spine.Skeleton.SetSkin(f.SkinName);
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(f.SkinName);
		if (colourData != null)
		{
			foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(f.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
			{
				Slot slot = Spine.Skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
		}
		base.transform.localScale = Vector3.one * 2f;
	}

	private void Update()
	{
		base.transform.localScale = Vector3.one * Utils.BounceLerp(1f, base.transform.localScale.x, ref ScaleSpeed);
	}

	private IEnumerator ScaleIn()
	{
		Debug.Log("AA");
		float Progress = 0f;
		float Duration = 0.3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Debug.Log(Progress / Duration);
			base.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		base.transform.localScale = Vector3.one;
	}
}
