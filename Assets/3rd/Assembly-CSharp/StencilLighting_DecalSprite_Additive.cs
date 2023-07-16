using UnityEngine;

[ExecuteInEditMode]
public class StencilLighting_DecalSprite_Additive : BaseMonoBehaviour
{
	[Range(1f, 15f)]
	public float BlendHeight = 1f;

	[Range(1f, 2f)]
	public float projScale = 1.27f;

	[ColorUsage(true, true)]
	public Color AdditiveColor = Color.white;

	public Texture TextureOverride;

	public bool renderToStencilTexture = true;

	public bool hideChild = true;

	public bool constrainChildTransform = true;

	public bool negativeScaleFix = true;

	public static readonly int overrideInfluenceID = Shader.PropertyToID("_StencilInfluence");

	public static readonly int overrideColorID = Shader.PropertyToID("_ColorInstanceOverride");

	public static readonly int overrideProjScaleID = Shader.PropertyToID("_ProjectionScale");

	public static readonly int overrideTextureID = Shader.PropertyToID("_MainTex");

	private Transform m_targetDecalObj;

	private Transform m_targetAddObj;

	private Renderer m_DecalRenderer;

	private Renderer m_AddRenderer;

	private MaterialPropertyBlock m_propBlock;

	public MaterialPropertyBlock PropBlock
	{
		get
		{
			if (m_propBlock == null)
			{
				m_propBlock = new MaterialPropertyBlock();
			}
			return m_propBlock;
		}
	}

	public void Init()
	{
		m_targetDecalObj = base.gameObject.transform.GetChild(0);
		m_targetAddObj = m_targetDecalObj.GetChild(0);
		m_DecalRenderer = m_targetDecalObj.GetComponent<MeshRenderer>();
		m_AddRenderer = m_targetAddObj.GetComponent<MeshRenderer>();
	}

	public void ApplyProperties()
	{
		m_DecalRenderer.enabled = renderToStencilTexture;
		PropBlock.Clear();
		PropBlock.SetFloat(overrideInfluenceID, AdditiveColor.a);
		PropBlock.SetFloat(overrideProjScaleID, projScale);
		if (TextureOverride != null)
		{
			PropBlock.SetTexture(overrideTextureID, TextureOverride);
		}
		if (m_DecalRenderer != null && m_DecalRenderer.enabled)
		{
			m_DecalRenderer.SetPropertyBlock(PropBlock, 0);
		}
		PropBlock.Clear();
		PropBlock.SetColor(overrideColorID, AdditiveColor);
		PropBlock.SetFloat(overrideInfluenceID, 1f);
		PropBlock.SetFloat(overrideProjScaleID, projScale);
		if (TextureOverride != null)
		{
			PropBlock.SetTexture(overrideTextureID, TextureOverride);
		}
		if (m_AddRenderer != null)
		{
			m_AddRenderer.SetPropertyBlock(PropBlock, 0);
		}
	}

	private void Awake()
	{
		Init();
		if (negativeScaleFix)
		{
			NegativeScaleFix();
		}
	}

	private void NegativeScaleFix()
	{
		Vector3 vector = new Vector3(m_targetDecalObj.localScale.x, m_targetDecalObj.localScale.y, m_targetDecalObj.localScale.z);
		if (m_targetDecalObj.lossyScale.x < 0f)
		{
			vector.x *= -1f;
		}
		if (m_targetDecalObj.lossyScale.y < 0f)
		{
			vector.y *= -1f;
		}
		if (m_targetDecalObj.lossyScale.z < 0f)
		{
			vector.z *= -1f;
		}
	}

	private void Start()
	{
		Init();
		ApplyProperties();
	}

	private void OnDrawGizmos()
	{
		if (!(m_DecalRenderer == null))
		{
			Gizmos.color = Color.red;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}
