using System;
using System.Collections;
using UnityEngine;

public class FollowerTask_Chat : FollowerTask
{
	private enum ConvoStage
	{
		None,
		Chat1,
		Chat2,
		Chat3,
		Finale,
		Finished
	}

	private enum ConvoOutcome
	{
		BecomeFriends,
		BecomeLovers,
		RemainLovers,
		GoodConvo,
		BecomeEnemies,
		Fight,
		BadConvo
	}

	private int _otherFollowerID;

	public FollowerTask_Chat OtherChatTask;

	private bool _isLeader;

	private bool _isSpeaker;

	private ConvoStage _convoStage;

	private ConvoOutcome _convoOutcome;

	private float _doingTimer;

	private int _score;

	private bool _talkNice;

	private Coroutine _greetCoroutine;

	public static Action<FollowerInfo, FollowerInfo, IDAndRelationship.RelationshipState> OnChangeRelationship;

	public override FollowerTaskType Type
	{
		get
		{
			return FollowerTaskType.Chat;
		}
	}

	public override FollowerLocation Location
	{
		get
		{
			return _brain.Location;
		}
	}

	public override bool BlockTaskChanges
	{
		get
		{
			return true;
		}
	}

	public override bool BlockReactTasks
	{
		get
		{
			return true;
		}
	}

	public Vector3 ChatPosition { get; private set; }

	private float HateThreshold
	{
		get
		{
			return -10f;
		}
	}

	private float FriendThreshold
	{
		get
		{
			return 5f;
		}
	}

	private float LoveThreshold
	{
		get
		{
			return 10f;
		}
	}

	public FollowerTask_Chat(int followerID, bool leader)
	{
		_otherFollowerID = followerID;
		_isLeader = leader;
	}

	protected override int GetSubTaskCode()
	{
		return _otherFollowerID;
	}

	protected override void OnStart()
	{
		if (_isLeader)
		{
			OtherChatTask = new FollowerTask_Chat(_brain.Info.ID, false);
			OtherChatTask.OtherChatTask = this;
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(_otherFollowerID);
			if (followerBrain == null)
			{
				End();
				return;
			}
			followerBrain.TransitionToTask(OtherChatTask);
		}
		_brain.Stats.Social = 100f;
		LocationState locationState = LocationManager.GetLocationState(Location);
		Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
		if (locationState == LocationState.Active && follower != null)
		{
			ChatPosition = follower.transform.position;
		}
		else
		{
			ChatPosition = TownCentre.RandomPositionInCachedTownCentre();
		}
		if (OtherChatTask.State != FollowerTaskState.Done)
		{
			FollowerTask_Chat otherChatTask = OtherChatTask;
			otherChatTask.OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Combine(otherChatTask.OnFollowerTaskStateChanged, new FollowerTaskDelegate(OnTaskStateChanged));
			SetState(FollowerTaskState.GoingTo);
		}
		else
		{
			OtherChatTask.End();
			End();
		}
	}

	protected override void OnArrive()
	{
		if (_isLeader)
		{
			CommenceConvo();
			OtherChatTask.CommenceConvo();
			SetState(FollowerTaskState.Idle);
		}
		else
		{
			SetState(FollowerTaskState.Idle);
		}
	}

	protected override void OnComplete()
	{
		FollowerTask_Chat otherChatTask = OtherChatTask;
		otherChatTask.OnFollowerTaskStateChanged = (FollowerTaskDelegate)Delegate.Remove(otherChatTask.OnFollowerTaskStateChanged, new FollowerTaskDelegate(OnTaskStateChanged));
	}

	public void CommenceConvo()
	{
		_convoStage = ConvoStage.Chat1;
		if (_isLeader)
		{
			Follower follower = FollowerManager.FindFollowerByID(_brain.Info.ID);
			Follower follower2 = FollowerManager.FindFollowerByID(_otherFollowerID);
			if ((bool)follower && (bool)follower2 && follower.gameObject.GetComponent<Interaction_BackToWork>() == null)
			{
				Interaction_BackToWork interaction_BackToWork = follower.gameObject.AddComponent<Interaction_BackToWork>();
				interaction_BackToWork.Init(follower);
				interaction_BackToWork.LockPosition = follower.transform;
			}
			if ((bool)follower && (bool)follower2 && follower2.gameObject.GetComponent<Interaction_BackToWork>() == null)
			{
				Interaction_BackToWork interaction_BackToWork2 = follower2.gameObject.AddComponent<Interaction_BackToWork>();
				interaction_BackToWork2.Init(follower2);
				interaction_BackToWork2.LockPosition = follower2.transform;
			}
		}
	}

