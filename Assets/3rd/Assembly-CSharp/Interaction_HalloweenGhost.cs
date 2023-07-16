using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class Interaction_HalloweenGhost : Interaction
{
	public static List<Interaction_HalloweenGhost> HalloweenGhosts = new List<Interaction_HalloweenGhost>();

	public SkeletonAnimation Spine;

	[SerializeField]
	private CritterBee critterBee;

	private FollowerOutfit outfit;

	private FollowerInfo followerInfo;

	private string sString;

	private LayerMask collisionMask;

	private bool destroying;

	private float checkDelayTimestamp;

	public FollowerInfo FollowerInfo
	{
		get
		{
			return followerInfo;
		}
	}

	public void Configure(FollowerInfo follower)
	{
		followerInfo = follower;
		outfit = new FollowerOutfit(follower);
		outfit.SetOutfit(Spine, FollowerOutfitType.Ghost, InventoryItem.ITEM_TYPE.NONE, false);
		Spine.Skeleton.A = 0.5f;
		Spine.AnimationState.SetAnimation(0, "Ghost/move-ghost", true);
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Island"));
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Obstacles"));
		UpdateLocalisation();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		HalloweenGhosts.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		HalloweenGhosts.Remove(this);
	}

	protected override void Update()
	{
		base.Update();
		if (!FollowerBrainStats.IsBloodMoon && !destroying)
		{
			destroying = true;
			DOTween.To(() => Spine.skeleton.A, delegate(float x)
			{
				Spine.skeleton.A = x;
			}, 0f, 1f).OnComplete(delegate
			{
				Object.Destroy(base.gameObject);
			});
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.Collect;
	}

	public override void GetLabel()
	{
		base.Label = ((Interactable && !destroying) ? sString : "");
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (!destroying)
		{
			Interactable = false;
			StartCoroutine(CatchGhost());
		}
	}

	private void FixedUpdate()
	{
		if (!(Time.time > checkDelayTimestamp))
		{
			return;
		}
		foreach (Follower follower in Follower.Followers)
		{
			if (Vector3.Distance(base.transform.position, follower.transform.position) < 2f)
			{
				if (follower.Brain.CurrentOverrideTaskType != 0)
				{
					return;
				}
				if (follower.Brain.GetTimeSinceTask(FollowerTaskType.ReactSpookedFromGhost) > 120f && TimeManager.TotalElapsedGameTime - StructureAndTime.GetOrAddTime(999, follower.Brain, StructureAndTime.IDTypes.Follower) > 600f)
				{
					StructureAndTime.SetTime(999, follower.Brain, StructureAndTime.IDTypes.Follower);
					follower.Brain.HardSwapToTask(new FollowerTask_ReactGhost());
					follower.FacePosition(base.transform.position);
				}
			}
		}
		checkDelayTimestamp = Time.time + 1f;
	}

	private IEnumerator CatchGhost()
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
		PlayerFarming.Instance.GoToAndStop(targetPosition, base.gameObject, false, false, delegate
		{
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("reeducate-3", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		});
		AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/start_ritual", base.transform.position);
		GetComponent<CritterBee>().enabled = false;
		float num = Mathf.Repeat(Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position), 360f);
		base.transform.localScale = new Vector3((num > 90f && num < 270f) ? 1 : (-1), 1f, 1f);
		Spine.AnimationState.SetAnimation(0, "Ghost/collect", false);
		yield return new WaitForSeconds(3f);
		base.gameObject.SetActive(false);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(base.transform.GetChild(0).transform.position, Vector3.one);
		AudioManager.Instance.PlayOneShot("event:/player/catch_firefly", base.transform.position);
		SeasonalEventData seasonalEventData = SeasonalEventManager.GetSeasonalEventData(SeasonalEventType.Halloween);
		List<string> unlockableSkins = seasonalEventData.GetUnlockableSkins();
		List<StructureBrain.TYPES> unlockableDecorations = seasonalEventData.GetUnlockableDecorations();
		if (unlockableSkins.Count > 0 && (Random.value > 0.5f || unlockableDecorations.Count == 0))
		{
			string skin = unlockableSkins[Random.Range(0, unlockableSkins.Count)];
			FollowerSkinCustomTarget.Create(PlayerFarming.Instance.transform.position, PlayerFarming.Instance.transform.position, 0f, skin, delegate
			{
				GameManager.GetInstance().OnConversationEnd();
				Object.Destroy(base.gameObject);
			});
			yield break;
		}
		if (unlockableDecorations.Count > 0)
		{
			StructureBrain.TYPES decoration = unlockableDecorations[Random.Range(0, unlockableDecorations.Count)];
			DecorationCustomTarget.Create(PlayerFarming.Instance.transform.position, PlayerFarming.Instance.transform.position, 0f, decoration, delegate
			{
				GameManager.GetInstance().OnConversationEnd();
				Object.Destroy(base.gameObject);
			});
			yield break;
		}
		for (int i = 0; i < Random.Range(3, 6); i++)
		{
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(state.gameObject, base.transform.position, Color.white, delegate
				{
					PlayerFarming.Instance.GetSoul(1);
				});
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			}
		}
		GameManager.GetInstance().OnConversationEnd();
		Object.Destroy(base.gameObject);
	}
}
