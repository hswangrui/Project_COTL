using Spine.Unity;
using UnityEngine;

public class SpiderHeadManager : BaseMonoBehaviour
{
	public SkeletonAnimation Spine;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string Head;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string HeadFacingUp;

	private EnemySpider spider;

	private void Awake()
	{
		spider = GetComponentInParent<EnemySpider>();
	}

	private void LateUpdate()
	{
		if (!(spider == null) && (!spider.DisableForces || spider.Attacking) && !(PlayerFarming.Instance == null))
		{
			Vector3 zero = Vector3.zero;
			bool direction = (spider.Attacking ? spider.AttackingTargetPosition : ((spider.pathToFollow == null || spider.pathToFollow.Count <= 0) ? PlayerFarming.Instance.transform.position : spider.pathToFollow[spider.pathToFollow.Count - 1])).y > spider.transform.position.y;
			SetDirection(direction);
		}
	}

	private void SetDirection(bool up)
	{
		Spine.skeleton.SetSkin(up ? HeadFacingUp : Head);
		Spine.skeleton.SetSlotsToSetupPose();
	}
}
