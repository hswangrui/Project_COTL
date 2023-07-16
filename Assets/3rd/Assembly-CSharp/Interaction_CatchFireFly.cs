using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Interaction_CatchFireFly : Interaction
{
	private const int AmountForSkin = 15;

	public CritterBee CritterBee;

	private LayerMask collisionMask;

	private string sString;

	private bool Activating;

	private int maxRange = 4;

	public void OnValidate()
	{
		CritterBee.IsFireFly = true;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 1f;
		if (Random.Range(1, 20) == 15)
		{
			maxRange = 10;
			base.transform.localScale = Vector3.one * 1.5f;
		}
		else
		{
			maxRange = 4;
			base.transform.localScale = Vector3.one;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Island"));
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Obstacles"));
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
	}

	private IEnumerator CatchCritterRoutine()
	{
		DataManager.Instance.TotalFirefliesCaught++;
		if (DataManager.Instance.TotalFirefliesCaught >= 15 && !DataManager.Instance.FollowerSkinsUnlocked.Contains("Butterfly"))
		{
			StartCoroutine(UnlockSkinIE());
			yield break;
		}
		Activating = true;
		CritterBee.enabled = false;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "catch-critter", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		base.transform.DOMove(PlayerFarming.Instance.CameraBone.transform.position, 0.5f);
		AudioManager.Instance.PlayOneShot("event:/player/weed_pick", base.transform.position);
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlayOneShot("event:/player/catch_firefly", base.transform.position);
		base.gameObject.SetActive(false);
		for (int i = 0; i < Random.Range(1, maxRange); i++)
		{
			yield return new WaitForSeconds(0.05f);
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(state.gameObject, CritterBee.spriteRenderer.transform.position, Color.white, delegate
				{
					PlayerFarming.Instance.GetSoul(1);
				});
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			}
		}
		yield return new WaitForSeconds(0.3f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		base.gameObject.Recycle();
	}

	private IEnumerator UnlockSkinIE()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		Vector3 targetPosition;
		if (PlayerFarming.Instance.gameObject.transform.position.x < base.transform.position.x)
		{
			float distance = Vector3.Distance(base.transform.position, base.transform.position + Vector3.left);
			Vector3 normalized = (base.transform.position + Vector3.left - base.transform.position).normalized;
			targetPosition = ((Physics2D.Raycast(base.transform.position, normalized, distance, collisionMask).collider != null) ? (base.transform.position + Vector3.right) : (base.transform.position + Vector3.left));
		}
		else
		{
			float distance2 = Vector3.Distance(base.transform.position, base.transform.position + Vector3.right);
			Vector3 normalized2 = (base.transform.position + Vector3.right - base.transform.position).normalized;
			targetPosition = ((Physics2D.Raycast(base.transform.position, normalized2, distance2, collisionMask).collider != null) ? (base.transform.position + Vector3.left) : (base.transform.position + Vector3.right));
		}
		PlayerFarming.Instance.playerController.speed = 0f;
		PlayerFarming.Instance.GoToAndStop(targetPosition, base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/Stings/upgrade_cult", base.transform.position);
		GetComponent<CritterBee>().enabled = false;
		Vector3 pos = base.transform.GetChild(0).transform.localPosition;
		float dur = 3f;
		float t = 0f;
		while (t < dur)
		{
			t += Time.deltaTime;
			base.transform.GetChild(0).transform.localPosition = pos + Random.insideUnitSphere * (t / dur) * 0.05f;
			yield return null;
		}
		base.gameObject.SetActive(false);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(base.transform.GetChild(0).transform.position + Vector3.down * 0.25f, Vector3.one);
		AudioManager.Instance.PlayOneShot("event:/player/catch_firefly", base.transform.position);
		FollowerSkinCustomTarget.Create(base.transform.GetChild(0).transform.position - Vector3.back * 0.5f, PlayerFarming.Instance.transform.position, 1f, "Butterfly", delegate
		{
			GameManager.GetInstance().OnConversationEnd();
			base.gameObject.Recycle();
		});
	}
}
