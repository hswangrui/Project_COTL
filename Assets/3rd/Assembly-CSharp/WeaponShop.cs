using Spine.Unity;
using UnityEngine;

public class WeaponShop : MonoBehaviour
{
	public SkeletonAnimation Spine;

	[SerializeField]
	private string VOtoPlay = "event:/dialogue/shop_weapons/buy_weaponshop";

	public void ChooseObject()
	{
		AudioManager.Instance.PlayOneShot(VOtoPlay);
		Spine.AnimationState.SetAnimation(0, "talk-yes", false);
		Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
	}
}
