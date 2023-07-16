public class FollowerState_Exhausted : FollowerState
{
	public const int EVERYONE_RECOVERED = -5;

	public const int EVERYONE_NEXT_DAY = -4;

	public const int EVERYONE_WOKEN_UP = -3;

	public const int EVERYONE_EXHAUSED = -2;

	public const int DEFAULT = -1;

	public override FollowerStateType Type
	{
		get
		{
			return FollowerStateType.Exhausted;
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
			return 1.5f;
		}
	}

	public override string OverrideIdleAnim
	{
		get
		{
			return "Fatigued/idle-fatigued";
		}
	}

	public override string OverrideWalkAnim
	{
		get
		{
			return "Fatigued/walk-fatigued";
		}
	}
}
