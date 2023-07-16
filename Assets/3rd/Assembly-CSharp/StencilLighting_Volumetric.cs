using UnityEngine;

[ExecuteInEditMode]
public class StencilLighting_Volumetric : BaseMonoBehaviour
{
	public bool useAdditiveColor;

	[SerializeField]
	[ColorUsage(false, true)]
	public Color AdditiveColor = Color.grey;

	[Range(0f, 1f)]
	public float AdditiveScale = 1f;

	public bool hideChild = true;

	public static readonly int overrideColorID = Shader.PropertyToID("_Color");

	private Transform m_targetObj;

	private Renderer m_renderer;

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
		m_targetObj = base.gameObject.transform.GetChild(0);
		m_targetObj.transform.localScale = new Vector3(AdditiveScale, AdditiveScale, AdditiveScale);
		m_renderer = m_targetObj.GetComponent<MeshRenderer>();
		m_targetObj.gameObject.SetActive(useAdditiveColor);
		if (useAdditiveColor)
		{
			ApplyProperties();
		}
	}

	public void ApplyProperties()
	{
		PropBlock.Clear();
		PropBlock.SetColor(overrideColorID, AdditiveColor);
		if (m_renderer != null)
		{
			m_renderer.SetPropertyBlock(PropBlock, 0);
		}
	}

	private void Awake()
	{
		Init();
		ApplyProperties();
		if (base.gameObject.layer != 15 && base.gameObject.layer != 20)
		{
			base.gameObject.layer = 20;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
	}
}
