public class disablePerPlatform : BaseMonoBehaviour
{
	public bool disableOnDesktop;

	public bool disableOnConsole;

	public bool disableOnSwitch;

	public bool disableOn8thGenConsoles;

	public bool disableOnLowQuality;

	private void Awake()
	{
		if (disableOnLowQuality && SettingsManager.Settings.Graphics.EnvironmentDetail == 0)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (disableOnDesktop)
		{
			base.gameObject.SetActive(true);
			return;
		}
		bool disableOn8thGenConsole = disableOn8thGenConsoles;
		bool disableOnSwitch2 = disableOnSwitch;
		bool disableOnConsole2 = disableOnConsole;
	}
}
