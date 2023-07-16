using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using I2.Loc;
using MMTools;
using Spine.Unity;
using Spine.Unity.Examples;
using UnityEngine;
using WebSocketSharp;

public class Interaction_FollowerDessentingChoice : Interaction
{
	[Serializable]
	private struct Question
	{
		public string Line1;

		public string Line2;

		public string AnswerA;

		public string AnswerB;

		public string ResultA;

		public string ResultB;

		public bool CorrectAnswerIsA;
	}

	[SerializeField]
	private SkeletonAnimation followerSpine;

	[SerializeField]
	private SkeletonAnimation portalSpine;

	[SerializeField]
	private Question[] questions;

	public FollowerInfo followerInfo;

	public ParticleSystem recruitParticles;

	private string skin;

	private Villager_Info v_i;

	private WorshipperInfoManager wim;

	public FollowerSpineEventListener eventlistener;

	private string IdleAnimation;

	private string TalkAnimation;

	private EventInstance receiveLoop;

	private void Start()
	{
		ActivateDistance = 3f;
		followerSpine = GetComponentInChildren<SkeletonAnimation>();
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
		case FollowerLocation.Dungeon1_4:
			IdleAnimation = "Dissenters/rescue2";
			TalkAnimation = "Dissenters/rescue-talk2";
			break;
		default:
			IdleAnimation = "Dissenters/rescue1";
			TalkAnimation = "Dissenters/rescue-talk1";
			break;
		}
		followerSpine.AnimationState.SetAnimation(0, IdleAnimation, true);
		skin = DataManager.GetRandomLockedSkin();
		if (skin.IsNullOrEmpty())
		{
			skin = DataManager.GetRandomSkin();
		}
		v_i = Villager_Info.NewCharacter(skin);
		wim = GetComponent<WorshipperInfoManager>();
		wim.SetV_I(v_i);
		followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base, skin);
		if (followerInfo.SkinName == "Giraffe")
		{
			followerInfo.Name = LocalizationManager.GetTranslation("FollowerNames/Sparkles");
		}
		followerInfo.Traits.Clear();
		followerInfo.Traits.Add((UnityEngine.Random.Range(0, 2) == 1) ? FollowerTrait.TraitType.Faithless : FollowerTrait.TraitType.Cynical);
		followerInfo.TraitsSet = true;
		eventlistener.SetPitchAndVibrator(followerInfo.follower_pitch, followerInfo.follower_vibrato, followerInfo.ID);
	}

	public override void GetLabel()
	{
		base.Label = (Interactable ? ScriptLocalization.Interactions.Talk : "");
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		StartCoroutine(InteractIE());
	}

	private IEnumerator InteractIE()
	{
		PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.left * 2f);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.LookAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		Question question = GetRandomQuestion();
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(base.gameObject, question.Line1));
		list.Add(new ConversationEntry(base.gameObject, question.Line2));
		list[0].CharacterName = followerInfo.Name;
		list[0].Offset = new Vector3(0f, 2f, 0f);
		list[1].CharacterName = followerInfo.Name;
		list[1].Offset = new Vector3(0f, 2f, 0f);
		list[0].soundPath = "event:/dialogue/followers/talk_short_hate";
		list[0].vibratoValue = followerInfo.follower_vibrato;
		list[0].pitchValue = followerInfo.follower_pitch;
		list[0].followerID = followerInfo.ID;
		list[1].soundPath = "event:/dialogue/followers/talk_short_hate";
		list[1].vibratoValue = followerInfo.follower_vibrato;
		list[1].pitchValue = followerInfo.follower_pitch;
		list[1].followerID = followerInfo.ID;
		list[0].Animation = TalkAnimation;
		list[1].Animation = TalkAnimation;
		List<MMTools.Response> responses = new List<MMTools.Response>
		{
			new MMTools.Response(question.AnswerA, delegate
			{
				StartCoroutine(ResponseIE(true, question));
			}, question.AnswerA),
			new MMTools.Response(question.AnswerB, delegate
			{
				StartCoroutine(ResponseIE(false, question));
			}, question.AnswerB)
		};
		MMConversation.Play(new ConversationObject(list, responses, null), false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
	}

	private IEnumerator ResponseIE(bool responseWasA, Question question)
	{
		yield return null;
		List<ConversationEntry> list = new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, responseWasA ? question.ResultA : question.ResultB)
		};
		list[0].CharacterName = followerInfo.Name;
		list[0].Offset = new Vector3(0f, 2f, 0f);
		list[0].vibratoValue = followerInfo.follower_vibrato;
		list[0].pitchValue = followerInfo.follower_pitch;
		if ((responseWasA && question.CorrectAnswerIsA) || (!responseWasA && !question.CorrectAnswerIsA))
		{
			list[0].Animation = "Conversations/talk-nice1";
		}
		else
		{
			list[0].soundPath = "event:/dialogue/followers/talk_short_hate";
			list[0].Animation = TalkAnimation;
		}
		MMConversation.Play(new ConversationObject(list, null, null), false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		Debug.Log("responseWasA: " + responseWasA);
		Debug.Log("question.CorrectAnswerIsA: " + question.CorrectAnswerIsA);
		if ((responseWasA && question.CorrectAnswerIsA) || (!responseWasA && !question.CorrectAnswerIsA))
		{
			Debug.Log("AAA");
			StartCoroutine(RecruitedFollower());
		}
		else
		{
			Debug.Log("BBB");
			GameManager.GetInstance().OnConversationEnd();
			followerSpine.AnimationState.SetAnimation(0, IdleAnimation, true);
		}
	}

	private Question GetRandomQuestion()
	{
		int num = ((DataManager.Instance.DessentingFollowerChoiceQuestionIndex < questions.Length - 1) ? (++DataManager.Instance.DessentingFollowerChoiceQuestionIndex) : UnityEngine.Random.Range(0, questions.Length));
		return questions[num];
	}

	private IEnumerator RecruitedFollower()
	{
		Interactable = false;
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		AudioManager.Instance.PlayOneShot("event:/followers/rescue");
		recruitParticles.Play();
		followerSpine.AnimationState.SetAnimation(0, "convert-short", false);
		portalSpine.gameObject.SetActive(true);
		portalSpine.AnimationState.SetAnimation(0, "convert-short", false);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_start", PlayerFarming.Instance.gameObject);
		receiveLoop = AudioManager.Instance.CreateLoop("event:/player/receive_animation_loop", PlayerFarming.Instance.gameObject, true);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		float duration = PlayerFarming.Instance.simpleSpineAnimator.Animate("specials/special-activate-long", 0, true).Animation.Duration;
		CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
		yield return new WaitForSeconds(duration - 1f);
		FollowerManager.CreateNewRecruit(followerInfo, NotificationCentre.NotificationType.NewRecruit);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_end", PlayerFarming.Instance.gameObject);
		receiveLoop.stop(STOP_MODE.ALLOWFADEOUT);
		float value = UnityEngine.Random.value;
		Thought thought = Thought.None;
		if (value < 0.7f)
		{
			value = UnityEngine.Random.value;
			if (value <= 0.3f)
			{
				thought = Thought.HappyConvert;
			}
			else if (value > 0.3f && value < 0.6f)
			{
				thought = Thought.GratefulConvert;
			}
			else if (value >= 0.6f)
			{
				thought = Thought.SkepticalConvert;
			}
		}
		else
		{
			value = UnityEngine.Random.value;
			thought = ((!(value <= 0.3f) || DataManager.Instance.Followers.Count <= 0) ? Thought.InstantBelieverConvert : Thought.ResentfulConvert);
		}
		ThoughtData data = FollowerThoughts.GetData(thought);
		data.Init();
		followerInfo.Thoughts.Add(data);
		RoomLockController.RoomCompleted();
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(1f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
