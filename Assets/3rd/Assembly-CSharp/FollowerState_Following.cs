public class FollowerState_Following : FollowerState
{
	public override FollowerStateType Type
	{
		get
		{
			return FollowerStateType.Following;
		}
	}

	public override float MaxSpeed
	{
		get
		{
			return 4f;
		}
	}

	public override string OverrideWalkAnim
	{
		get
		{
			return "run-fast";
		}
	}
}
