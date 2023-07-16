using UnityEngine;

[ExecuteInEditMode]
public class StencilLighting_DecalSprite_Effects_A : BaseMonoBehaviour
{
	[Range(1f, 15f)]
	public float BlendHeight = 1f;

	[Range(1f, 2f)]
	public float projScale = 1.27f;

	[Range(0f, 360f)]
	public float maskRotation;

	public Texture MaskShape;

	public bool constrainChildTransform = true;

	public static readonly int scalePosOffsetID = Shader.PropertyToID("_ScalePosOffset");

	public static readonly int maskRotationID = Shader.PropertyToID("_MaskRotation");

	public static readonly int overrideProjScaleID = Shader.PropertyToID("_ProjectionScale");

	public static readonly int overrideMaskID = Shader.PropertyToID("_MaskTex");

	private Transform m_targetDecalObj;

	private Renderer m_DecalRenderer;

	private Vector4 scalePosOffset;

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
		m_DecalRenderer = m_targetDecalObj.GetComponent<MeshRenderer>();
	}

	public void ApplyProperties()
	{
		PropBlock.Clear();
		scalePosOffset = new Vector4(base.transform.localScale.x, base.transform.localScale.y, base.transform.position.x, base.transform.position.y);
		PropBlock.SetVector(scalePosOffsetID, scalePosOffset);
		PropBlock.SetFloat(overrideProjScaleID, projScale);
		PropBlock.SetFloat(maskRotationID, maskRotation);
		if (MaskShape != null)
		{
			PropBlock.SetTexture(overrideMaskID, MaskShape);
		}
		if (m_DecalRenderer != null)
		{
			m_DecalRenderer.SetPropertyBlock(PropBlock, 0);
		}
	}

	private void Awake()
	{
		Init();
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
