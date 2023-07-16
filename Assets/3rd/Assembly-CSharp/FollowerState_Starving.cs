public class FollowerState_Starving : FollowerState
{
	public override FollowerStateType Type
	{
		get
		{
			return FollowerStateType.Starving;
		}
	}

	public override float MaxSpeed
	{
		get
		{
			return 1f;
		}
	}
}
