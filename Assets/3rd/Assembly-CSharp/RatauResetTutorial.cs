public class RatauResetTutorial : BaseMonoBehaviour
{
	public void Play()
	{
		PlayerFarming.Instance.health.GodMode = Health.CheatMode.None;
	}
}
