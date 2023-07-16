using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class VelocityTextureOffset : BaseMonoBehaviour
{
	private Vector3 lastPos;

	private Vector3 transformSpeed = Vector3.zero;

	public float OffsetScale = 1f;

	private float moveSpeed;

	private TrailRenderer _renderer;

	public static readonly int velocityOffsetID = Shader.PropertyToID("_VelocityOffset");

	public bool randomizeStartOffset = true;

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

	private void Start()
	{
		if (_renderer == null)
		{
			_renderer = GetComponent<TrailRenderer>();
		}
		lastPos = base.transform.position;
		if (randomizeStartOffset)
		{
			Vector4 vector = _renderer.material.GetVector(velocityOffsetID);
			_renderer.material.SetVector(velocityOffsetID, new Vector4(vector.x, vector.y, Random.Range(0f, 1f), Random.Range(0f, 1f)));
		}
	}

	private void Update()
	{
		transformSpeed = base.transform.position - lastPos;
		transformSpeed /= Time.deltaTime;
		moveSpeed = transformSpeed.magnitude;
		float num = 100f / (1f / Time.deltaTime);
		Vector4 vector = _renderer.material.GetVector(velocityOffsetID);
		_renderer.material.SetVector(velocityOffsetID, new Vector4(vector.x, vector.y, vector.z - num * OffsetScale * 0.01f * moveSpeed * vector.x, vector.w));
		lastPos = base.transform.position;
	}
}
