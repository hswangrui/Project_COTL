using Spine.Unity;
using UnityEngine;

public class ManualUpdateSkeletonAnimation : BaseMonoBehaviour
{
	public SkeletonAnimation skeletonAnimation;

	[Range(1f / 60f, 0.125f)]
	public float timeInterval = 1f / 24f;

	private float deltaTime;

	private void Start()
	{
		if (skeletonAnimation == null)
		{
			skeletonAnimation = GetComponent<SkeletonAnimation>();
		}
		skeletonAnimation.Initialize(false);
		skeletonAnimation.clearStateOnDisable = false;
		skeletonAnimation.enabled = false;
		ManualUpdate();
	}

	private void Update()
	{
		deltaTime += Time.deltaTime;
		if (deltaTime >= timeInterval)
		{
			ManualUpdate();
		}
	}

	private void ManualUpdate()
	{
		skeletonAnimation.Update(deltaTime);
		skeletonAnimation.LateUpdate();
		deltaTime -= timeInterval;
	}
}
