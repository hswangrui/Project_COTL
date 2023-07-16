using Spine.Unity;
using UnityEngine;

public class MidasCheckSkin : MonoBehaviour
{
	private SkeletonAnimation spine;

	private void OnEnable()
	{
		spine = GetComponent<SkeletonAnimation>();
		CheckSkin();
	}

	public void CheckSkin()
	{
		if (DataManager.Instance.MidasBeaten)
		{
			spine.Skeleton.SetSkin("Beaten");
		}
		else
		{
			spine.Skeleton.SetSkin("Normal");
		}
		spine.skeleton.SetSlotsToSetupPose();
		spine.AnimationState.Apply(spine.skeleton);
	}
}
