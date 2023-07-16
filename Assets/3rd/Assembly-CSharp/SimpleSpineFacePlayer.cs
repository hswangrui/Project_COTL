using Spine.Unity;

public class SimpleSpineFacePlayer : BaseMonoBehaviour
{
	private SkeletonAnimation Spine;

	private void Start()
	{
		Spine = GetComponentInChildren<SkeletonAnimation>();
	}

	private void Update()
	{
		if (Spine != null && PlayerFarming.Instance != null)
		{
			Spine.skeleton.ScaleX = ((PlayerFarming.Instance.transform.position.x < base.transform.position.x) ? 1 : (-1));
		}
	}
}
