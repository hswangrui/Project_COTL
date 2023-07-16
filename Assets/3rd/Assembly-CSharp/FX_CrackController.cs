using UnityEngine;

public class FX_CrackController : BaseMonoBehaviour
{
	public float Offset;

	public AnimationCurve OffsetOverDuration = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public float duration = 2f;

	public AnimationCurve WidthOverLength = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	[Range(0f, 1f)]
	public float Length;

	private float hLength;

	[Range(0f, 1f)]
	public float Width = 0.3f;

	[Range(0f, 1f)]
	public float Falloff = 0.2f;

	private float timer;

	public bool playOnEnable = true;

	public bool destroyOnFinish = true;

	public bool animate = true;

	public static readonly int OffsetID = Shader.PropertyToID("_Offset");

	public static readonly int WidthID = Shader.PropertyToID("_Width");

	public static readonly int LengthID = Shader.PropertyToID("_Length");

	public static readonly int FalloffID = Shader.PropertyToID("_Falloff");

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

	private void OnEnable()
	{
		timer = 0f;
	}

	private void Update()
	{
		if (!base.isActiveAndEnabled || !playOnEnable)
		{
			return;
		}
		timer += Time.deltaTime;
		if (timer > duration)
		{
			if (destroyOnFinish)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				timer = 0f;
			}
		}
		if (animate)
		{
			float offset = OffsetOverDuration.Evaluate(Mathf.Clamp01(timer / duration));
			Offset = offset;
		}
		ApplyProperties();
	}

	private void OnDisable()
	{
		if (destroyOnFinish)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnValidate()
	{
		ApplyProperties();
	}

	public void ApplyProperties()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer obj in componentsInChildren)
		{
			PropBlock.SetFloat(OffsetID, Offset);
			PropBlock.SetFloat(WidthID, Width);
			PropBlock.SetFloat(LengthID, Length);
			PropBlock.SetFloat(FalloffID, Falloff);
			obj.SetPropertyBlock(PropBlock);
			PropBlock.Clear();
		}
	}
}
