using UnityEngine;

[ExecuteInEditMode]
public class StencilLighting_DecalSprite : BaseMonoBehaviour
{
	[Range(1f, 15f)]
	public float BlendHeight = 1f;

	public bool useInfluenceOverride;

	[SerializeField]
	public float overrideInfluence = 1f;

	private float oldInfluence;

	public bool hideChild = true;

	public bool constrainChildTransform = true;

	public bool negativeScaleFix = true;

	public static readonly int overrideInfluenceID = Shader.PropertyToID("_StencilInfluence");

	private Transform m_targetDecalObj;

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
		oldInfluence = overrideInfluence;
		m_targetDecalObj = base.gameObject.transform.GetChild(0);
		m_renderer = m_targetDecalObj.GetComponent<MeshRenderer>();
	}

	public void ApplyProperties()
	{
		PropBlock.Clear();
		PropBlock.SetFloat(overrideInfluenceID, overrideInfluence);
		if (m_renderer != null)
		{
			m_renderer.SetPropertyBlock(PropBlock, 0);
		}
		oldInfluence = overrideInfluence;
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

	private void ResetRotation()
	{
		if (Application.isPlaying)
		{
			Transform parent = m_targetDecalObj.parent;
			m_targetDecalObj.parent = null;
			m_targetDecalObj.localRotation = Quaternion.Euler(new Vector3(-135f, 0f, 0f));
			m_targetDecalObj.parent = parent;
		}
	}

	private void Awake()
	{
		Init();
		if (negativeScaleFix)
		{
			NegativeScaleFix();
		}
		if (constrainChildTransform)
		{
			ResetRotation();
		}
	}

	private void Start()
	{
		Init();
		if (useInfluenceOverride)
		{
			ApplyProperties();
		}
	}

	private void OnDrawGizmos()
	{
		if (!(m_renderer == null))
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}
