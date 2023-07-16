using UnityEngine;

[CreateAssetMenu(menuName = "COTL/BiomeLightingSettings", fileName = "BiomeLightingSettings_")]
public class BiomeLightingSettings : ScriptableObject
{
	[Header("Time")]
	[SerializeField]
	private bool m_unscaledTime;

	[Header("Ambient")]
	[ColorUsage(false, true)]
	[SerializeField]
	private Color m_ambientColour = Color.grey;

	[Header("Directional Light")]
	[ColorUsage(false, false)]
	[SerializeField]
	private Color m_directionalLightColor = Color.white;

	[SerializeField]
	private float m_directionalLightIntensity = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_ShadowStrength = 0.5f;

	[SerializeField]
	private Vector3 m_lightRotation = Vector3.zero;

	[Header("Lighting Ramp Colours")]
	[SerializeField]
	private Color m_globalHighlightColor = new Color(0.8f, 0.8f, 0.8f, 1f);

	[SerializeField]
	private Color m_globalShadowColor = new Color(0f, 0f, 0f, 1f);

	[Header("StencilLighting LUT Effects")]
	[SerializeField]
	private Texture m_LutTexture_Shadow;

	[SerializeField]
	private Texture m_LutTexture_Lit;

	[Range(0f, 2f)]
	[SerializeField]
	private float m_StencilInfluence = 0.5f;

	[SerializeField]
	private float m_exposure = 1.15f;

	[Header("Fog")]
	[SerializeField]
	private Color m_fogColor = new Color(0.0741f, 0f, 0.129f, 1f);

	[SerializeField]
	private Vector2 m_fogDist = new Vector2(10f, 15f);

	[SerializeField]
	private float m_fogHeight = 0.5f;

	[SerializeField]
	private float m_fogSpread = 1f;

	[Header("God Rays Color")]
	[SerializeField]
	private Color m_godRayColor = Color.white;

	[Header("Screenspace Shadows")]
	[SerializeField]
	private Material m_ScreenSpaceOverlayMat;

	[HideInInspector]
	public OverrideLightingProperties overrideLightingProperties = new OverrideLightingProperties();

	public bool UnscaledTime
	{
		get
		{
			return m_unscaledTime;
		}
		set
		{
			m_unscaledTime = value;
		}
	}

	public Color AmbientColour
	{
		get
		{
			return m_ambientColour;
		}
		set
		{
			m_ambientColour = value;
		}
	}

	public Color DirectionalLightColour
	{
		get
		{
			return m_directionalLightColor;
		}
		set
		{
			m_directionalLightColor = value;
		}
	}

	public float DirectionalLightIntensity
	{
		get
		{
			return m_directionalLightIntensity;
		}
		set
		{
			m_directionalLightIntensity = value;
		}
	}

	public float ShadowStrength
	{
		get
		{
			return m_ShadowStrength;
		}
		set
		{
			m_ShadowStrength = value;
		}
	}

	public Vector3 LightRotation
	{
		get
		{
			return m_lightRotation;
		}
		set
		{
			m_lightRotation = value;
		}
	}

	public Color GlobalHighlightColor
	{
		get
		{
			return m_globalHighlightColor;
		}
		set
		{
			m_globalHighlightColor = value;
		}
	}

	public Color GlobalShadowColor
	{
		get
		{
			return m_globalShadowColor;
		}
		set
		{
			m_globalShadowColor = value;
		}
	}

	public Texture LutTexture_Shadow
	{
		get
		{
			return m_LutTexture_Shadow;
		}
		set
		{
			m_LutTexture_Shadow = value;
		}
	}

	public Texture LutTexture_Lit
	{
		get
		{
			return m_LutTexture_Lit;
		}
		set
		{
			m_LutTexture_Lit = value;
		}
	}

	public float StencilInfluence
	{
		get
		{
			return m_StencilInfluence;
		}
		set
		{
			m_StencilInfluence = value;
		}
	}

	public float Exposure
	{
		get
		{
			return m_exposure;
		}
		set
		{
			m_exposure = value;
		}
	}

	public Color FogColor
	{
		get
		{
			return m_fogColor;
		}
		set
		{
			m_fogColor = value;
		}
	}

	public Vector2 FogDist
	{
		get
		{
			return m_fogDist;
		}
		set
		{
			m_fogDist = value;
		}
	}

	public float FogHeight
	{
		get
		{
			return m_fogHeight;
		}
		set
		{
			m_fogHeight = value;
		}
	}

	public float FogSpread
	{
		get
		{
			return m_fogSpread;
		}
		set
		{
			m_fogSpread = value;
		}
	}

	public Color GodRayColor
	{
		get
		{
			return m_godRayColor;
		}
		set
		{
			m_godRayColor = value;
		}
	}

	public Material ScreenSpaceOverlayMat
	{
		get
		{
			return m_ScreenSpaceOverlayMat;
		}
		set
		{
			m_ScreenSpaceOverlayMat = value;
		}
	}

	public bool IsEquivalent(BiomeLightingSettings other)
	{
		if (m_ambientColour == other.AmbientColour && m_directionalLightColor == other.DirectionalLightColour && m_directionalLightIntensity == other.DirectionalLightIntensity && m_ShadowStrength == other.ShadowStrength && !overrideLightingProperties.LightRotation && !other.overrideLightingProperties.LightRotation && m_globalHighlightColor == other.GlobalHighlightColor && m_globalShadowColor == other.GlobalShadowColor && m_LutTexture_Shadow == other.LutTexture_Shadow && m_LutTexture_Lit == other.LutTexture_Lit && m_StencilInfluence == other.StencilInfluence && m_exposure == other.Exposure && m_fogColor == other.FogColor && m_fogDist == other.FogDist && m_fogHeight == other.FogHeight && m_fogSpread == other.FogSpread && m_godRayColor == other.GodRayColor && m_ScreenSpaceOverlayMat == other.ScreenSpaceOverlayMat)
		{
			return true;
		}
		return false;
	}
}
