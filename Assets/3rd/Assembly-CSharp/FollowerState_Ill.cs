public class FollowerState_Ill : FollowerState
{
	public float SpeedMultiplier = 1f;

	public override FollowerStateType Type
	{
		get
		{
			return FollowerStateType.Ill;
		}
	}

	public override float XPMultiplierAddition
	{
		get
		{
			return -0.5f;
		}
	}

	public override float MaxSpeed
	{
		get
		{
			return 0.75f * SpeedMultiplier;
		}
	}

	public override string OverrideIdleAnim
	{
		get
		{
			return "Sick/idle-sick";
		}
	}

	public override string OverrideWalkAnim
	{
		get
		{
			return "Sick/walk-sick";
		}
	}
}
