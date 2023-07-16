using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Interaction_CatchSpider : Interaction
{
	public CritterSpider CritterSpider;

	public bool isCrab;

	private const int spidersRequiredForEasterEgg = 20;

	private const int dayRequiredForEasterEgg = 15;

	private string sString;

	private bool Activating;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 1f;
		if (isCrab && DataManager.GetFollowerSkinUnlocked("Crab"))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.CatchCritter;
	}

	public override void GetLabel()
	{
		base.Label = (Activating ? "" : sString);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		GameManager.GetInstance().StartCoroutine(CatchCritterRoutine());
		if (!isCrab)
		{
			DataManager.Instance.SpidersCaught++;
		}
	}

	private IEnumerator CatchCritterRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/player/weed_pick", base.transform.position);
		Activating = true;
		CritterSpider.enabled = false;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "catch-critter", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		base.transform.DOMove(PlayerFarming.Instance.transform.position, 1f);
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider_small/gethit", base.transform.position);
		if (isCrab)
		{
			if (!DataManager.GetFollowerSkinUnlocked("Crab"))
			{
				FollowerSkinCustomTarget.Create(base.transform.position, PlayerFarming.Instance.transform.position, 0.4f, "Crab", null);
			}
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FISH_CRAB, 1, base.transform.position);
		}
		else
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.MEAT_MORSEL, 1, base.transform.position);
		}
		CritterSpider.Inventory.DropItem();
		base.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.4f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		base.gameObject.Recycle();
	}

	private IEnumerator Delay(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}
}
