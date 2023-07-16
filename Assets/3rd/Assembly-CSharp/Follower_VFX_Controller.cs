using System.Collections;
using Spine.Unity;
using UnityEngine;

public class Follower_VFX_Controller : BaseMonoBehaviour
{
	public GameObject promoteFX;

	public SkeletonAnimation spine;

	public SimpleSpineAnimator simpleSpineAnimator;

	private int followerStartingLayer;

	private int playerStartingLayer;

	private void Start()
	{
	}

	public void PromoteMeVFX()
	{
		promoteFX.SetActive(true);
		followerStartingLayer = spine.gameObject.layer;
		playerStartingLayer = PlayerFarming.Instance.gameObject.layer;
		StopAllCoroutines();
		StartCoroutine(PromoteVFX());
	}

	private IEnumerator PromoteVFX()
	{
		yield return new WaitForSeconds(1f);
		spine.gameObject.layer = LayerMask.NameToLayer("VFX");
		PlayerFarming.Instance.gameObject.layer = LayerMask.NameToLayer("VFX");
		BiomeConstants.Instance.ImpactFrameForIn();
		simpleSpineAnimator.FlashWhite(true);
		yield return new WaitForSeconds(2f);
		simpleSpineAnimator.FlashWhite(false);
		BiomeConstants.Instance.ImpactFrameForOut();
		spine.gameObject.layer = followerStartingLayer;
		PlayerFarming.Instance.gameObject.layer = playerStartingLayer;
	}

	private void Update()
	{
	}
}
