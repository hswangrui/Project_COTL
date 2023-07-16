public abstract class FollowerState
{
	public abstract FollowerStateType Type { get; }

	public virtual float XPMultiplierAddition
	{
		get
		{
			return 0f;
		}
	}

	public virtual float MaxSpeed
	{
		get
		{
			return 2.25f;
		}
	}

	public virtual string OverrideIdleAnim
	{
		get
		{
			return null;
		}
	}

	public virtual string OverrideWalkAnim
	{
		get
		{
			return null;
		}
	}

	public virtual void Setup(Follower follower)
	{
		SetStateAnimations(follower);
	}

	public virtual void Cleanup(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Idle);
		if (OverrideIdleAnim != null && animationData.Animation.name == OverrideIdleAnim)
		{
			animationData.Animation = animationData.DefaultAnimation;
		}
		SimpleSpineAnimator.SpineChartacterAnimationData animationData2 = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Moving);
		if (OverrideWalkAnim != null && animationData2.Animation.name == OverrideWalkAnim)
		{
			animationData2.Animation = animationData2.DefaultAnimation;
		}
	}

	public void SetStateAnimations(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Idle);
		if (OverrideIdleAnim != null && animationData.Animation.name == animationData.DefaultAnimation.name)
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, OverrideIdleAnim);
		}
		SimpleSpineAnimator.SpineChartacterAnimationData animationData2 = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Moving);
		if (OverrideWalkAnim != null && animationData2.Animation.name == animationData2.DefaultAnimation.name)
		{
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, OverrideWalkAnim);
		}
	}
}
