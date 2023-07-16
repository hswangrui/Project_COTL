using Spine.Unity;
using UnityEngine;

public class RatooCheckSkin : MonoBehaviour
{
	public SkeletonAnimation spine;

	private void OnEnable()
	{
		CheckSkin();
	}

	public void CheckSkin()
	{
		if (!DataManager.Instance.RatooGivenHeart)
		{
			spine.Skeleton.SetSkin("normal");
		}
		else
		{
			spine.Skeleton.SetSkin("heart");
		}
		spine.skeleton.SetSlotsToSetupPose();
		spine.AnimationState.Apply(spine.skeleton);
	}
}
