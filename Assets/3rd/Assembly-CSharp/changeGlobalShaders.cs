using UnityEngine;

public class changeGlobalShaders : BaseMonoBehaviour
{
	public Color biomeColor;

	public float VerticalFog_ZOffset = 0.25f;

	public float VerticalFog_GradientSpread = 1f;

	public Vector2 windDirection = new Vector2(1f, 0f);

	public float windSpeed = 3f;

	public float windDensity = 0.1f;

	public float cloudDensity = 1f;

	public float _GlobalDitherIntensity = 1f;

	private void Start()
	{
	}

	private void applyShaders()
	{
		Shader.SetGlobalFloat("_VerticalFog_ZOffset", VerticalFog_ZOffset);
		Shader.SetGlobalFloat("_VerticalFog_GradientSpread", VerticalFog_GradientSpread);
		Shader.SetGlobalVector("_WindDiection", windDirection);
		Shader.SetGlobalFloat("_WindSpeed", windSpeed);
		Shader.SetGlobalFloat("_WindDensity", windDensity);
		Shader.SetGlobalFloat("_CloudDensity", cloudDensity);
		Shader.SetGlobalFloat("_GlobalDitherIntensity", _GlobalDitherIntensity);
	}

	private void Update()
	{
	}
}
