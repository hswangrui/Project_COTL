using Spine;
using Spine.Unity;

public class SimpleSpineDeactivateAfterPlay : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string Animation;

	public bool Init { get; set; }

	private void Update()
	{
		HandleFrozenTime();
		if (!Init && Spine.AnimationState != null)
		{
			Spine.AnimationState.SetAnimation(0, Animation, true);
			Spine.AnimationState.Complete += AnimationState_Complete;
			Init = true;
		}
	}

	private void OnDisable()
	{
		Init = false;
		if (Spine != null && Spine.AnimationState != null)
		{
			Spine.AnimationState.Complete -= AnimationState_Complete;
		}
	}

	private void AnimationState_Complete(TrackEntry trackEntry)
	{
		base.gameObject.SetActive(false);
	}

	private void HandleFrozenTime()
	{
		if (PlayerRelic.TimeFrozen)
		{
			Spine.timeScale = 0.0001f;
		}
		else
		{
			Spine.timeScale = 1f;
		}
	}
}