	private void NextConvoStage()
	{
		_convoStage++;
		SetState(FollowerTaskState.Idle);
	}

	private bool GetTalkNice()
	{
		int num = 70;
		if (_brain.Stats.GuaranteedGoodInteractions)
		{
			num = 100;
		}
		return UnityEngine.Random.Range(0, 100) < num;
	}

	protected override void TaskTick(float deltaGameTime)
	{
		if (base.State == FollowerTaskState.Idle)
		{
			if (_convoStage == ConvoStage.Finished)
			{
				End();
			}
			else
			{
				if (_convoStage == ConvoStage.None)
				{
					return;
				}
				switch (_convoStage)
				{
				case ConvoStage.Chat1:
				case ConvoStage.Chat3:
					_isSpeaker = _isLeader;
					if (_isSpeaker)
					{
						bool talkNice2 = GetTalkNice();
						Talk(talkNice2);
						OtherChatTask.Talk(talkNice2);
					}
					break;
				case ConvoStage.Chat2:
					_isSpeaker = !_isLeader;
					if (_isSpeaker)
					{
						bool talkNice = GetTalkNice();
						Talk(talkNice);
						OtherChatTask.Talk(talkNice);
					}
					break;
				case ConvoStage.Finale:
					_isSpeaker = _isLeader;
					if (_score >= 2)
					{
						RelationshipUp();
					}
					else
					{
						RelationshipDown();
					}
					break;
				}
				SetState(FollowerTaskState.Doing);
			}
		}
		else if (base.State == FollowerTaskState.Doing && _isSpeaker && (_doingTimer -= deltaGameTime) <= 0f)
		{
			NextConvoStage();
			OtherChatTask.NextConvoStage();
		}
	}

	public void Talk(bool talkNice)
	{
		_talkNice = talkNice;
		_score += (_talkNice ? 1 : 0);
	}

	private void RelationshipUp()
	{
		IDAndRelationship orCreateRelationship = _brain.Info.GetOrCreateRelationship(_otherFollowerID);
		orCreateRelationship.Relationship++;
		bool flag = FollowerManager.AreSiblings(_brain.Info.ID, _otherFollowerID);
		if (orCreateRelationship.CurrentRelationshipState < IDAndRelationship.RelationshipState.Friends && (float)orCreateRelationship.Relationship >= FriendThreshold)
		{
			orCreateRelationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Friends;
			_convoOutcome = ConvoOutcome.BecomeFriends;
			_brain.AddThought(Thought.NewFriend);
		}
		else if (orCreateRelationship.CurrentRelationshipState < IDAndRelationship.RelationshipState.Lovers && (float)orCreateRelationship.Relationship >= LoveThreshold && !flag)
		{
			orCreateRelationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Lovers;
			_convoOutcome = ConvoOutcome.BecomeLovers;
			_brain.AddThought(Thought.NewLover);
		}
		else if (orCreateRelationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Lovers)
		{
			_convoOutcome = ConvoOutcome.RemainLovers;
			_brain.AddThought(Thought.GoodChatWithLover);
		}
		else
		{
			_convoOutcome = ConvoOutcome.GoodConvo;
			_brain.AddThought(Thought.GoodChat);
		}
	}

	private void RelationshipDown()
	{
		IDAndRelationship orCreateRelationship = _brain.Info.GetOrCreateRelationship(_otherFollowerID);
		orCreateRelationship.Relationship--;
		if (orCreateRelationship.CurrentRelationshipState > IDAndRelationship.RelationshipState.Enemies && (float)orCreateRelationship.Relationship <= HateThreshold)
		{
			orCreateRelationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Enemies;
			_convoOutcome = ConvoOutcome.BecomeEnemies;
			_brain.AddThought(Thought.NewEnemy);
		}
		else if (orCreateRelationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Enemies && UnityEngine.Random.Range(0, 100) < 33)
		{
			_convoOutcome = ConvoOutcome.Fight;
		}
		else
		{
			_convoOutcome = ConvoOutcome.BadConvo;
			_brain.AddThought(Thought.BadChat);
		}
	}

	private void OnTaskStateChanged(FollowerTaskState oldState, FollowerTaskState newState)
	{
		if (newState == FollowerTaskState.Done)
		{
			End();
		}
	}

	private void UndoStateAnimationChanges(Follower follower)
	{
		SimpleSpineAnimator.SpineChartacterAnimationData animationData = follower.SimpleAnimator.GetAnimationData(StateMachine.State.Moving);
		animationData.Animation = animationData.DefaultAnimation;
		follower.ResetStateAnimations();
	}

