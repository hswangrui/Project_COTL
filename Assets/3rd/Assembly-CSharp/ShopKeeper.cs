using System.Collections;
using FMODUnity;
using Spine;
using Spine.Unity;
using UnityEngine;

public class ShopKeeper : MonoBehaviour
{
	public SkeletonAnimation ShopKeeperSpine;

	public GameObject shopKeeper;

	[SerializeField]
	private string _cantAffordAnimation = "cant-afford";

	[SerializeField]
	private string _buyAnimation = "buy";

	[SerializeField]
	private string _talkAnimation = "talk";

	[SerializeField]
	private string _normalAnimation = "animation";

	private Spine.Animation cantAffordAnimation;

	private Spine.Animation canAffordAnimation;

	private SkeletonAnimation skeletonAnimation;

	[SerializeField]
	private GameObject BoughtItemBark;

	[SerializeField]
	private GameObject NormalBark;

	[SerializeField]
	private GameObject CantAffordBark;

	[EventRef]
	public string saleVO;

	[EventRef]
	public string cantAffordVO;

	[EventRef]
	public string saleSfx = "event:/shop/buy";

	[EventRef]
	public string cantAffordSfx = "event:/ui/negative_feedback";

	private void Start()
	{
		cantAffordAnimation = ShopKeeperSpine.skeleton.Data.FindAnimation(_cantAffordAnimation);
		canAffordAnimation = ShopKeeperSpine.skeleton.Data.FindAnimation(_buyAnimation);
		skeletonAnimation = shopKeeper.GetComponent<SkeletonAnimation>();
	}

	private void Update()
	{
	}

	public IEnumerator cantAfford()
	{
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot(cantAffordSfx, base.transform.position);
		AudioManager.Instance.PlayOneShot(cantAffordVO, base.gameObject.transform.position);
		UpdateBark(CantAffordBark);
		if (ShopKeeperSpine == null && shopKeeper != null)
		{
			ShopKeeperSpine = skeletonAnimation;
		}
		if (!(ShopKeeperSpine == null) && ShopKeeperSpine.gameObject.activeInHierarchy && cantAffordAnimation != null)
		{
			ShopKeeperSpine.AnimationState.SetAnimation(0, cantAffordAnimation, false);
			ShopKeeperSpine.AnimationState.AddAnimation(0, _normalAnimation, true, 0f);
		}
	}

	private void UpdateBark(GameObject bark)
	{
		if (NormalBark != null)
		{
			NormalBark.SetActive(false);
		}
		if (BoughtItemBark != null)
		{
			BoughtItemBark.SetActive(false);
		}
		if (CantAffordBark != null)
		{
			CantAffordBark.SetActive(false);
		}
		bark.SetActive(true);
	}

	public IEnumerator boughtItem()
	{
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot(saleSfx, base.transform.position);
		AudioManager.Instance.PlayOneShot(saleVO, base.gameObject.transform.position);
		UpdateBark(BoughtItemBark);
		if (ShopKeeperSpine == null && shopKeeper != null && shopKeeper.activeInHierarchy)
		{
			ShopKeeperSpine = skeletonAnimation;
		}
		if (!(ShopKeeperSpine == null) && ShopKeeperSpine.gameObject.activeInHierarchy && canAffordAnimation != null)
		{
			ShopKeeperSpine.AnimationState.SetAnimation(0, canAffordAnimation, false);
			ShopKeeperSpine.AnimationState.AddAnimation(0, _normalAnimation, true, 0f);
			if (NormalBark != null)
			{
				NormalBark.SetActive(false);
			}
			if (BoughtItemBark != null)
			{
				BoughtItemBark.SetActive(true);
			}
		}
	}
}
