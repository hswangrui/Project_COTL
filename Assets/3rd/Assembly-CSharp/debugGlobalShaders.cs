using UnityEngine;

public class debugGlobalShaders : BaseMonoBehaviour
{
	public float _GlobalTimeUnscaled;

	public Color _ItemInWoodsColor;

	public Vector2 windDirection;

	public float _WindSpeed;

	public float _WindDensity;

	public Texture _Lighting_RenderTexture;

	public float _VerticalFog_ZOffset;

	public float _VerticalFog_GradientSpread;

	public float _CloudDensity;

	public float _CloudAlpha;

	public Vector3 _PlayerPosition;

	public float _GlobalDither;

	private void Start()
	{
	}

	public void GetGlobalShaders()
	{
		_GlobalTimeUnscaled = Shader.GetGlobalFloat("_GlobalTimeUnscaled");
		windDirection = Shader.GetGlobalVector("_WindDiection");
		_WindSpeed = Shader.GetGlobalFloat("_WindSpeed");
		_WindDensity = Shader.GetGlobalFloat("_WindDensity");
		_Lighting_RenderTexture = Shader.GetGlobalTexture("_Lighting_RenderTexture");
		_VerticalFog_ZOffset = Shader.GetGlobalFloat("_VerticalFog_ZOffset");
		_VerticalFog_GradientSpread = Shader.GetGlobalFloat("_VerticalFog_GradientSpread");
		_ItemInWoodsColor = Shader.GetGlobalColor("_ItemInWoodsColor");
		_CloudDensity = Shader.GetGlobalFloat("_CloudDensity");
		_CloudAlpha = Shader.GetGlobalFloat("_CloudAlpha");
		_PlayerPosition = Shader.GetGlobalVector("_PlayerPosition");
		_GlobalDither = Shader.GetGlobalFloat("_GlobalDitherIntensity");
	}

	private void Update()
	{
		GetGlobalShaders();
	}
}
