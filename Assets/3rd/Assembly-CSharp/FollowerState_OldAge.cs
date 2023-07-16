public class FollowerState_OldAge : FollowerState
{
	public override FollowerStateType Type
	{
		get
		{
			return FollowerStateType.OldAge;
		}
	}

	public override float MaxSpeed
	{
		get
		{
			return 0.5f;
		}
	}

	public override string OverrideIdleAnim
	{
		get
		{
			return "Old/idle-old";
		}
	}

	public override string OverrideWalkAnim
	{
		get
		{
			return "Old/walk-old";
		}
	}
}