	protected override Vector3 UpdateDestination(Follower follower)
	{
		if (_isLeader)
		{
			FollowerBrain followerBrain = FollowerBrain.FindBrainByID(_otherFollowerID);
			if (followerBrain != null && followerBrain.CurrentTaskType == FollowerTaskType.Chat)
			{
				FollowerTask_Chat followerTask_Chat = (FollowerTask_Chat)followerBrain.CurrentTask;
				return followerTask_Chat.ChatPosition + Vector3.right * ((followerTask_Chat.ChatPosition.x < follower.transform.position.x) ? 1.5f : (-1.5f));
			}
			if (follower != null)
			{
				return follower.transform.position;
			}
			return _brain.LastPosition;
		}
		return ChatPosition;
	}

	public override void Setup(Follower follower)
	{
		base.Setup(follower);
		if (base.State == FollowerTaskState.Doing)
		{
			Follower follower2 = FollowerManager.FindFollowerByID(_otherFollowerID);
			if ((bool)follower2 && (bool)follower && follower.State != null)
			{
				follower.State.facingAngle = Utils.GetAngle(follower.transform.position, follower2.transform.position);
			}
		}
	}

	public override void OnGoingToBegin(Follower follower)
	{
		base.OnGoingToBegin(follower);
		if (_isLeader)
		{
			Follower follower2 = FollowerManager.FindFollowerByID(_otherFollowerID);
			_greetCoroutine = follower.StartCoroutine(WaitForGreetCoroutine(follower, follower2));
			IDAndRelationship orCreateRelationship = _brain.Info.GetOrCreateRelationship(_otherFollowerID);
			IDAndRelationship orCreateRelationship2 = follower2.Brain.Info.GetOrCreateRelationship(_brain.Info.ID);
			follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Conversations/walkpast-" + GetRelationshipAnimation(orCreateRelationship.Relationship));
			follower2.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			follower2.SetBodyAnimation("Conversations/greet-" + GetRelationshipAnimation(orCreateRelationship2.Relationship), true);
			follower2.FacePosition(follower.transform.position);
		}
	}

	public override void OnDoingBegin(Follower follower)
	{
		UndoStateAnimationChanges(follower);
		Follower follower2 = FollowerManager.FindFollowerByID(_otherFollowerID);
		if (follower2 == null || follower == null)
		{
			End();
			return;
		}
		follower2.FacePosition(follower.transform.position);
		follower.FacePosition(follower2.transform.position);
		IDAndRelationship orCreateRelationship = _brain.Info.GetOrCreateRelationship(_otherFollowerID);
		switch (_convoStage)
		{
		case ConvoStage.None:
			throw new ArgumentException("Doing unstarted convo!!");
		case ConvoStage.Chat1:
		case ConvoStage.Chat2:
		case ConvoStage.Chat3:
			if (_isSpeaker)
			{
				follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
				follower.SetBodyAnimation(_talkNice ? ("Conversations/talk-" + GetRelationshipAnimationGoodConversation(orCreateRelationship.Relationship) + UnityEngine.Random.Range(1, 4)) : ("Conversations/talk-" + GetRelationshipAnimationBadConversation(orCreateRelationship.Relationship) + UnityEngine.Random.Range(1, 4)), true);
				_doingTimer = ConvertAnimTimeToGameTime(follower.SimpleAnimator.Duration());
			}
			else
			{
				follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
				follower.SetBodyAnimation("Conversations/idle-" + GetRelationshipAnimation(orCreateRelationship.Relationship), true);
			}
			break;
		case ConvoStage.Finale:
			DoFinale(follower, orCreateRelationship);
			break;
		}
	}

	public override void Cleanup(Follower follower)
	{
		if (_greetCoroutine != null)
		{
			follower.StopCoroutine(_greetCoroutine);
			_greetCoroutine = null;
		}
		if ((bool)follower.GetComponent<Interaction_BackToWork>())
		{
			UnityEngine.Object.Destroy(follower.GetComponent<Interaction_BackToWork>());
		}
		UndoStateAnimationChanges(follower);
	}

	private IEnumerator WaitForGreetCoroutine(Follower follower, Follower otherFollower)
	{
		while (base.State == FollowerTaskState.GoingTo && Vector3.Distance(follower.transform.position, otherFollower.transform.position) > 3f)
		{
			yield return null;
		}
		_greetCoroutine = null;
	}

	private float ConvertAnimTimeToGameTime(float duration)
	{
		return duration * 2f;
	}

