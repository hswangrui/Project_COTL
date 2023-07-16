public class PickUpSoulFragment : PickUp
{
	public delegate void CollectSoulFragment();

	public static CollectSoulFragment OnCollectSoulFragment;

	public override void PickMeUp()
	{
		base.PickMeUp();
		CollectSoulFragment onCollectSoulFragment = OnCollectSoulFragment;
		if (onCollectSoulFragment != null)
		{
			onCollectSoulFragment();
		}
	}
}
