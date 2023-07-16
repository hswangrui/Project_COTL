using System;
using System.Collections;
using I2.Loc;
using Lamb.UI;
using Map;
using Spine.Unity;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class Interaction_MonsterHeart : Interaction
{
	public delegate void HeartTaken();

	public Vector3 MoveToPosition;

	private bool Enabled = true;

	private PlayerFarming playerFarming;

	public SimpleSpineAnimator simpleSpineAnimator;

	private string sMonsterHeart;

	public Interaction_Chest Chest;

	private Coroutine heartBeatingRoutine;

	public Objectives.CustomQuestTypes ObjectiveToComplete;

	public event HeartTaken OnHeartTaken;

	private void Start()
	{
		UpdateLocalisation();
		heartBeatingRoutine = StartCoroutine(BeatingHeartRoutine());
	}

	private IEnumerator BeatingHeartRoutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.4f);
			AudioManager.Instance.PlayOneShot("event:/monster_heart/monster_heart_beat");
			yield return new WaitForSeconds(0.7f);
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sMonsterHeart = ScriptLocalization.Interactions.MonsterHeart;
	}

	public override void GetLabel()
	{
		base.Label = (Enabled ? sMonsterHeart : "");
	}

	public void Play(InventoryItem.ITEM_TYPE forceReward = InventoryItem.ITEM_TYPE.NONE)
	{
		if (!DataManager.Instance.BossesCompleted.Contains(PlayerFarming.Location) && !DungeonSandboxManager.Active)
		{
			base.enabled = true;
			return;
		}
		if (DungeonSandboxManager.Active)
		{
			forceReward = ((MapManager.Instance.CurrentMap.GetFinalBossNode() == MapManager.Instance.CurrentNode) ? InventoryItem.ITEM_TYPE.GOD_TEAR : InventoryItem.ITEM_TYPE.NONE);
		}
		Chest.RevealBossReward(forceReward);
		if (DungeonSandboxManager.Active)
		{
			DungeonSandboxManager.Instance.BossesCompleted.Add(PlayerFarming.Location);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (Enabled)
		{
			GameManager.GetInstance().OnConversationNew(false);
			GameManager.GetInstance().OnConversationNext(state.gameObject, 7f);
			playerFarming = state.GetComponent<PlayerFarming>();
			GameObject gameObject = new GameObject();
			gameObject.transform.position = base.transform.position + MoveToPosition;
			playerFarming.GoToAndStop(gameObject, base.gameObject, false, false, delegate
			{
				StartCoroutine(TakeHeart());
			});
			Enabled = false;
		}
	}

	private IEnumerator TakeHeart()
	{
		playerFarming.state.CURRENT_STATE = StateMachine.State.InActive;
		playerFarming.state.facingAngle = Utils.GetAngle(playerFarming.transform.position, base.transform.position);
		playerFarming.CustomAnimation("get-heart", false);
		playerFarming.simpleSpineAnimator.OnSpineEvent += OnSpineEvent;
		AudioManager.Instance.PlayOneShot("event:/monster_heart/monster_heart_sequence");
		StopCoroutine(heartBeatingRoutine);
		yield return new WaitForSeconds(3f);
		HeartHasBeenTaken();
	}

	private void OnSpineEvent(string EventName)
	{
		switch (EventName)
		{
		case "take-heart":
		{
			CameraManager.shakeCamera(0.3f, Utils.GetAngle(playerFarming.transform.position, base.transform.position));
			if (simpleSpineAnimator != null)
			{
				simpleSpineAnimator.Animate("dead-noheart", 0, false);
				break;
			}
			SkeletonAnimation componentInChildren = GetComponentInChildren<SkeletonAnimation>();
			if ((bool)componentInChildren)
			{
				componentInChildren.AnimationState.SetAnimation(0, "dead-noheart", false);
			}
			break;
		}
		case "monster-heart-sound":
			AudioManager.Instance.PlayOneShot("event:/monster_heart/monster_heart_beat", base.transform.position);
			break;
		case "monster-heart-zoom":
			GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
			break;
		}
	}

	private void HeartHasBeenTaken()
	{
		if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.MonsterHeart))
		{
			UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.MonsterHeart);
			uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
			{
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/MonsterHeart", Objectives.CustomQuestTypes.MonsterHeart), true);
				if ((bool)ChainDoor.Instance)
				{
					ChainDoor.Instance.Play(delegate
					{
						StartCoroutine(DoHeartTaken());
					});
				}
				else
				{
					StartCoroutine(DoHeartTaken());
				}
			});
		}
		else if ((bool)ChainDoor.Instance)
		{
			ChainDoor.Instance.Play(delegate
			{
				StartCoroutine(DoHeartTaken());
			});
		}
		else
		{
			StartCoroutine(DoHeartTaken());
		}
	}

	private IEnumerator DoHeartTaken()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			allBrain.AddThought(Thought.InAweOfLeaderChain);
		}
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/door/goop_door_unlock");
		GameManager.GetInstance().OnConversationEnd();
		Debug.Log("TAKEN?");
		Interaction_Chest chest = Chest;
		if ((object)chest != null)
		{
			chest.RevealBossReward(InventoryItem.ITEM_TYPE.NONE);
		}
		yield return new WaitForSeconds(0.5f);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.KillABossAndTakeTheirHeart);
		if (ObjectiveToComplete != 0)
		{
			ObjectiveManager.CompleteCustomObjective(ObjectiveToComplete);
		}
		Inventory.AddItem(22, 1);
		DataManager.Instance.BossesCompleted.Add(PlayerFarming.Location);
		HeartTaken onHeartTaken = this.OnHeartTaken;
		if (onHeartTaken != null)
		{
			onHeartTaken();
		}
		UnityEngine.Object.Destroy(this);
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Utils.DrawCircleXY(base.transform.position + MoveToPosition, 0.4f, Color.blue);
	}
}
