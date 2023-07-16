using System;
using System.Collections;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFollowerXPIcon : BaseMonoBehaviour
{
	public float DisplayXP;

	public TextMeshProUGUI Name;

	public TextMeshProUGUI Level;

	public TextMeshProUGUI ReadyForPromotion;

	public Image XPProgressBar;

	public SkeletonGraphic Spine;

	[HideInInspector]
	public FollowerBrain Brain;

	public CanvasGroup canvasGroup;

	private Coroutine cUpdateXPRoutine;

	private void OnLevelUp()
	{
		if (cUpdateXPRoutine != null)
		{
			StopCoroutine(cUpdateXPRoutine);
		}
		StartCoroutine(FlashColour());
		canvasGroup.alpha = 1f;
		UpdateBar();
	}

	private void OnDestroy()
	{
		if (Brain != null && Brain.Info != null)
		{
			FollowerBrainInfo info = Brain.Info;
			info.OnReadyToPromote = (Action)Delegate.Remove(info.OnReadyToPromote, new Action(OnLevelUp));
		}
	}

	public void Play(FollowerBrain Brain)
	{
		this.Brain = Brain;
		FollowerBrainInfo info = this.Brain.Info;
		info.OnReadyToPromote = (Action)Delegate.Combine(info.OnReadyToPromote, new Action(OnLevelUp));
		Name.text = Brain.Info.Name;
		DisplayXP = Brain.Info.CacheXP;
		Debug.Log("DisplayXP " + DisplayXP);
		UpdateBar();
		Spine.Skeleton.SetSkin(Brain.Info.SkinName);
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(Brain.Info.SkinName);
		if (colourData != null)
		{
			foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(Brain.Info.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
			{
				Slot slot = Spine.Skeleton.FindSlot(slotAndColour.Slot);
				if (slot != null)
				{
					slot.SetColor(slotAndColour.color);
				}
			}
		}
		canvasGroup.alpha = 0f;
	}

	public void UpdateXP(float Delay)
	{
		if (cUpdateXPRoutine != null)
		{
			StopCoroutine(cUpdateXPRoutine);
		}
		cUpdateXPRoutine = StartCoroutine(UpdateXPRoutine(Delay));
	}

	private IEnumerator UpdateXPRoutine(float Delay)
	{
		yield return new WaitForSecondsRealtime(Delay);
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			canvasGroup.alpha = Progress / Duration;
			yield return null;
		}
		UpdateBar();
		yield return new WaitForSecondsRealtime(0.2f);
		StartCoroutine(FlashColour());
	}

	private IEnumerator FlashColour()
	{
		float Progress = 0f;
		float Duration = 0.5f;
		Color ProgressBarColor = XPProgressBar.color;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (num < Duration)
			{
				XPProgressBar.color = Color.Lerp(Color.white, ProgressBarColor, Mathf.SmoothStep(0.5f, 1f, Progress / Duration));
				yield return null;
				continue;
			}
			break;
		}
	}

	public void Skip()
	{
		if (cUpdateXPRoutine != null)
		{
			StopCoroutine(cUpdateXPRoutine);
		}
		UpdateBar();
	}

	private void UpdateBar()
	{
		float num = 1f;
		XPProgressBar.transform.localScale = new Vector3(num, XPProgressBar.transform.localScale.y);
		Level.text = "Level " + Brain.Info.XPLevel.ToNumeral();
		ReadyForPromotion.enabled = num >= 1f;
	}
}
