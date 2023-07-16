public class ActivatePerArea : BaseMonoBehaviour
{
	public FollowerLocation ActiveLocation = FollowerLocation.None;

	public FollowerLocation currentLocation = FollowerLocation.None;

	private void OnEnable()
	{
		currentLocation = PlayerFarming.Location;
		if (currentLocation == ActiveLocation)
		{
			base.gameObject.SetActive(true);
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}
}
