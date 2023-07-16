using I2.Loc;

public class ShowPlaceName : BaseMonoBehaviour
{
	[TermsPopup("")]
	public string PlaceName;

	public void Play()
	{
		HUD_DisplayName.Play(PlaceName);
	}
}
