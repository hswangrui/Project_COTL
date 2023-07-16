using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineMirror : BaseMonoBehaviour
{
	public Shader shader;

	private Material material;

	public Color OverlayColor = new Color(0.1607843f, 0.2509804f, 0.2784314f, 0.8470588f);

	public SkeletonAnimation MySpine;

	public SkeletonAnimation MirrorSpine;

	private MeshRenderer meshRenderer;

	private MeshRenderer mirrorMeshRenderer;

	public void Init(SkeletonAnimation MirrorSpine)
	{
		this.MirrorSpine = MirrorSpine;
	}

	private void Start()
	{
		MySpine.skeletonDataAsset = MirrorSpine.skeletonDataAsset;
		MySpine.Initialize(true);
		MySpine.skeleton.SetSkin(MirrorSpine.skeleton.Skin);
		MySpine.AnimationState.SetAnimation(0, MirrorSpine.AnimationName, MirrorSpine.AnimationState.GetCurrent(0).Loop);
		meshRenderer = GetComponent<MeshRenderer>();
		mirrorMeshRenderer = MirrorSpine.gameObject.GetComponent<MeshRenderer>();
		material = Object.Instantiate(mirrorMeshRenderer.material);
		material.shader = shader;
		MirrorSpine.AnimationState.Start += MirrorAnimation;
		Vector3 eulerAngles = MirrorSpine.transform.eulerAngles;
		eulerAngles.z *= -1f;
		base.transform.eulerAngles = eulerAngles;
	}

	private void OnWillRenderObject()
	{
	}

	private void MirrorAnimation(TrackEntry trackEntry)
	{
		MySpine.AnimationState.SetAnimation(0, MirrorSpine.AnimationName, MirrorSpine.AnimationState.GetCurrent(0).Loop);
	}

	private void LateUpdate()
	{
		if (meshRenderer.enabled != mirrorMeshRenderer.enabled)
		{
			meshRenderer.enabled = mirrorMeshRenderer.enabled;
		}
		if (MySpine.skeleton.ScaleX != MirrorSpine.skeleton.ScaleX)
		{
			MySpine.skeleton.ScaleX = MirrorSpine.skeleton.ScaleX;
		}
	}
}
