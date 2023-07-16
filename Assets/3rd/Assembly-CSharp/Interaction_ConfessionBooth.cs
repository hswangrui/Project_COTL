using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using src.Extensions;
using Unify;
using UnityEngine;

public class Interaction_ConfessionBooth : Interaction
{
	private bool Activating;

	private Follower sacrificeFollower;

	public GameObject Position1;

	public GameObject Position2;

	public GameObject Position3;

	public GameObject CameraBone;

	public Structure Structure;

	public SpriteRenderer SpeechBubble;

	public List<Sprite> SpeechSprites = new List<Sprite>();

	private string SacrificeFollower;

	private string NotEnoughFollowers;

	private string NoFollowers;

	private string AlreadyHeardConfession;

	private FollowerTask_ManualControl Task;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		SacrificeFollower = ScriptLocalization.Interactions.TakeConfession;
		NotEnoughFollowers = ScriptLocalization.Interactions.RequiresMoreFollowers;
		NoFollowers = ScriptLocalization.Interactions.NoFollowers;
		AlreadyHeardConfession = ScriptLocalization.Interactions.AlreadyTakenConfession;
	}

	public override void GetLabel()
	{
		int value;
		bool flag = DataManager.Instance.DayPreviosulyUsedStructures.TryGetValue(Structure.Type, out value);
		if (!flag || (flag && TimeManager.CurrentDay > value))
		{
			DataManager.Instance.DayPreviosulyUsedStructures.Remove(Structure.Type);
			if (FollowerManager.FollowersAtLocation(PlayerFarming.Location).Count <= 0)
			{
				base.Label = ((DataManager.Instance.Followers.Count <= 0) ? NoFollowers : NotEnoughFollowers);
				Interactable = false;
			}
			else
			{
				Interactable = true;
				base.Label = (Activating ? "" : SacrificeFollower);
			}
		}
		else
		{
			Interactable = false;
			base.Label = AlreadyHeardConfession;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (Activating)
		{
			return;
		}
		base.OnInteract(state);
		Activating = true;
		Interactable = false;
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 8f);
		PlayerFarming.Instance.GoToAndStop(Position1.transform.position, base.gameObject, false, false, delegate
		{
			if (FollowerManager.FollowersAtLocation(PlayerFarming.Location).Count <= 0 || DataManager.Instance.Followers.Count <= 0)
			{
				GameManager.GetInstance().OnConversationEnd();
				Activating = false;
			}
			else
			{
				PlayerFarming.Instance.Spine.UseDeltaTime = false;
				StartCoroutine(PositionCharacters(state.gameObject, Position1.transform.position));
				state.CURRENT_STATE = StateMachine.State.InActive;
				Time.timeScale = 0f;
				List<Follower> list = new List<Follower>();
				foreach (Follower follower in Follower.Followers)
				{
					if (!FollowerManager.FollowerLocked(follower.Brain.Info.ID))
					{
						list.Add(follower);
					}
				}
				UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
				//followerSelectInstance.VotingType = TwitchVoting.VotingType.CONFESSION;
				list.Sort((Follower a, Follower b) => b.Brain.Info.XPLevel.CompareTo(a.Brain.Info.XPLevel));
				followerSelectInstance.Show(list);
				UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
				uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
				{
					Time.timeScale = 1f;
					PlayerFarming.Instance.Spine.UseDeltaTime = true;
					sacrificeFollower = FollowerManager.FindFollowerByID(followerInfo.ID);
					if (TimeManager.IsNight && sacrificeFollower.Brain.CurrentTask != null && sacrificeFollower.Brain.CurrentTask.State == FollowerTaskState.Doing && (sacrificeFollower.Brain.CurrentTaskType == FollowerTaskType.Sleep || sacrificeFollower.Brain.CurrentTaskType == FollowerTaskType.SleepBedRest))
					{
						CultFaithManager.AddThought(Thought.Cult_WokeUpFollower, sacrificeFollower.Brain.Info.ID, 1f);
					}
					FollowerTask currentTask = sacrificeFollower.Brain.CurrentTask;
					if (currentTask != null)
					{
						currentTask.Abort();
					}
					Task = new FollowerTask_ManualControl();
					sacrificeFollower.Brain.HardSwapToTask(Task);
					GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject);
					sacrificeFollower.transform.position = Position3.transform.position;
					StartCoroutine(SacrificeFollowerRoutine());
					SimulationManager.Pause();
				});
				UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
				uIFollowerSelectMenuController2.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnCancel, (Action)delegate
				{
					GameManager.GetInstance().OnConversationEnd();
					Activating = false;
					Time.timeScale = 1f;
					Activating = false;
					PlayerFarming.Instance.Spine.UseDeltaTime = true;
					PlayerFarming.Instance.GoToAndStop(Position3, null, true);
				});
				UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
				uIFollowerSelectMenuController3.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnHidden, (Action)delegate
				{
					followerSelectInstance = null;
				});
			}
		});
	}

	private IEnumerator SacrificeFollowerRoutine()
	{
		yield return new WaitForSeconds(2f);
		bool isInBooth = false;
		sacrificeFollower.FacePosition(Position1.transform.position);
		Task.GoToAndStop(sacrificeFollower, Position2.transform.position, delegate
		{
			isInBooth = true;
		});
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower_noBookPage");
		while (!isInBooth)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		SpeechBubble.gameObject.SetActive(true);
		SpeechBubble.gameObject.transform.DOScale(0f, 0f);
		SpeechBubble.gameObject.transform.DOScale(1f, 0.5f).SetEase(Ease.OutSine);
		SpeechBubble.sprite = SpeechSprites[UnityEngine.Random.Range(0, SpeechSprites.Count)];
		switch (UnityEngine.Random.Range(0, 4))
		{
		case 0:
			sacrificeFollower.Interaction_FollowerInteraction.eventListener.PlayFollowerVO("event:/dialogue/followers/general_talk");
			break;
		case 1:
			sacrificeFollower.Interaction_FollowerInteraction.eventListener.PlayFollowerVO("event:/dialogue/followers/talk_short_hate");
			break;
		case 2:
			sacrificeFollower.Interaction_FollowerInteraction.eventListener.PlayFollowerVO("event:/dialogue/followers/talk_short_love");
			break;
		case 3:
			sacrificeFollower.Interaction_FollowerInteraction.eventListener.PlayFollowerVO("event:/dialogue/followers/talk_short_nice");
			break;
		}
		yield return new WaitForSeconds(2.2f);
		SpeechBubble.sprite = SpeechSprites[UnityEngine.Random.Range(0, SpeechSprites.Count)];
		SpeechBubble.gameObject.transform.localScale = Vector3.one * 1.2f;
		SpeechBubble.gameObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
		switch (UnityEngine.Random.Range(0, 4))
		{
		case 0:
			sacrificeFollower.Interaction_FollowerInteraction.eventListener.PlayFollowerVO("event:/dialogue/followers/general_talk");
			break;
		case 1:
			sacrificeFollower.Interaction_FollowerInteraction.eventListener.PlayFollowerVO("event:/dialogue/followers/talk_short_hate");
			break;
		case 2:
			sacrificeFollower.Interaction_FollowerInteraction.eventListener.PlayFollowerVO("event:/dialogue/followers/talk_short_love");
			break;
		case 3:
			sacrificeFollower.Interaction_FollowerInteraction.eventListener.PlayFollowerVO("event:/dialogue/followers/talk_short_nice");
			break;
		}
		yield return new WaitForSeconds(2.2f);
		SpeechBubble.gameObject.transform.DOScale(0f, 0.5f).SetEase(Ease.OutQuart);
		yield return new WaitForSeconds(0.5f);
		SpeechBubble.gameObject.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower_noBookPage");
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(CameraBone, 5f);
		GameManager.GetInstance().OnConversationNext(sacrificeFollower.gameObject, 8f);
		string Animation = "";
		float Duration = 0f;
		Thought thought = Thought.None;
		Task.GoToAndStop(sacrificeFollower, Position3.transform.position, delegate
		{
			float value = UnityEngine.Random.value;
			if (value <= 0.1f)
			{
				value = UnityEngine.Random.value;
				if (value < 1f / 3f)
				{
					thought = Thought.GaveConfessionAnxious;
					Animation = "Reactions/react-feared";
					Duration = 2.8f;
				}
				else if (Utils.Between(value, 1f / 3f, 2f / 3f))
				{
					thought = Thought.GaveConfessionAnnoyed;
					Animation = "Reactions/react-sad";
					Duration = 2.9f;
				}
				else if (value >= 2f / 3f)
				{
					thought = Thought.GaveConfessionDivine;
					Animation = "Reactions/react-happy1";
					Duration = 2.1f;
				}
			}
			else
			{
				value = UnityEngine.Random.value;
				if (value < 1f / 3f)
				{
					thought = Thought.GaveConfessionHappy;
					Animation = "Reactions/react-enlightened2";
					Duration = 2.1f;
				}
				else if (Utils.Between(value, 1f / 3f, 2f / 3f))
				{
					thought = Thought.GaveConfessionEcstatic;
					Animation = "Reactions/react-happy1";
					Duration = 2.1f;
				}
				else if (value >= 2f / 3f)
				{
					thought = Thought.GaveConfessionHonoured;
					Animation = "Reactions/react-happy1";
					Duration = 2.1f;
				}
			}
			sacrificeFollower.Brain.AddThought(thought);
			sacrificeFollower.SetBodyAnimation(Animation, false);
			sacrificeFollower.AddBodyAnimation("idle", true, 0f);
			AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
			sacrificeFollower.Brain.AddAdoration(FollowerBrain.AdorationActions.ConfessionBooth, null);
		});
		yield return new WaitForSeconds(2f);
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("TAKE_CONFESSION"));
		yield return new WaitForSeconds(2f);
		StartCoroutine(DelayEnd());
	}

	private IEnumerator DelayEnd()
	{
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		sacrificeFollower.Brain.CompleteCurrentTask();
		Activating = false;
		DataManager.Instance.DayPreviosulyUsedStructures.Add(Structure.Type, TimeManager.CurrentDay);
		PlayerFarming.Instance.GoToAndStop(Position3, null, true);
		SimulationManager.UnPause();
	}

	private IEnumerator PositionCharacters(GameObject Character, Vector3 TargetPosition)
	{
		float Progress = 0f;
		float Duration = 0.3f;
		Vector3 StartingPosition = Character.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Character.transform.position = Vector3.Lerp(StartingPosition, TargetPosition, Progress / Duration);
			yield return null;
		}
		Character.transform.position = TargetPosition;
	}
}
