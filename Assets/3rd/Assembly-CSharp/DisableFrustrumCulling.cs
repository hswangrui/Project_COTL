using UnityEngine;

public class DisableFrustrumCulling : BaseMonoBehaviour
{
	public bool DisableCulling;

	public Vector3 myMaxBoundsCenter;

	public Vector3 myMaxBoundsSize;

	private SkinnedMeshRenderer skinnedMeshRenderer;

	private void OnEnable()
	{
		if (DisableCulling)
		{
			skinnedMeshRenderer = base.gameObject.GetComponent<SkinnedMeshRenderer>();
			Bounds localBounds = new Bounds(myMaxBoundsCenter, myMaxBoundsSize);
			skinnedMeshRenderer.localBounds = localBounds;
		}
	}
}