	private void DoFinale(Follower follower, IDAndRelationship relationship)
	{
		switch (_convoOutcome)
		{
		case ConvoOutcome.BecomeFriends:
			follower.SetBodyAnimation("Conversations/become-friends", false);
			follower.WorshipperBubble.Play(WorshipperBubble.SPEECH_TYPE.FRIENDS);
			_doingTimer = ConvertAnimTimeToGameTime(5.5f);
			if (_isLeader)
			{
				Action<FollowerInfo, FollowerInfo, IDAndRelationship.RelationshipState> onChangeRelationship3 = OnChangeRelationship;
				if (onChangeRelationship3 != null)
				{
					onChangeRelationship3(FollowerInfo.GetInfoByID(_brain.Info.ID), FollowerInfo.GetInfoByID(_otherFollowerID), IDAndRelationship.RelationshipState.Friends);
				}
			}
			break;
		case ConvoOutcome.BecomeLovers:
			follower.SetBodyAnimation("Conversations/become-lovers", false);
			follower.WorshipperBubble.Play(WorshipperBubble.SPEECH_TYPE.LOVE);
			_doingTimer = ConvertAnimTimeToGameTime(5.5f);
			if (_isLeader)
			{
				Action<FollowerInfo, FollowerInfo, IDAndRelationship.RelationshipState> onChangeRelationship2 = OnChangeRelationship;
				if (onChangeRelationship2 != null)
				{
					onChangeRelationship2(FollowerInfo.GetInfoByID(_brain.Info.ID), FollowerInfo.GetInfoByID(_otherFollowerID), IDAndRelationship.RelationshipState.Lovers);
				}
			}
			break;
		case ConvoOutcome.RemainLovers:
			follower.SetBodyAnimation("loving", true);
			_doingTimer = ConvertAnimTimeToGameTime(5.3333335f);
			break;
		case ConvoOutcome.GoodConvo:
			follower.SetBodyAnimation("Conversations/react-" + GetRelationshipAnimationGoodConversation(relationship.Relationship) + UnityEngine.Random.Range(1, 4), false);
			_doingTimer = ConvertAnimTimeToGameTime(2f);
			break;
		case ConvoOutcome.BecomeEnemies:
			follower.SetBodyAnimation("Conversations/become-enemies", false);
			follower.WorshipperBubble.Play(WorshipperBubble.SPEECH_TYPE.ENEMIES);
			_doingTimer = ConvertAnimTimeToGameTime(5.5f);
			if (_isLeader)
			{
				Action<FollowerInfo, FollowerInfo, IDAndRelationship.RelationshipState> onChangeRelationship = OnChangeRelationship;
				if (onChangeRelationship != null)
				{
					onChangeRelationship(FollowerInfo.GetInfoByID(_brain.Info.ID), FollowerInfo.GetInfoByID(_otherFollowerID), IDAndRelationship.RelationshipState.Enemies);
				}
			}
			break;
		case ConvoOutcome.Fight:
			follower.SetBodyAnimation("fight", true);
			_doingTimer = ConvertAnimTimeToGameTime(5f);
			break;
		case ConvoOutcome.BadConvo:
			follower.SetBodyAnimation("Conversations/react-" + GetRelationshipAnimationBadConversation(relationship.Relationship) + UnityEngine.Random.Range(1, 4), false);
			_doingTimer = ConvertAnimTimeToGameTime(2f);
			break;
		}
		switch (_convoOutcome)
		{
		case ConvoOutcome.BecomeFriends:
		case ConvoOutcome.BecomeLovers:
		case ConvoOutcome.RemainLovers:
		case ConvoOutcome.GoodConvo:
			follower.AddBodyAnimation("idle", true, 0f);
			break;
		case ConvoOutcome.BecomeEnemies:
		case ConvoOutcome.Fight:
		case ConvoOutcome.BadConvo:
			follower.AddBodyAnimation("Conversations/idle-" + GetRelationshipAnimation(relationship.Relationship), true, 0f);
			break;
		}
	}

	private string GetRelationshipAnimation(int relationship)
	{
		if ((float)relationship < HateThreshold)
		{
			return "hate";
		}
		if (Utils.WithinRange(relationship, HateThreshold, -1f))
		{
			return "mean";
		}
		if (Utils.WithinRange(relationship, 0f, LoveThreshold))
		{
			return "nice";
		}
		if ((float)relationship > LoveThreshold)
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

	public override void SimDoingBegin(SimFollower simFollower)
	{
		_doingTimer = 5f;
	}

	protected override float SocialChange(float deltaGameTime)
	{
		return 0f;
	}

	protected override float SatiationChange(float deltaGameTime)
	{
		return 0f;
	}
}
