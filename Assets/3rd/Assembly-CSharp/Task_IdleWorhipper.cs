using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class Task_IdleWorhipper : Task
{
	private GameObject MoveToObject;

	private static List<Task_IdleWorhipper> Worshippers = new List<Task_IdleWorhipper>();

	public Worshipper w;

	public float ConversationCooledDown = 3f;

	private float VomitCooldown;

	private Vomit ClosestVomit;

	private float DeadWorshipperCooldown;

	private DeadWorshipper ClosestDeadWorshipper;

	private Task_IdleWorhipper cTarget;

	private IDAndRelationship cRelationship;

	private IDAndRelationship Relationship;

	private Vector3 cTargetPosition;

	private GameObject MoveToGameObject;

	private int Score;

	private GraphNode Node;

	private GraphNode cNode;

	private float HateThreshold
	{
		get
		{
			return Villager_Info.RelationshipHateThreshold;
		}
	}

	private float FriendThreshold
	{
		get
		{
			return Villager_Info.RelationshipFriendThreshold;
		}
	}

	private float LoveThreshold
	{
		get
		{
			return Villager_Info.RelationshipLoveThreshold;
		}
	}

	public override void StartTask(TaskDoer t, GameObject TargetObject)
	{
		base.StartTask(t, TargetObject);
		Worshippers.Add(this);
		w = t.GetComponent<Worshipper>();
		Type = Task_Type.NONE;
		t.InConversation = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		MoveToObject = new GameObject();
	}

	public override void ClearTask()
	{
		UnityEngine.Object.Destroy(MoveToObject);
		t.StopAllCoroutines();
		Timer = 0f;
		if (cTarget != null)
		{
			if (cTarget.t != null)
			{
				cTarget.t.StopAllCoroutines();
				cTarget.t.InConversation = false;
			}
			cTarget.Timer = 0f;
			cTarget.state.CURRENT_STATE = StateMachine.State.Idle;
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (MoveToGameObject != null)
		{
			UnityEngine.Object.Destroy(MoveToGameObject);
		}
		MoveToGameObject = null;
		t.ClearPaths();
		t.InConversation = false;
		base.ClearTask();
		Worshippers.Remove(this);
	}

	public override void TaskUpdate()
	{
		if (t.InConversation)
		{
			return;
		}
		ConversationCooledDown -= Time.deltaTime;
		if (state.CURRENT_STATE != 0)
		{
			return;
		}
		if ((Timer -= Time.deltaTime) < 0f)
		{
			Timer = UnityEngine.Random.Range(5f, 7f);
			t.givePath(TownCentre.Instance.RandomPositionInTownCentre());
		}
		else
		{
			if (!(ConversationCooledDown <= 0f))
			{
				return;
			}
			if ((VomitCooldown -= Time.deltaTime) < 0f && CheckVomit())
			{
				VomitCooldown = 10f;
				float f = Utils.GetAngle(ClosestVomit.transform.position, t.transform.position) * ((float)Math.PI / 180f);
				float num = UnityEngine.Random.Range(0.5f, 2f);
				MoveToObject.transform.position = ClosestVomit.transform.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
				w.GoToAndStop(MoveToObject, ReactSicken, ClosestVomit.gameObject, false);
				return;
			}
			if (!((DeadWorshipperCooldown -= Time.deltaTime) < 0f) || !CheckDeadWorshipper())
			{
				foreach (Task_IdleWorhipper worshipper in Worshippers)
				{
					if (worshipper.t.CurrentTask != null && worshipper.t.CurrentTask.Type == Task_Type.NONE && !worshipper.t.InConversation && worshipper.ConversationCooledDown <= 0f && worshipper != this && Vector3.Distance(worshipper.t.transform.position, t.transform.position) < 3f)
					{
						cTarget = worshipper;
						cTarget.cTarget = this;
						BeginConversation();
						break;
					}
				}
				return;
			}
			DeadWorshipperCooldown = 10f;
			float f2 = Utils.GetAngle(ClosestDeadWorshipper.transform.position, t.transform.position) * ((float)Math.PI / 180f);
			float num2 = UnityEngine.Random.Range(0.5f, 2f);
			MoveToObject.transform.position = ClosestDeadWorshipper.transform.position + new Vector3(num2 * Mathf.Cos(f2), num2 * Mathf.Sin(f2));
			if (ClosestDeadWorshipper.StructureInfo.Age >= 5)
			{
				w.GoToAndStop(MoveToObject, ReactSicken, ClosestDeadWorshipper.gameObject, false);
			}
			else
			{
				w.GoToAndStop(MoveToObject, ReactGreive, ClosestDeadWorshipper.gameObject, false);
			}
		}
	}

	private bool CheckVomit()
	{
		ClosestVomit = null;
		float num = 10f;
		foreach (Vomit vomit in Vomit.Vomits)
		{
			float num2 = Vector3.Distance(t.transform.position, vomit.transform.position);
			if (num2 < num)
			{
				num = num2;
				ClosestVomit = vomit;
			}
		}
		return ClosestVomit != null;
	}

	private void ReactSicken()
	{
		w.wim.v_i.Illness += 10f;
		w.TimedAnimation("Reactions/react-sick", 2.9666667f, w.BackToIdle);
	}

	private bool CheckDeadWorshipper()
	{
		ClosestDeadWorshipper = null;
		float num = 10f;
		foreach (DeadWorshipper deadWorshipper in DeadWorshipper.DeadWorshippers)
		{
			float num2 = Vector3.Distance(t.transform.position, deadWorshipper.transform.position);
			if (num2 < num)
			{
				num = num2;
				ClosestDeadWorshipper = deadWorshipper;
			}
		}
		return ClosestDeadWorshipper != null;
	}

	private void ReactGreive()
	{
		w.wim.v_i.Illness += 5f;
		switch (w.wim.GetRelationship(ClosestDeadWorshipper.StructureInfo.FollowerID).CurrentRelationshipState)
		{
		case IDAndRelationship.RelationshipState.Enemies:
			w.TimedAnimation("Reactions/react-laugh", 3.3333333f, w.BackToIdle);
			w.wim.v_i.Faith += 10f;
			break;
		case IDAndRelationship.RelationshipState.Friends:
		case IDAndRelationship.RelationshipState.Lovers:
			w.TimedAnimation("Reactions/react-cry", 9f, w.BackToIdle);
			w.wim.v_i.Faith -= 15f;
			break;
		default:
			w.wim.v_i.Faith -= 5f;
			w.TimedAnimation("Reactions/react-sad", 2.9666667f, w.BackToIdle);
			break;
		}
	}

	private void BeginConversation()
	{
		t.ClearPaths();
		t.InConversation = true;
		cTarget.t.InConversation = true;
		cTarget.t.ClearPaths();
		ConversationCooledDown = 5f;
		cTarget.ConversationCooledDown = 5f;
		cTargetPosition = t.transform.position + Vector3.right * ((!(cTarget.t.transform.position.x < t.transform.position.x)) ? 1 : (-1));
		MoveToGameObject = new GameObject();
		MoveToGameObject.transform.position = cTargetPosition;
		cRelationship = cTarget.w.wim.GetRelationship(w.wim.v_i.ID);
		cTarget.w.GoToAndStop(MoveToGameObject, Conversation, t.gameObject, false);
		cTarget.w.SetAnimation("Conversations/walkpast-" + GetRelationshipAnimation(cRelationship.Relationship), true);
		Relationship = w.wim.GetRelationship(cTarget.w.wim.v_i.ID);
		w.SetAnimation("Conversations/greet-" + GetRelationshipAnimation(Relationship.Relationship), false);
		w.AddAnimation("Conversations/idle-" + GetRelationshipAnimationGoodConversation(Relationship.Relationship), true);
		state.facingAngle = Utils.GetAngle(t.transform.position, cTarget.t.transform.position);
	}

	private void Conversation()
	{
		if (t != null)
		{
			t.StartCoroutine(DoConversation());
		}
	}

	private IEnumerator DoConversation()
	{
		Node = AstarPath.active.GetNearest(t.transform.position).node;
		cNode = AstarPath.active.GetNearest(cTarget.t.transform.position).node;
		Node.Walkable = true;
		cNode.Walkable = true;
		state.facingAngle = Utils.GetAngle(t.transform.position, cTarget.t.transform.position);
		cTarget.state.facingAngle = Utils.GetAngle(cTarget.t.transform.position, t.transform.position);
		cTarget.t.InConversation = true;
		cTarget.t.StopAllCoroutines();
		cTarget.t.ClearPaths();
		cTarget.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Score = 0;
		yield return new WaitForEndOfFrame();
		bool talkNice = UnityEngine.Random.Range(0, 100) < 50 || (w.GuaranteedGoodInteractions && cTarget.w.GuaranteedGoodInteractions);
		Talk(talkNice, w, cTarget.w, Relationship.Relationship, cRelationship.Relationship);
		yield return new WaitForSeconds(w.simpleAnimator.Duration());
		talkNice = UnityEngine.Random.Range(0, 100) < 50 || (w.GuaranteedGoodInteractions && cTarget.w.GuaranteedGoodInteractions);
		Talk(talkNice, cTarget.w, w, cRelationship.Relationship, Relationship.Relationship);
		yield return new WaitForSeconds(cTarget.w.simpleAnimator.Duration());
		talkNice = UnityEngine.Random.Range(0, 100) < 50 || (w.GuaranteedGoodInteractions && cTarget.w.GuaranteedGoodInteractions);
		Talk(talkNice, w, cTarget.w, Relationship.Relationship, cRelationship.Relationship);
		yield return new WaitForSeconds(w.simpleAnimator.Duration());
		bool Fight = false;
		float seconds;
		if (Score >= 2)
		{
			cRelationship.Relationship++;
			Relationship.Relationship++;
			if (Relationship.CurrentRelationshipState < IDAndRelationship.RelationshipState.Friends && (float)Relationship.Relationship >= FriendThreshold)
			{
				cRelationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Friends;
				Relationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Friends;
				w.SetAnimation("Conversations/become-friends", false);
				cTarget.w.SetAnimation("Conversations/become-friends", false);
				w.bubble.Play(WorshipperBubble.SPEECH_TYPE.FRIENDS);
				cTarget.w.bubble.Play(WorshipperBubble.SPEECH_TYPE.FRIENDS);
				seconds = 5.5f;
			}
			else if (Relationship.CurrentRelationshipState < IDAndRelationship.RelationshipState.Lovers && (float)Relationship.Relationship >= LoveThreshold)
			{
				cRelationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Lovers;
				Relationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Lovers;
				w.SetAnimation("Conversations/become-lovers", false);
				cTarget.w.SetAnimation("Conversations/become-lovers", false);
				w.bubble.Play(WorshipperBubble.SPEECH_TYPE.LOVE);
				cTarget.w.bubble.Play(WorshipperBubble.SPEECH_TYPE.LOVE);
				seconds = 5.5f;
			}
			else if (Relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Lovers)
			{
				w.SetAnimation("loving", true);
				cTarget.w.SetAnimation("loving", true);
				seconds = 5.3333335f;
			}
			else
			{
				w.SetAnimation("Conversations/react-" + GetRelationshipAnimationGoodConversation(Relationship.Relationship) + UnityEngine.Random.Range(1, 4), false);
				cTarget.w.SetAnimation("Conversations/react-" + GetRelationshipAnimationGoodConversation(cRelationship.Relationship) + UnityEngine.Random.Range(1, 4), false);
				seconds = 2f;
			}
			w.AddAnimation("idle", true);
			cTarget.w.AddAnimation("idle", true);
		}
		else
		{
			cRelationship.Relationship--;
			Relationship.Relationship--;
			if (Relationship.CurrentRelationshipState > IDAndRelationship.RelationshipState.Enemies && (float)Relationship.Relationship <= HateThreshold)
			{
				cRelationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Enemies;
				Relationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Enemies;
				w.SetAnimation("Conversations/become-enemies", false);
				cTarget.w.SetAnimation("Conversations/become-enemies", false);
				w.bubble.Play(WorshipperBubble.SPEECH_TYPE.ENEMIES);
				cTarget.w.bubble.Play(WorshipperBubble.SPEECH_TYPE.ENEMIES);
				seconds = 5.5f;
			}
			else if (Relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Enemies && UnityEngine.Random.Range(0, 100) < 33)
			{
				w.SetAnimation("fight", true);
				cTarget.w.SetAnimation("fight", true);
				seconds = 5f;
				Fight = true;
			}
			else
			{
				w.SetAnimation("Conversations/react-" + GetRelationshipAnimationBadConversation(Relationship.Relationship) + UnityEngine.Random.Range(1, 4), false);
				cTarget.w.SetAnimation("Conversations/react-" + GetRelationshipAnimationBadConversation(cRelationship.Relationship) + UnityEngine.Random.Range(1, 4), false);
				seconds = 2f;
			}
			w.AddAnimation("Conversations/idle-" + GetRelationshipAnimation(Relationship.Relationship), true);
			cTarget.w.AddAnimation("Conversations/idle-" + GetRelationshipAnimation(cRelationship.Relationship), true);
		}
		yield return new WaitForSeconds(seconds);
		Node.Walkable = true;
		cNode.Walkable = true;
		t.InConversation = false;
		Timer = 0f;
		cTarget.t.InConversation = false;
		cTarget.Timer = 0f;
		cTarget.state.CURRENT_STATE = StateMachine.State.Idle;
		state.CURRENT_STATE = StateMachine.State.Idle;
		UnityEngine.Object.Destroy(MoveToGameObject);
		MoveToGameObject = null;
		if (Fight)
		{
			w.Die();
		}
	}

	private void Talk(bool TalkNice, Worshipper Speaker, Worshipper Reactor, int Relationship, int ReactorRelationship)
	{
		Speaker.SetAnimation(TalkNice ? ("Conversations/talk-" + GetRelationshipAnimationGoodConversation(Relationship) + UnityEngine.Random.Range(1, 4)) : ("Conversations/talk-" + GetRelationshipAnimationBadConversation(Relationship) + UnityEngine.Random.Range(1, 4)), true);
		Reactor.SetAnimation("Conversations/idle-" + GetRelationshipAnimation(ReactorRelationship), true);
		Score += (TalkNice ? 1 : 0);
	}

	private string GetRelationshipAnimation(int Relationship)
	{
		if ((float)Relationship < HateThreshold)
		{
			return "hate";
		}
		if (Utils.WithinRange(cRelationship.Relationship, HateThreshold, -1f))
		{
			return "mean";
		}
		if (Utils.WithinRange(cRelationship.Relationship, 0f, LoveThreshold))
		{
			return "nice";
		}
		if ((float)Relationship > LoveThreshold)
		{
			return "love";
		}
		return "nice";
	}

	private string GetRelationshipAnimationGoodConversation(int Relationship)
	{
		if ((float)Relationship > LoveThreshold)
		{
			return "love";
		}
		return "nice";
	}

	private string GetRelationshipAnimationBadConversation(int Relationship)
	{
		if ((float)Relationship < HateThreshold)
		{
			return "hate";
		}
		return "mean";
	}
}
