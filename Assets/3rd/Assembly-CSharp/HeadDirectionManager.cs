using Spine.Unity;
using UnityEngine;

public class HeadDirectionManager : BaseMonoBehaviour
{
	public enum Mode
	{
		LookAngle,
		FacingAngle
	}

	public Mode CurrentMode;

	public SkeletonAnimation Spine;

	private StateMachine state;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string North;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string Horizontal;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string Default;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string South;

	public float UpdateValue;

	private void Start()
	{
		state = GetComponent<StateMachine>();
	}

	private void LateUpdate()
	{
		if (Spine == null || Spine.timeScale == 0.0001f)
		{
			return;
		}
		switch (CurrentMode)
		{
		case Mode.FacingAngle:
			UpdateValue = state.facingAngle;
			break;
		case Mode.LookAngle:
			UpdateValue = state.LookAngle;
			break;
		}
		UpdateValue = Mathf.Repeat(UpdateValue, 360f);
		if (UpdateValue <= 22.5f || UpdateValue >= 337.5f)
		{
			if (Spine.AnimationName != Horizontal)
			{
				Spine.AnimationState.SetAnimation(1, Horizontal, true);
			}
		}
		else if (UpdateValue > 22.5f && UpdateValue <= 157.5f)
		{
			if (Spine.AnimationName != North)
			{
				Spine.AnimationState.SetAnimation(1, North, true);
			}
		}
		else if (UpdateValue > 157.5f && UpdateValue <= 202.5f)
		{
			if (Spine.AnimationName != Horizontal)
			{
				Spine.AnimationState.SetAnimation(1, Horizontal, true);
			}
		}
		else if (UpdateValue > 202.5f && UpdateValue <= 247.5f)
		{
			if (Spine.AnimationName != Default)
			{
				Spine.AnimationState.SetAnimation(1, Default, true);
			}
		}
		else if (UpdateValue > 247.5f && UpdateValue <= 292.5f)
		{
			if (Spine.AnimationName != South)
			{
				Spine.AnimationState.SetAnimation(1, South, true);
			}
		}
		else if (UpdateValue > 292.5f && UpdateValue <= 337.5f && Spine.AnimationName != Default)
		{
			Spine.AnimationState.SetAnimation(1, Default, true);
		}
	}
}
