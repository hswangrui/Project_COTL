using UnityEngine;

public class ParticleDisableLightModulePerPlatform : MonoBehaviour
{
	public bool disableOnDesktop;

	public bool disableOnConsole;

	public bool disableOnSwitch;

	public bool disableOnLowQuality;

	private ParticleSystem particleSystem;

	private void Start()
	{
		if (particleSystem == null)
		{
			particleSystem = GetComponent<ParticleSystem>();
		}
		ParticleSystem.LightsModule lights = particleSystem.lights;
		if (disableOnLowQuality && SettingsManager.Settings.Graphics.EnvironmentDetail == 0)
		{
			lights.enabled = false;
			return;
		}
		if (!disableOnDesktop)
		{
			base.gameObject.SetActive(true);
			return;
		}
		lights.enabled = false;
		base.gameObject.SetActive(false);
	}
}
