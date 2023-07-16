public class FollowerState_Motivated : FollowerState
{
	public override FollowerStateType Type
	{
		get
		{
			return FollowerStateType.Motivated;
		}
	}

	public override float MaxSpeed
	{
		get
		{
			return 1.4f;
		}
	}
}
