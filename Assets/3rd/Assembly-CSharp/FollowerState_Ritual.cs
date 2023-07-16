public class FollowerState_Ritual : FollowerState
{
	public override FollowerStateType Type
	{
		get
		{
			return FollowerStateType.Ritual;
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
			return "pray";
		}
	}

	public override string OverrideWalkAnim
	{
		get
		{
			return "walk-hooded-ritual";
		}
	}
}
