using Spine;
using Spine.Unity;

public class SimpleSpineEventListener : BaseMonoBehaviour
{
	public delegate void SpineEvent(string EventName);

	public SkeletonAnimation skeletonAnimation;

	public event SpineEvent OnSpineEvent;

	private void Start()
	{
		if (skeletonAnimation == null)
		{
			skeletonAnimation = GetComponent<SkeletonAnimation>();
		}
		skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Event e)
	{
		if (this.OnSpineEvent != null)
		{
			this.OnSpineEvent(e.Data.Name);
		}
	}

	private void OnDestroy()
	{
		skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
	}
}
