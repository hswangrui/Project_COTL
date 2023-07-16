using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ForceShadowAttenuation : BaseMonoBehaviour
{
	private Material dummyMat;

	private Mesh dummyMesh;

	private MeshRenderer m_rend
	{
		get
		{
			return base.gameObject.GetComponent<MeshRenderer>();
		}
	}

	private MeshFilter m_filter
	{
		get
		{
			return base.gameObject.GetComponent<MeshFilter>();
		}
	}

	private void Awake()
	{
		Init();
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (dummyMat == null)
		{
			dummyMat = new Material(Shader.Find("Hidden/ForwardBaseDummy"));
		}
		if (m_rend.sharedMaterial == null)
		{
			m_rend.sharedMaterial = dummyMat;
		}
		if (dummyMesh == null)
		{
			dummyMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
		}
		m_rend.receiveShadows = true;
		m_rend.shadowCastingMode = ShadowCastingMode.On;
		dummyMesh.RecalculateBounds();
		m_filter.sharedMesh = dummyMesh;
	}

	public void OnDrawGizmos()
	{
		Bounds bounds = default(Bounds);
		bounds = m_rend.bounds;
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
