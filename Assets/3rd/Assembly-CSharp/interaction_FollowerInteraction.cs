using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerInteractionWheel;
using MMTools;
using Spine.Unity.Examples;
using src.Extensions;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class interaction_FollowerInteraction : Interaction
{
	private bool Activated;

	public Follower follower;

	private FollowerTask_ManualControl Task;

	private SimpleSpineAnimator CacheSpineAnimator;

	private string CacheAnimation;

	private float CacheAnimationProgress;

	private float CacheFacing;

	private bool CacheLoop;

	public FollowerAdorationUI AdorationOrb;

	public UIFollowerPrayingProgress FollowerPrayingProgress;

	private bool ShowDivineInspirationTutorialOnClose;

	private LayerMask collisionMask;

	private FollowerRecruit followerRecruit;

	private string generalAcknowledgeVO = "event:/dialogue/followers/general_acknowledge";

	private string negativeAcknowledgeVO = "event:/dialogue/followers/negative_acknowledge";

	private string positiveAcknowledgeVO = "event:/dialogue/followers/positive_acknowledge";

	public string bowVO = "event:/followers/Speech/FollowerBow";

	public FollowerSpineEventListener eventListener;

	private Thought transitioningCursedState;

	private string sSpeakTo;

	private string sColllectReward;

	private string sCompleteQuest;

	private FollowerTaskType previousTaskType;

	public Action OnGivenRewards;

	private bool GiveDoctrinePieceOnClose;

	private RendererMaterialSwap rendMatSwap;

	private Material prevMat;

	public Material followerUIMat;

	private string generalTalkVO
	{
		get
		{
			if (follower.Brain.Info.ID == FollowerManager.DeathCatID)
			{
				return "event:/dialogue/followers/boss/fol_deathcat";
			}
			if (follower.Brain.Info.ID == FollowerManager.AymID)
			{
				return "event:/dialogue/followers/boss/fol_guardian_a";
			}
			if (follower.Brain.Info.ID == FollowerManager.BaalID)
			{
				return "event:/dialogue/followers/boss/fol_guardian_b";
			}
			if (follower.Brain.Info.ID == FollowerManager.KallamarID)
			{
				return "event:/dialogue/followers/boss/fol_kallamar";
			}
			if (follower.Brain.Info.ID == FollowerManager.LeshyID)
			{
				return "event:/dialogue/followers/boss/fol_leshy";
			}
			if (follower.Brain.Info.ID == FollowerManager.ShamuraID)
			{
				return "event:/dialogue/followers/boss/fol_shamura";
			}
			if (follower.Brain.Info.ID == FollowerManager.HeketID)
			{
				return "event:/dialogue/followers/boss/fol_heket";
			}
			return "event:/dialogue/followers/general_talk";
		}
	}

	private bool PlayVO
	{
		get
		{
			return true;
		}
	}

	public override void IndicateHighlighted()
	{
		base.IndicateHighlighted();
		if (!follower.Brain.ThoughtExists(Thought.Dissenter) && !follower.WorshipperBubble.Active)
		{
			AdorationOrb.Show();
		}
		if (follower.Brain.CurrentTask is FollowerTask_Pray || follower.Brain.CurrentTask is FollowerTask_PrayPassive)
		{
			FollowerPrayingProgress.Hide();
		}
	}

	public override void EndIndicateHighlighted()
	{
		base.EndIndicateHighlighted();
		AdorationOrb.Hide();
		UIFollowerName componentInChildren = GetComponentInChildren<UIFollowerName>();
		if ((object)componentInChildren != null)
		{
			componentInChildren.Show();
		}
		if (follower.Brain.CurrentTask is FollowerTask_Pray || follower.Brain.CurrentTask is FollowerTask_PrayPassive)
		{
			FollowerPrayingProgress.Show();
		}
	}

	public override void GetLabel()
	{
		if (follower == null)
		{
			base.Label = "";
			return;
		}
		if (PlayerFarming.Location != FollowerLocation.Base || !Interactable)
		{
			base.Label = "";
			return;
		}
		if (follower.Brain != null && DataManager.Instance.CompletedQuestFollowerIDs.Contains(follower.Brain.Info.ID))
		{
			base.Label = sCompleteQuest;
			return;
		}
		if (follower.Brain != null && follower.Brain.Stats.Adoration >= follower.Brain.Stats.MAX_ADORATION)
		{
			base.Label = sColllectReward;
			return;
		}
		base.Label = (Interactable ? (sSpeakTo + " <color=yellow>" + follower.Brain.Info.Name + "</color>" + ((follower.Brain.Info.XPLevel > 1) ? (" " + ScriptLocalization.Interactions.Level + " " + follower.Brain.Info.XPLevel.ToNumeral()) : "") + (follower.Brain.Info.MarriedToLeader ? " <sprite name=\"icon_Married\">" : "")) : "");
	}

	private void Awake()
	{
		followerRecruit = GetComponent<FollowerRecruit>();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
	}

	private void Start()
	{
		ActivateDistance = 2f;
		UpdateLocalisation();
		HasSecondaryInteraction = false;
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Island"));
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Obstacles"));
		WorshipperBubble worshipperBubble = follower.WorshipperBubble;
		worshipperBubble.OnBubblePlay = (Action)Delegate.Combine(worshipperBubble.OnBubblePlay, (Action)delegate
		{
			AdorationOrb.Hide();
		});
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sSpeakTo = ScriptLocalization.Interactions.SpeakTo;
		sColllectReward = ScriptLocalization.Interactions.CollectDiscipleReward;
		sCompleteQuest = ScriptLocalization.Interactions.CompleteQuest;
	}

	protected override void Update()
	{
		base.Update();
		if (!(follower != null) || follower.Brain == null)
		{
			return;
		}
		if (follower.Brain.CurrentTaskType == FollowerTaskType.GetPlayerAttention && follower.Brain.CurrentTask != null)
		{
			if ((follower.Brain.CurrentTask as FollowerTask_GetAttention).ComplaintType == Follower.ComplaintType.GiveOnboarding && (follower.Brain.CurrentTask as FollowerTask_GetAttention).AutoInteract)
			{
				AutomaticallyInteract = true;
			}
			else
			{
				AutomaticallyInteract = false;
			}
			PriorityWeight = 2f;
			ActivateDistance = 2f;
		}
		else if ((follower.Brain.CurrentTaskType == FollowerTaskType.Sleep || follower.Brain.CurrentTaskType == FollowerTaskType.SleepBedRest) && follower.Brain.CurrentTask != null && follower.Brain.CurrentTask.State == FollowerTaskState.Doing && follower.Brain.HasHome)
		{
			AutomaticallyInteract = false;
			PriorityWeight = 2f;
			ActivateDistance = 1.5f;
		}
		else
		{
			AutomaticallyInteract = false;
			PriorityWeight = 1f;
			ActivateDistance = 2f;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (Activated)
		{
			return;
		}
		base.OnInteract(state);
		Follower.ComplaintType complaintType = Follower.ComplaintType.None;
		FollowerTaskType taskType = follower.Brain.CurrentTaskType;
		AutomaticallyInteract = false;
		follower.WorshipperBubble.gameObject.SetActive(false);
		follower.CompletedQuestIcon.gameObject.SetActive(false);
		previousTaskType = ((follower.Brain.CurrentTask != null) ? follower.Brain.CurrentTask.Type : FollowerTaskType.None);
		UIFollowerName componentInChildren = GetComponentInChildren<UIFollowerName>();
		if ((object)componentInChildren != null)
		{
			componentInChildren.Hide(false);
		}
		GameManager.GetInstance().CamFollowTarget.SnappyMovement = true;
		BiomeConstants.Instance.DepthOfFieldTween(1.5f, 4.5f, 10f, 1f, 145f);
		List<CommandItem> commandItems = FollowerCommandGroups.DefaultCommands(follower);
		CacheAndSetFollower();
		HideOtherFollowers();
		List<ObjectivesData> quests = Quests.GetUnCompletedFollowerQuests(follower.Brain.Info.ID, "");
		Objectives_TalkToFollower talkToFollowerQuest = null;
		foreach (ObjectivesData item in quests)
		{
			if (item is Objectives_TalkToFollower && string.IsNullOrEmpty(((Objectives_TalkToFollower)item).ResponseTerm))
			{
				talkToFollowerQuest = item as Objectives_TalkToFollower;
				complaintType = Follower.ComplaintType.CompletedQuest;
				break;
			}
		}
		foreach (ObjectivesData objective2 in DataManager.Instance.Objectives)
		{
			if (objective2 is Objectives_TalkToFollower)
			{
				Objectives_TalkToFollower objectives_TalkToFollower = objective2 as Objectives_TalkToFollower;
				if (objectives_TalkToFollower.TargetFollower == follower.Brain.Info.ID)
				{
					CloseAndSpeak(objectives_TalkToFollower.ResponseTerm);
					objectives_TalkToFollower.Done = true;
					objectives_TalkToFollower.Complete();
					ObjectiveManager.CheckObjectives();
					return;
				}
			}
		}
		if (follower.Brain.Info.CursedState == Thought.OldAge && !DataManager.Instance.OldFollowerSpoken)
		{
			DataManager.Instance.OldFollowerSpoken = true;
			eventListener.PlayFollowerVO(negativeAcknowledgeVO);
			CloseAndSpeak("TooOldToWork", delegate
			{
				OnInteract(state);
			}, false);
			GameManager.GetInstance().CamFollowTarget.SnappyMovement = true;
			return;
		}
		complaintType = ((talkToFollowerQuest != null || follower.Brain.CurrentTask == null || follower.Brain.CurrentTaskType != FollowerTaskType.GetPlayerAttention) ? complaintType : (follower.Brain.CurrentTask as FollowerTask_GetAttention).ComplaintType);
		int num = ((follower.transform.position.x < PlayerFarming.Instance.transform.position.x) ? 1 : (-1));
		if (Physics2D.Raycast(base.transform.position, new Vector3(num, 0f, 0f), 2f, collisionMask).collider != null)
		{
			num *= -1;
		}
		PlayerFarming.Instance.GoToAndStop(follower.transform.position + new Vector3(2f * (float)num, 0f), follower.gameObject, false, true, delegate
		{
			SimulationManager.Pause();
			if (follower.Brain.Stats.Adoration >= follower.Brain.Stats.MAX_ADORATION)
			{
				StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType, delegate
				{
					if (complaintType == Follower.ComplaintType.CompletedQuest && talkToFollowerQuest != null)
					{
						follower.ShowCompletedQuestIcon(true);
					}
				}));
			}
			else if ((taskType == FollowerTaskType.GetPlayerAttention && complaintType != Follower.ComplaintType.ShowTwitchMessage) || talkToFollowerQuest != null)
			{
				if (complaintType == Follower.ComplaintType.GiveOnboarding)
				{
					quests = Onboarding.Instance.GetOnboardingQuests(follower.Brain.Info.ID);
					if (quests.Count == 0)
					{
						DataManager.Instance.CurrentOnboardingFollowerID = -1;
						DataManager.Instance.CurrentOnboardingFollowerTerm = "";
						if (ObjectiveManager.GetNumberOfObjectivesInGroup("Objectives/GroupTitles/Quest") < 1)
						{
							complaintType = Follower.ComplaintType.GiveQuest;
						}
					}
				}
				ObjectivesData objective = null;
				if (complaintType == Follower.ComplaintType.GiveQuest)
				{
					List<int> list = new List<int>();
					int num2 = -1;
					int num3 = -1;
					int num4 = -1;
					foreach (Follower item2 in FollowerManager.FollowersAtLocation(FollowerLocation.Base))
					{
						if (item2.Brain.Info.ID != follower.Brain.Info.ID && !FollowerManager.FollowerLocked(item2.Brain.Info.ID) && item2.Brain.Info.CursedState == Thought.None && !FollowerManager.UniqueFollowerIDs.Contains(item2.Brain.Info.ID))
						{
							list.Add(item2.Brain.Info.ID);
						}
					}
					list.Shuffle();
					foreach (int item3 in list)
					{
						if (num2 == -1)
						{
							num2 = item3;
						}
						else if (num3 == -1)
						{
							num3 = item3;
						}
					}
					list.Clear();
					foreach (FollowerInfo item4 in DataManager.Instance.Followers_Dead)
					{
						if (!item4.HadFuneral)
						{
							list.Add(item4.ID);
						}
					}
					list.Shuffle();
					foreach (int item5 in list)
					{
						if (num4 == -1)
						{
							num4 = item5;
							break;
						}
					}
					if (FollowerManager.UniqueFollowerIDs.Contains(follower.Brain.Info.ID))
					{
						StoryObjectiveData[] allStoryObjectiveDatas = Quests.AllStoryObjectiveDatas;
						foreach (StoryObjectiveData storyObjectiveData in allStoryObjectiveDatas)
						{
							if (storyObjectiveData.QuestGiverRequiresID == follower.Brain.Info.ID)
							{
								if (storyObjectiveData.RequireTarget_1 && storyObjectiveData.Target1FollowerID != -1 && !FollowerManager.FollowerLocked(storyObjectiveData.Target1FollowerID) && FollowerInfo.GetInfoByID(storyObjectiveData.Target1FollowerID) != null)
								{
									num2 = storyObjectiveData.Target1FollowerID;
								}
								if (storyObjectiveData.RequireTarget_2 && storyObjectiveData.Target2FollowerID != -1 && !FollowerManager.FollowerLocked(storyObjectiveData.Target2FollowerID) && FollowerInfo.GetInfoByID(storyObjectiveData.Target2FollowerID) != null)
								{
									num3 = storyObjectiveData.Target2FollowerID;
								}
								if (storyObjectiveData.RequireTarget_Deadbody && storyObjectiveData.DeadBodyFollowerID != -1 && !FollowerManager.FollowerLocked(storyObjectiveData.DeadBodyFollowerID) && FollowerInfo.GetInfoByID(storyObjectiveData.DeadBodyFollowerID) != null)
								{
									num4 = storyObjectiveData.DeadBodyFollowerID;
								}
								break;
							}
						}
					}
					objective = Quests.GetRandomBaseQuest(follower.Brain.Info.ID, num2, num3, num4);
				}
				else if (complaintType == Follower.ComplaintType.CompletedQuest && talkToFollowerQuest != null)
				{
					objective = talkToFollowerQuest;
				}
				bool WillLevelUp = follower.Brain.GetWillLevelUp(FollowerBrain.AdorationActions.Quest) && complaintType == Follower.ComplaintType.CompletedQuest;
				Coroutine routine = null;
				if (complaintType == Follower.ComplaintType.CompletedQuest)
				{
					foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
					{
						if (completedObjective.Type == Objectives.TYPES.COLLECT_ITEM && completedObjective.GroupId == "Objectives/GroupTitles/Quest")
						{
							routine = StartCoroutine(GiveItemsRoutine(((Objectives_CollectItem)completedObjective).ItemType, ((Objectives_CollectItem)completedObjective).Target));
							break;
						}
					}
				}
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(follower.gameObject, 4f);
				StartCoroutine(WaitForRoutine(routine, delegate
				{
					follower.AdorationUI.Hide();
					UIFollowerName componentInChildren2 = follower.GetComponentInChildren<UIFollowerName>();
					if ((object)componentInChildren2 != null)
					{
						componentInChildren2.Hide();
					}
					MMConversation.OnConversationEnd += OnConversationEnd;
					List<ConversationEntry> conversationEntry = GetConversationEntry(complaintType, objective);
					foreach (ConversationEntry item6 in conversationEntry)
					{
						item6.soundPath = (PlayVO ? item6.soundPath : "");
					}
					MMConversation.Play(new ConversationObject(conversationEntry, null, delegate
					{
						string animation = "Reactions/react-bow";
						eventListener.PlayFollowerVO(bowVO);
						float timer = 1.8666667f;
						eventListener.PlayFollowerVO(generalAcknowledgeVO);
						if (complaintType == Follower.ComplaintType.CompletedQuest)
						{
							CultFaithManager.AddThought(Thought.Cult_CompleteQuest, -1, 1f);
							talkToFollowerQuest.Done = true;
							ObjectiveManager.UpdateObjective(talkToFollowerQuest);
							animation = "Reactions/react-happy1";
							eventListener.PlayFollowerVO(generalAcknowledgeVO);
							follower.Brain.AddThought(Thought.LeaderDidQuest);
							AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
							TimeManager.TimeSinceLastQuest = 0f;
							StartCoroutine(FrameDelayCallback(delegate
							{
								GameManager.GetInstance().OnConversationNew();
								GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
								follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Quest, delegate
								{
									if (follower.Brain.Stats.Adoration >= follower.Brain.Stats.MAX_ADORATION)
									{
										StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType, null, false));
									}
									else
									{
										Close(true, true, false);
									}
								});
								DataManager.Instance.CompletedQuestFollowerIDs.Remove(follower.Brain.Info.ID);
								follower.ShowCompletedQuestIcon(false);
								if (previousTaskType == FollowerTaskType.Sleep)
								{
									follower.Brain._directInfoAccess.WakeUpDay = -1;
								}
							}));
						}
						else
						{
							if (complaintType == Follower.ComplaintType.FailedQuest)
							{
								CultFaithManager.AddThought(Thought.Cult_FailQuest, -1, 1f);
								ObjectiveManager.ObjectiveRemoved(follower.Brain._directInfoAccess.CurrentPlayerQuest);
								NotificationCentreScreen.Play(NotificationCentre.NotificationType.QuestFailed);
								animation = "Reactions/react-sad";
								eventListener.PlayFollowerVO(negativeAcknowledgeVO);
								follower.Brain.AddThought(Thought.LeaderFailedQuest);
							}
							else if (complaintType == Follower.ComplaintType.GiveOnboarding)
							{
								if (DataManager.Instance.CurrentOnboardingFollowerID == -1)
								{
									quests.Clear();
								}
								TimeManager.TimeSinceLastQuest = 0f;
								animation = "Reactions/react-happy1";
								switch (DataManager.Instance.CurrentOnboardingFollowerTerm)
								{
								case "Conversation_NPC/FollowerOnboarding/CleanUpBase":
								{
									IllnessBar instance = IllnessBar.Instance;
									if ((object)instance != null)
									{
										instance.Reveal();
									}
									if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Illness))
									{
										MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Illness);
									}
									break;
								}
								case "Conversation_NPC/FollowerOnboarding/NameCult":
									StartCoroutine(FrameDelayCallback(delegate
									{
										GameManager.GetInstance().OnConversationNew();
										GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
										UICultNameMenuController cultNameMenuInstance = MonoSingleton<UIManager>.Instance.CultNameMenuTemplate.Instantiate();
										cultNameMenuInstance.Show(false, true);
										UICultNameMenuController uICultNameMenuController = cultNameMenuInstance;
										uICultNameMenuController.OnNameConfirmed = (Action<string>)Delegate.Combine(uICultNameMenuController.OnNameConfirmed, new Action<string>(NamedCult));
										UICultNameMenuController uICultNameMenuController2 = cultNameMenuInstance;
										uICultNameMenuController2.OnHide = (Action)Delegate.Combine(uICultNameMenuController2.OnHide, (Action)delegate
										{
										});
										UICultNameMenuController uICultNameMenuController3 = cultNameMenuInstance;
										uICultNameMenuController3.OnHidden = (Action)Delegate.Combine(uICultNameMenuController3.OnHidden, (Action)delegate
										{
											cultNameMenuInstance = null;
										});
										DataManager.Instance.CurrentOnboardingFollowerID = -1;
										DataManager.Instance.LastFollowerQuestGivenTime = TimeManager.TotalElapsedGameTime;
									}));
									return;
								case "Conversation_NPC/FollowerOnboarding/SickFollower":
									StartCoroutine(FrameDelayCallback(delegate
									{
										IllnessBar instance2 = IllnessBar.Instance;
										if ((object)instance2 != null)
										{
											instance2.Reveal();
										}
										if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Illness))
										{
											MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Illness);
										}
									}));
									animation = "Sick/chunder";
									break;
								case "Conversation_NPC/FollowerOnboarding/CureDissenter":
									StartCoroutine(FrameDelayCallback(delegate
									{
										if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Dissenter))
										{
											MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Dissenter);
										}
									}));
									animation = "tantrum-big";
									timer = 6f;
									break;
								case "Conversation_NPC/FollowerOnboardOldAge":
									quests.Clear();
									follower.SetOutfit(FollowerOutfitType.Old, false);
									break;
								case "FollowerInteractions/GiveQuest/CrisisOfFaith":
									animation = string.Format("Conversations/react-hate{0}", UnityEngine.Random.Range(1, 3));
									timer = 2f;
									break;
								}
								foreach (ObjectivesData item7 in quests)
								{
									if (item7 != null)
									{
										objective = item7;
										ObjectiveManager.Add(objective, true);
									}
								}
								DataManager.Instance.CurrentOnboardingFollowerID = -1;
								DataManager.Instance.LastFollowerQuestGivenTime = TimeManager.TotalElapsedGameTime;
							}
							else if (complaintType == Follower.ComplaintType.GiveQuest && objective != null)
							{
								StartCoroutine(AcceptQuestIE(objective));
								return;
							}
							Close(false, true, false);
							switch (DataManager.Instance.CurrentOnboardingFollowerTerm)
							{
							case "Conversation_NPC/FollowerOnboarding/SickFollower":
								transitioningCursedState = Thought.Ill;
								break;
							case "Conversation_NPC/FollowerOnboarding/CureDissenter":
								transitioningCursedState = Thought.Dissenter;
								break;
							case "Conversation_NPC/FollowerOnboardOldAge":
								transitioningCursedState = Thought.OldAge;
								break;
							}
							follower.TimedAnimation(animation, timer, delegate
							{
								follower.Brain.CompleteCurrentTask();
								if (transitioningCursedState == Thought.Ill)
								{
									follower.Brain.MakeSick();
									StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.VOMIT, 0);
									infoByType.FollowerID = follower.Brain.Info.ID;
									PlacementRegion.TileGridTile tileGridTile = null;
									if ((bool)PlacementRegion.Instance)
									{
										tileGridTile = StructureManager.GetClosestTileGridTileAtWorldPosition(base.transform.position, PlacementRegion.Instance.StructureInfo.Grid, 1f);
									}
									if (tileGridTile != null)
									{
										infoByType.GridTilePosition = tileGridTile.Position;
										StructureManager.BuildStructure(FollowerLocation.Base, infoByType, tileGridTile.WorldPosition, Vector2Int.one, false);
									}
									else
									{
										StructureManager.BuildStructure(FollowerLocation.Base, infoByType, base.transform.position, Vector2Int.one, false);
									}
								}
								else if (transitioningCursedState == Thought.Dissenter)
								{
									follower.Brain.MakeDissenter();
								}
								else if (transitioningCursedState == Thought.OldAge)
								{
									follower.Brain.MakeOld();
								}
								transitioningCursedState = Thought.None;
								DataManager.Instance.CurrentOnboardingFollowerTerm = "";
								Close(true, true, false);
							});
						}
					}), !WillLevelUp);
					MMConversation.PlayVO = PlayVO;
				}));
			}
			else
			{
				UnityEngine.Object.FindObjectOfType<CameraFollowTarget>().SetOffset(new Vector3(0f, 0f, -1f));
				UIFollowerInteractionWheelOverlayController uIFollowerInteractionWheelOverlayController = MonoSingleton<UIManager>.Instance.FollowerInteractionWheelTemplate.Instantiate();
				uIFollowerInteractionWheelOverlayController.Show(follower, commandItems);
				uIFollowerInteractionWheelOverlayController.OnItemChosen = (Action<FollowerCommands[]>)Delegate.Combine(uIFollowerInteractionWheelOverlayController.OnItemChosen, new Action<FollowerCommands[]>(OnFollowerCommandFinalized));
				uIFollowerInteractionWheelOverlayController.OnHidden = (Action)Delegate.Combine(uIFollowerInteractionWheelOverlayController.OnHidden, (Action)delegate
				{
				});
				uIFollowerInteractionWheelOverlayController.OnCancel = (Action)Delegate.Combine(uIFollowerInteractionWheelOverlayController.OnCancel, (Action)delegate
				{
					Close(true, true, false);
				});
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
				HUD_Manager.Instance.Hide(false, 0);
			}
		});
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (transitioningCursedState != 0)
		{
			if (transitioningCursedState == Thought.Ill)
			{
				follower.Brain.MakeSick();
			}
			else if (transitioningCursedState == Thought.Dissenter)
			{
				follower.Brain.MakeDissenter();
			}
			else if (transitioningCursedState == Thought.OldAge)
			{
				follower.Brain.MakeOld();
			}
			transitioningCursedState = Thought.None;
		}
	}

	private IEnumerator WaitForRoutine(Coroutine routine, Action callback)
	{
		if (routine != null)
		{
			yield return new WaitForSeconds(1f);
			yield return routine;
		}
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator GiveItemsRoutine(InventoryItem.ITEM_TYPE itemType, int quantity)
	{
		for (int i = 0; i < Mathf.Max(quantity, 10); i++)
		{
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, itemType, null);
			yield return new WaitForSeconds(0.025f);
		}
		Inventory.ChangeItemQuantity(itemType, -quantity);
	}

	private IEnumerator DelayedCurse(Thought cursedType, float delay)
	{
		yield return new WaitForSeconds(delay);
		switch (cursedType)
		{
		case Thought.Dissenter:
			follower.Brain.MakeDissenter();
			break;
		case Thought.Ill:
			follower.Brain.MakeSick();
			break;
		}
	}

	private void OnConversationEnd(bool SetPlayerToIdle = true, bool ShowHUD = true)
	{
		MMConversation.OnConversationEnd -= OnConversationEnd;
		ShowOtherFollowers();
	}

	public IEnumerator GiveDiscipleRewardRoutine(FollowerTaskType previousTask, Action Callback = null, bool GoToAndStop = true)
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		HUD_Manager.Instance.Hide(false, 0);
		yield return new WaitForEndOfFrame();
		SimulationManager.Pause();
		follower.FacePosition(PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("sermons/sermon-loop", 0, true, 0f);
		follower.SetBodyAnimation("devotion/devotion-start", false);
		follower.AddBodyAnimation("devotion/devotion-waiting", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/player/float_follower", follower.gameObject);
		follower.AdorationUI.Show();
		yield return new WaitForSeconds(0.25f);
		follower.Brain.Stats.Adoration = 0f;
		follower.Brain.Info.XPLevel++;
		Action onGivenRewards = OnGivenRewards;
		if (onGivenRewards != null)
		{
			onGivenRewards();
		}
		float SpeedUpSequenceMultiplier = 0.75f;
		follower.AdorationUI.BarController.ShrinkBarToEmpty(2f * SpeedUpSequenceMultiplier);
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower_noBookPage", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.transform.position);
		float DevotionToGive = 20f;
		while (true)
		{
			float num = DevotionToGive - 1f;
			DevotionToGive = num;
			if (!(num >= 0f))
			{
				break;
			}
			if (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(PlayerFarming.Instance.CameraBone, follower.CameraBone.transform.position, Color.white, delegate
				{
					PlayerFarming.Instance.GetSoul(1);
				});
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			yield return new WaitForSeconds(0.1f * SpeedUpSequenceMultiplier);
		}
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.transform.position);
		if (DoctrineUpgradeSystem.TrySermonsStillAvailable() && DoctrineUpgradeSystem.TryGetStillDoctrineStone() && DataManager.Instance.GetVariable(DataManager.Variables.FirstDoctrineStone))
		{
			PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.DOCTRINE_STONE, 1, base.transform.position);
			if (pickUp != null)
			{
				Interaction_DoctrineStone component = pickUp.GetComponent<Interaction_DoctrineStone>();
				if (component != null)
				{
					component.AutomaticallyInteract = true;
					component.MagnetToPlayer();
				}
			}
		}
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		follower.SetBodyAnimation("Reactions/react-bow", false);
		follower.AddBodyAnimation("idle", true, 0f);
		yield return new WaitForSeconds(0.5f);
		Debug.Log("Complete task!");
		follower.Brain.CompleteCurrentTask();
		yield return new WaitForSeconds(0.5f);
		if (previousTask == FollowerTaskType.Sleep)
		{
			follower.Brain._directInfoAccess.WakeUpDay = -1;
		}
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.LoyaltyCollectReward);
		if (Callback != null)
		{
			Callback();
		}
		Close(true, true, false);
	}

	private IEnumerator AcceptQuestIE(ObjectivesData objective)
	{
		yield return new WaitForEndOfFrame();
		Objectives_Story story = null;
		if (objective is Objectives_Story)
		{
			story = objective as Objectives_Story;
			((Objectives_Story)objective).StoryDataItem.QuestGiven = true;
			if (((Objectives_Story)objective).ParentStoryDataItem != null)
			{
				foreach (StoryDataItem childStoryDataItem in ((Objectives_Story)objective).ParentStoryDataItem.ChildStoryDataItems)
				{
					if (!childStoryDataItem.QuestGiven)
					{
						childStoryDataItem.QuestLocked = true;
					}
				}
			}
			objective = ((Objectives_Story)objective).StoryDataItem.Objective;
			if (!story.StoryDataItem.StoryObjectiveData.HasTimer)
			{
				objective.QuestExpireDuration = -1f;
			}
			objective.ResetInitialisation();
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameObject g = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Choice Indicator"), GameObject.FindWithTag("Canvas").transform) as GameObject;
		ChoiceIndicator choice = g.GetComponent<ChoiceIndicator>();
		choice.Offset = new Vector3(0f, -300f);
		if (!objective.IsInitialised())
		{
			objective.Init(true);
		}
		objective.Follower = follower.Brain.Info.ID;
		Quests.AddObjectiveToHistory(objective.Index, objective.QuestCooldown);
		choice.Show("UI/Generic/Accept", "FollowerInteractions/AcceptQuest", "UI/Generic/Decline", "FollowerInteractions/DeclineQuest", delegate
		{
			AcceptedQuest(objective);
			follower.TimedAnimation("Reactions/react-happy1", 1.8666667f, delegate
			{
				follower.Brain.CompleteCurrentTask();
			});
		}, delegate
		{
			if (story != null)
			{
				story.StoryDataItem.QuestDeclined = true;
				foreach (StoryDataItem childStoryDataItem2 in story.StoryDataItem.ChildStoryDataItems)
				{
					childStoryDataItem2.QuestDeclined = true;
				}
			}
			follower.TimedAnimation("Reactions/react-sad", 1.8666667f, delegate
			{
				follower.Brain.CompleteCurrentTask();
			});
			CultFaithManager.AddThought(Thought.Cult_DeclinedQuest, -1, 1f);
		}, base.transform.position);
		choice.ShowObjectivesBox(objective);
		while (g != null)
		{
			choice.UpdatePosition(base.transform.position);
			yield return null;
		}
		Objectives_RecruitCursedFollower objectives_RecruitCursedFollower;
		if ((objectives_RecruitCursedFollower = objective as Objectives_RecruitCursedFollower) != null && objectives_RecruitCursedFollower.CursedState == Thought.Dissenter && DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Dissenter))
		{
			UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Dissenter);
			uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
			{
				Close(true, true, false);
			});
		}
		else
		{
			Close(true, true, false);
		}
	}

	private void AcceptedQuest(ObjectivesData quest)
	{
		if (quest is Objectives_RecruitCursedFollower)
		{
			Objectives_RecruitCursedFollower objectives_RecruitCursedFollower = quest as Objectives_RecruitCursedFollower;
			if (BiomeBaseManager.Instance.SpawnExistingRecruits && DataManager.Instance.Followers_Recruit.Count == 0)
			{
				BiomeBaseManager.Instance.SpawnExistingRecruits = false;
			}
			for (int i = 0; i < objectives_RecruitCursedFollower.Target; i++)
			{
				FollowerInfo followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base);
				followerInfo.StartingCursedState = objectives_RecruitCursedFollower.CursedState;
				FollowerManager.CreateNewRecruit(followerInfo, NotificationCentre.NotificationType.NewRecruit);
			}
		}
		if (quest.Type == Objectives.TYPES.EAT_MEAL && ((Objectives_EatMeal)quest).MealType == StructureBrain.TYPES.MEAL_POOP && !DataManager.Instance.RecipesDiscovered.Contains(InventoryItem.ITEM_TYPE.MEAL_POOP))
		{
			DataManager.Instance.RecipesDiscovered.Add(InventoryItem.ITEM_TYPE.MEAL_POOP);
		}
		if (quest.Type == Objectives.TYPES.EAT_MEAL && ((Objectives_EatMeal)quest).MealType == StructureBrain.TYPES.MEAL_GRASS && !DataManager.Instance.RecipesDiscovered.Contains(InventoryItem.ITEM_TYPE.MEAL_GRASS))
		{
			DataManager.Instance.RecipesDiscovered.Add(InventoryItem.ITEM_TYPE.MEAL_GRASS);
		}
		ObjectiveManager.Add(quest);
	}

	private void NamedCult(string result)
	{
		DataManager.Instance.CultName = result;
		ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, string.Format(LocalizationManager.GetTranslation("Conversation_NPC/FollowerOnboarding/NameCult2"), result));
		conversationEntry.CharacterName = "<color=yellow>" + follower.Brain.Info.Name + "</color>";
		ConversationObject conversationObject = new ConversationObject(new List<ConversationEntry> { conversationEntry }, null, delegate
		{
			eventListener.PlayFollowerVO(positiveAcknowledgeVO);
			follower.TimedAnimation("Reactions/react-happy1", 1.8666667f, delegate
			{
				follower.Brain.CompleteCurrentTask();
			});
			follower.Brain.AddThought(Thought.LeaderDidQuest);
			AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
			StartCoroutine(FrameDelayCallback(delegate
			{
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
				follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Quest, delegate
				{
					if (follower.Brain.Stats.Adoration >= follower.Brain.Stats.MAX_ADORATION)
					{
						StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType, null, false));
					}
					else
					{
						Close();
					}
				});
			}));
		});
		conversationEntry.SetZoom = true;
		conversationEntry.Zoom = 4f;
		MMConversation.Play(conversationObject, true, true, true, false);
		MMConversation.PlayVO = PlayVO;
	}

	private void CacheAndSetFollower()
	{
		StartCoroutine(CacheAndSetFollowerRoutine());
	}

	private IEnumerator CacheAndSetFollowerRoutine()
	{
		yield return new WaitForEndOfFrame();
		if (follower.Brain != null)
		{
			Task = new FollowerTask_ManualControl();
			follower.Brain.HardSwapToTask(Task);
		}
		yield return new WaitForEndOfFrame();
		CacheSpineAnimator = follower.GetComponentInChildren<SimpleSpineAnimator>();
		if ((bool)CacheSpineAnimator)
		{
			CacheSpineAnimator.enabled = false;
		}
		if (follower != null && follower.Spine != null && follower.Spine.AnimationState != null && follower.Spine.AnimationState.GetCurrent(1) != null)
		{
			CacheAnimation = follower.Spine.AnimationState.GetCurrent(1).Animation.Name;
			CacheLoop = follower.Spine.AnimationState.GetCurrent(1).Loop;
			CacheAnimationProgress = follower.Spine.AnimationState.GetCurrent(1).TrackTime;
			CacheFacing = follower.Spine.Skeleton.ScaleX;
		}
		string animName = "Worship/worship";
		if (follower.Brain.Info.CursedState == Thought.Dissenter)
		{
			animName = "Worship/worship-dissenter";
		}
		else if (follower.Brain.Info.CursedState == Thought.Ill)
		{
			animName = "Worship/worship-sick";
		}
		else if (follower.Brain.Info.CursedState == Thought.BecomeStarving)
		{
			animName = "Worship/worship-hungry";
		}
		else if (follower.Brain.CurrentState != null && follower.Brain.CurrentState.Type == FollowerStateType.Exhausted)
		{
			animName = "Worship/worship-fatigued";
		}
		follower.SetBodyAnimation(animName, true);
		follower.FacePosition(state.transform.position);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
	}

	private void ResetFollower()
	{
		if (follower == null)
		{
			return;
		}
		if ((bool)CacheSpineAnimator)
		{
			CacheSpineAnimator.enabled = true;
		}
		if (follower.Spine != null)
		{
			if (!string.IsNullOrEmpty(CacheAnimation) && follower.Spine.Skeleton.Data.FindAnimation(CacheAnimation) != null)
			{
				follower.SetBodyAnimation(CacheAnimation, CacheLoop);
			}
			if (follower.Spine.Skeleton != null)
			{
				follower.Spine.Skeleton.ScaleX = CacheFacing;
			}
			if (follower.Spine.AnimationState.GetCurrent(1) != null)
			{
				follower.Spine.AnimationState.GetCurrent(1).TrackTime = CacheAnimationProgress;
			}
		}
		if (follower.Brain != null && follower.Brain.CurrentTaskType == FollowerTaskType.ManualControl)
		{
			follower.Brain.CompleteCurrentTask();
		}
	}

	public void SelectTask(StateMachine state, bool Cancellable, bool GiveDoctrinePieceOnClose)
	{
		this.GiveDoctrinePieceOnClose = GiveDoctrinePieceOnClose;
		StartCoroutine(SelectTaskRoutine(state));
	}

	private IEnumerator SelectTaskRoutine(StateMachine state)
	{
		base.state = state;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0f, -1f));
		yield return StartCoroutine(CacheAndSetFollowerRoutine());
		HUD_Manager.Instance.Hide(false, 0);
		follower.Spine.transform.localScale = new Vector3(1f, 1f, 1f);
		SimulationManager.Pause();
		if (follower.Brain._directInfoAccess.StartingCursedState != 0)
		{
			Debug.Log("1 CALL BACK!");
			Close(true, true, false);
		}
		else if (FollowerCommandGroups.GiveWorkerCommands(follower).Count > 0)
		{
			UIFollowerInteractionWheelOverlayController uIFollowerInteractionWheelOverlayController = MonoSingleton<UIManager>.Instance.FollowerInteractionWheelTemplate.Instantiate();
			uIFollowerInteractionWheelOverlayController.Show(follower, FollowerCommandGroups.GiveWorkerCommands(follower), false, false);
			uIFollowerInteractionWheelOverlayController.OnItemChosen = (Action<FollowerCommands[]>)Delegate.Combine(uIFollowerInteractionWheelOverlayController.OnItemChosen, new Action<FollowerCommands[]>(OnFollowerCommandFinalized));
		}
		else
		{
			Debug.Log("3 CALL BACK!");
			Close();
		}
	}

	private void OnFollowerCommandFinalized(params FollowerCommands[] followerCommands)
	{
		FollowerCommands followerCommands2 = followerCommands[0];
		FollowerCommands preFinalCommand = ((followerCommands.Length > 1) ? followerCommands[1] : FollowerCommands.None);
		Debug.Log(string.Format("Follower Command Finalized with {0} and {1}", followerCommands2, preFinalCommand).Colour(Color.green));
		switch (followerCommands2)
		{
		case FollowerCommands.GiveWorkerCommand_2:
		case FollowerCommands.MakeDemand:
			if (follower.Brain.Info.CursedState == Thought.OldAge)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("TooOldToWork");
				return;
			}
			if (follower.Brain.Info.CursedState == Thought.Ill)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("TooIllToWork");
				return;
			}
			if (follower.Brain.Info.CursedState == Thought.BecomeStarving)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("TooHungryToWork");
				return;
			}
			if (FollowerBrainStats.IsHoliday)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("HolidayIsActive");
				return;
			}
			break;
		case FollowerCommands.Build:
		{
			int num2 = StructureManager.GetAllStructuresOfType<Structures_BuildSite>(PlayerFarming.Location).Count + StructureManager.GetAllStructuresOfType<Structures_BuildSiteProject>(PlayerFarming.Location).Count;
			follower.Brain.Stats.WorkerBeenGivenOrders = true;
			if (num2 > 0)
			{
				eventListener.PlayFollowerVO(generalAcknowledgeVO);
				ConvertToWorker();
				follower.Brain.Info.WorkerPriority = WorkerPriority.None;
				follower.Brain.CompleteCurrentTask();
				break;
			}
			eventListener.PlayFollowerVO(negativeAcknowledgeVO);
			CloseAndSpeak("NoBuildingSites");
			return;
		}
		case FollowerCommands.CutTrees:
		case FollowerCommands.ForageBerries:
		case FollowerCommands.ClearWeeds:
		case FollowerCommands.ClearRubble:
		case FollowerCommands.WorshipAtShrine:
		case FollowerCommands.Cook_2:
		case FollowerCommands.Farmer_2:
		case FollowerCommands.FaithEnforcer:
		case FollowerCommands.TaxEnforcer:
		case FollowerCommands.CollectTax:
		case FollowerCommands.Janitor_2:
		case FollowerCommands.Refiner_2:
		case FollowerCommands.Undertaker:
		{
			ConvertToWorker();
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.Stats.WorkerBeenGivenOrders = true;
			FollowerRole NewRole;
			FollowerRole followerRole = (NewRole = follower.Brain.Info.FollowerRole);
			switch (followerCommands2)
			{
			case FollowerCommands.CutTrees:
				NewRole = FollowerRole.Lumberjack;
				break;
			case FollowerCommands.ClearRubble:
				NewRole = FollowerRole.StoneMiner;
				break;
			case FollowerCommands.Farmer_2:
				NewRole = FollowerRole.Farmer;
				break;
			case FollowerCommands.Cook_2:
				NewRole = FollowerRole.Chef;
				break;
			case FollowerCommands.Janitor_2:
				NewRole = FollowerRole.Janitor;
				break;
			case FollowerCommands.Undertaker:
				NewRole = FollowerRole.Undertaker;
				break;
			case FollowerCommands.ForageBerries:
				NewRole = FollowerRole.Forager;
				break;
			case FollowerCommands.Refiner_2:
				NewRole = FollowerRole.Refiner;
				break;
			case FollowerCommands.CollectTax:
			{
				for (int i = 0; i < follower.Brain._directInfoAccess.TaxCollected; i++)
				{
					ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, follower.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
				}
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", follower.transform.position);
				Inventory.AddItem(20, follower.Brain._directInfoAccess.TaxCollected);
				follower.Brain._directInfoAccess.TaxCollected = 0;
				break;
			}
			case FollowerCommands.WorshipAtShrine:
				NewRole = FollowerRole.Worshipper;
				if (followerRole != NewRole)
				{
					follower.Brain.CheckChangeTask();
					ShowDivineInspirationTutorialOnClose = true;
				}
				break;
			}
			if (NewRole == followerRole)
			{
				break;
			}
			follower.Brain.NewRoleSet(NewRole);
			follower.Brain.SetPersonalOverrideTask(FollowerTask.GetFollowerTaskFromRole(NewRole));
			StartCoroutine(FrameDelayCallback(delegate
			{
				follower.Brain.Info.FollowerRole = NewRole;
				follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
				follower.SetOutfit(FollowerOutfitType.Follower, false);
				List<FollowerTask> allUnoccupiedTasksOfType = FollowerBrain.GetAllUnoccupiedTasksOfType(FollowerTask.GetFollowerTaskFromRole(NewRole));
				if (allUnoccupiedTasksOfType.Count > 0)
				{
					follower.Brain.HardSwapToTask(allUnoccupiedTasksOfType[UnityEngine.Random.Range(0, allUnoccupiedTasksOfType.Count)]);
				}
			}));
			break;
		}
		case FollowerCommands.Study:
			if (follower.Brain.Info.FollowerRole != FollowerRole.Monk)
			{
				eventListener.PlayFollowerVO(generalAcknowledgeVO);
				follower.Brain.Info.FollowerRole = FollowerRole.Monk;
				follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
				follower.SetOutfit(FollowerOutfitType.Follower, false);
				follower.Brain.CheckChangeTask();
			}
			follower.Brain.Stats.WorkerBeenGivenOrders = true;
			break;
		case FollowerCommands.Surveillance:
		{
			UIFollowerInteractionWheelOverlayController activeMenu = UIManager.GetActiveMenu<UIFollowerInteractionWheelOverlayController>();
			if (activeMenu != null)
			{
				activeMenu.OnHidden = null;
			}
			if (base.transform.position.x < state.transform.position.x)
			{
				GameManager.GetInstance().CameraSetOffset(new Vector3(-1f, 0f, -1f));
			}
			else
			{
				GameManager.GetInstance().CameraSetOffset(new Vector3(-2.5f, 0f, -1f));
			}
			UIFollowerSummaryMenuController followerSummaryMenuInstance = MonoSingleton<UIManager>.Instance.ShowFollowerSummaryMenu(follower);
			UIFollowerSummaryMenuController uIFollowerSummaryMenuController = followerSummaryMenuInstance;
			uIFollowerSummaryMenuController.OnHidden = (Action)Delegate.Combine(uIFollowerSummaryMenuController.OnHidden, (Action)delegate
			{
				followerSummaryMenuInstance = null;
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.ReadMind);
				Close(true, true, false);
			});
			HUD_Manager.Instance.Hide(false, 0);
			return;
		}
		case FollowerCommands.Sleep:
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.Sleep);
			follower.Brain.CompleteCurrentTask();
			follower.Brain._directInfoAccess.WakeUpDay = -1;
			break;
		case FollowerCommands.BedRest:
		{
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			List<Structures_HealingBay> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_HealingBay>(FollowerLocation.Base);
			bool flag = false;
			for (int num = allStructuresOfType.Count - 1; num >= 0; num--)
			{
				if (!allStructuresOfType[num].CheckIfOccupied() || allStructuresOfType[num].Data.FollowerID == follower.Brain.Info.ID)
				{
					flag = true;
					break;
				}
			}
			if (!flag && follower.Brain.HasHome && Dwelling.GetDwellingByID(follower.Brain._directInfoAccess.DwellingID) != null && Dwelling.GetDwellingByID(follower.Brain._directInfoAccess.DwellingID).StructureBrain.IsCollapsed)
			{
				CloseAndSpeak("SendToBedRest/BrokenBed", delegate
				{
					follower.Brain.CompleteCurrentTask();
					follower.Brain.HardSwapToTask(new FollowerTask_SleepBedRest());
					follower.Brain.SetPersonalOverrideTask(FollowerTaskType.SleepBedRest);
				});
				return;
			}
			if (previousTaskType != FollowerTaskType.ClaimDwelling)
			{
				follower.Brain.CompleteCurrentTask();
				follower.Brain.HardSwapToTask(new FollowerTask_SleepBedRest());
				follower.Brain.SetPersonalOverrideTask(FollowerTaskType.SleepBedRest);
			}
			else
			{
				StartCoroutine(DelayCallback(1f, delegate
				{
					follower.Brain.CurrentOverrideTaskType = FollowerTaskType.SleepBedRest;
				}));
			}
			break;
		}
		case FollowerCommands.Gift:
			eventListener.PlayFollowerVO(negativeAcknowledgeVO);
			CloseAndSpeak("NoGifts");
			return;
		case FollowerCommands.Gift_Small:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.GIFT_SMALL));
			return;
		case FollowerCommands.Gift_Medium:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.GIFT_MEDIUM));
			return;
		case FollowerCommands.Gift_Necklace1:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_1));
			return;
		case FollowerCommands.Gift_Necklace2:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_2));
			return;
		case FollowerCommands.Gift_Necklace3:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_3));
			return;
		case FollowerCommands.Gift_Necklace4:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_4));
			return;
		case FollowerCommands.Gift_Necklace5:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_5));
			return;
		case FollowerCommands.Gift_Necklace_Dark:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_Dark));
			return;
		case FollowerCommands.Gift_Necklace_Light:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_Light));
			return;
		case FollowerCommands.Gift_Necklace_Demonic:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_Demonic));
			return;
		case FollowerCommands.Gift_Necklace_Loyalty:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_Loyalty));
			return;
		case FollowerCommands.Gift_Necklace_Gold_Skull:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_Gold_Skull));
			return;
		case FollowerCommands.Gift_Necklace_Missionary:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_Missionary));
			return;
		case FollowerCommands.RemoveNecklace:
			StartCoroutine(GiveItemRoutine(InventoryItem.ITEM_TYPE.Necklace_5));
			return;
		case FollowerCommands.GiveLeaderItem:
		{
			FollowerLocation location = FollowerLocation.Dungeon1_1;
			InventoryItem.ITEM_TYPE item = InventoryItem.ITEM_TYPE.FLOWER_RED;
			if (follower.Brain.Info.ID == FollowerManager.HeketID)
			{
				location = FollowerLocation.Dungeon1_2;
				item = InventoryItem.ITEM_TYPE.MUSHROOM_SMALL;
			}
			else if (follower.Brain.Info.ID == FollowerManager.KallamarID)
			{
				location = FollowerLocation.Dungeon1_3;
				item = InventoryItem.ITEM_TYPE.CRYSTAL;
			}
			else if (follower.Brain.Info.ID == FollowerManager.ShamuraID)
			{
				location = FollowerLocation.Dungeon1_4;
				item = InventoryItem.ITEM_TYPE.SPIDER_WEB;
			}
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
			DataManager.Instance.SecretItemsGivenToFollower.Add(location);
			GameManager.GetInstance().WaitForSeconds(1f, delegate
			{
				Inventory.ChangeItemQuantity(item, -1);
				ResourceCustomTarget.Create(follower.gameObject, PlayerFarming.Instance.CameraBone.transform.position, item, delegate
				{
					List<ConversationEntry> list = new List<ConversationEntry>
					{
						new ConversationEntry(base.gameObject, string.Format("Conversation_NPC/{0}/SecretQuest/0", location))
					};
					list[0].pitchValue = follower.Brain._directInfoAccess.follower_pitch;
					list[0].vibratoValue = follower.Brain._directInfoAccess.follower_vibrato;
					list[0].soundPath = generalTalkVO;
					list[0].SkeletonData = follower.Spine;
					list[0].CharacterName = "<color=yellow>" + follower.Brain.Info.Name + "</color>";
					list[0].SetZoom = true;
					list[0].Zoom = 4f;
					int num3 = 0;
					while (true)
					{
						string termToSpeak = ConversationEntry.Clone(list[0]).TermToSpeak;
						int num4 = ++num3;
						if (LocalizationManager.GetTermData(termToSpeak.Replace("0", num4.ToString())) == null)
						{
							break;
						}
						ConversationEntry conversationEntry = ConversationEntry.Clone(list[0]);
						conversationEntry.TermToSpeak = conversationEntry.TermToSpeak.Replace("0", num3.ToString());
						list.Add(conversationEntry);
					}
					foreach (ConversationEntry item2 in list)
					{
						item2.soundPath = (PlayVO ? item2.soundPath : "");
					}
					MMConversation.Play(new ConversationObject(list, null, delegate
					{
						Close(true, true, false);
					}));
					MMConversation.PlayVO = PlayVO;
				});
			});
			return;
		}
		case FollowerCommands.EatSomething:
			if (FollowerBrainStats.Fasting)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("CantEatFasting");
			}
			else
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("NoMeals");
			}
			return;
		case FollowerCommands.Meal:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealGrass:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_GRASS);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_GRASS);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealPoop:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_POOP);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_POOP);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealMushrooms:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_MUSHROOMS);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_MUSHROOMS);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealFollowerMeat:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_FOLLOWER_MEAT);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_FOLLOWER_MEAT);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealGoodFish:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_GOOD_FISH);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_GOOD_FISH);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealGreat:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_GREAT);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_GREAT);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealMeat:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_MEAT);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_MEAT);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealBerries:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_BERRIES);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_BERRIES);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealDeadly:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_DEADLY);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_DEADLY);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealMediumVeg:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_MEDIUM_VEG);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_MEDIUM_VEG);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealMeatLow:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_BAD_MEAT);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_BAD_MEAT);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealMeatHigh:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_GREAT_MEAT);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_GREAT_MEAT);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealMixedLow:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_BAD_MIXED);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_BAD_MIXED);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealMixedMedium:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_MEDIUM_MIXED);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_MEDIUM_MIXED);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealMixedHigh:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_GREAT_MIXED);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_GREAT_MIXED);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealBadFish:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_BAD_FISH);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_BAD_FISH);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.MealGreatFish:
			follower.Brain.CancelTargetedMeal(StructureBrain.TYPES.MEAL_GREAT_FISH);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureBrain.TYPES.MEAL_GREAT_FISH);
			follower.Brain.CompleteCurrentTask();
			break;
		case FollowerCommands.Ascend:
			if (follower.Brain.Stats.Happiness < 80f)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("AscendFaithTooLow");
			}
			return;
		case FollowerCommands.AreYouSureYes:
			Task.GoToAndStop(follower, PlayerFarming.Instance.transform.position + Vector3.left * ((follower.transform.position.x < PlayerFarming.Instance.transform.position.x) ? 1.5f : (-1.5f)), delegate
			{
				switch (preFinalCommand)
				{
				case FollowerCommands.Murder:
					eventListener.PlayFollowerVO(generalAcknowledgeVO);
					PlayerFarming.Instance.StartCoroutine(MurderFollower());
					break;
				case FollowerCommands.Ascend:
					eventListener.PlayFollowerVO(generalAcknowledgeVO);
					PlayerFarming.Instance.StartCoroutine(AscendFollower());
					break;
				}
			});
			return;
		case FollowerCommands.ExtortMoney:
			if (follower.Brain.Stats.PaidTithes)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				Debug.Log("ALREADY TITHES!");
				CloseAndSpeak("AlreadyPaidTithes");
			}
			else
			{
				eventListener.PlayFollowerVO(generalAcknowledgeVO);
				follower.Brain.Stats.PaidTithes = true;
				StartCoroutine(ExtortMoneyRoutine());
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
			}
			return;
		case FollowerCommands.Bribe:
			if (follower.Brain.Stats.Bribed)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				Debug.Log("ALREADY BRIBED! ");
				CloseAndSpeak("AlreadyBribed");
			}
			else if (Inventory.GetItemQuantity(20) < 3)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("NoGoldBribe");
			}
			else
			{
				eventListener.PlayFollowerVO(generalAcknowledgeVO);
				StartCoroutine(BribeRoutine());
			}
			return;
		case FollowerCommands.Imprison:
		{
			BiomeConstants.Instance.DepthOfFieldTween(1.5f, 5f, 10f, 1f, 145f);
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.SetBodyAnimation("picked-up-hate", true);
			Prison ClosestPrison = GetClosestPrison();
			ClosestPrison.StructureInfo.FollowerID = follower.Brain.Info.ID;
			PlayerFarming.Instance.PickUpFollower(follower);
			PlayerFarming.Instance.GoToAndStop(ClosestPrison.PrisonerLocation.gameObject, ClosestPrison.gameObject, true, true, delegate
			{
				PlayerFarming.Instance.DropFollower();
				foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
				{
					if (allBrain.Location == follower.Brain.Location && allBrain != follower.Brain)
					{
						if (follower.Brain.Stats.Reeducation > 0f)
						{
							if (allBrain.CurrentTaskType == FollowerTaskType.Sleep)
							{
								allBrain.AddThought(Thought.DissenterImprisonedSleeping);
							}
							else if (allBrain.HasTrait(FollowerTrait.TraitType.Libertarian))
							{
								allBrain.AddThought(Thought.ImprisonedLibertarian);
							}
							else
							{
								allBrain.AddThought(Thought.DissenterImprisoned);
							}
						}
						else if (allBrain.CurrentTaskType == FollowerTaskType.Sleep)
						{
							allBrain.AddThought(Thought.InnocentImprisonedSleeping);
						}
						else if (allBrain.HasTrait(FollowerTrait.TraitType.Disciplinarian))
						{
							allBrain.AddThought(Thought.InnocentImprisonedDisciplinarian);
						}
						else if (allBrain.HasTrait(FollowerTrait.TraitType.Libertarian))
						{
							allBrain.AddThought(Thought.ImprisonedLibertarian);
						}
						else
						{
							allBrain.AddThought(Thought.InnocentImprisoned);
						}
					}
				}
				if (follower.Brain.Info.CursedState != Thought.Dissenter)
				{
					bool flag2 = false;
					foreach (ObjectivesData objective in DataManager.Instance.Objectives)
					{
						if (objective.Type == Objectives.TYPES.CUSTOM && ((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.SendFollowerToPrison && ((Objectives_Custom)objective).TargetFollowerID == follower.Brain.Info.ID)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						if (DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.Disciplinarian))
						{
							CultFaithManager.AddThought(Thought.Cult_Imprison_Trait, -1, 1f);
						}
						else
						{
							CultFaithManager.AddThought(Thought.Cult_Imprison, -1, 1f);
						}
					}
				}
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.SendFollowerToPrison, follower.Brain.Info.ID);
				AudioManager.Instance.PlayOneShot("event:/followers/imprison", follower.transform.position);
				follower.Brain.TransitionToTask(new FollowerTask_Imprisoned(ClosestPrison.StructureInfo.ID));
				ClosestPrison.StructureInfo.FollowerID = follower.Brain.Info.ID;
				ClosestPrison.StructureInfo.FollowerImprisonedTimestamp = TimeManager.TotalElapsedGameTime;
				ClosestPrison.StructureInfo.FollowerImprisonedFaith = follower.Brain.Stats.Reeducation;
				if (!DataManager.Instance.Followers_Imprisoned_IDs.Contains(follower.Brain.Info.ID))
				{
					DataManager.Instance.Followers_Imprisoned_IDs.Add(follower.Brain.Info.ID);
				}
				Close(false, true, false);
			}, 30f);
			return;
		}
		case FollowerCommands.Dance:
			if (follower.Brain.Stats.Inspired)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("AlreadyInspired");
				return;
			}
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.Stats.Inspired = true;
			Task.GoToAndStop(follower, PlayerFarming.Instance.transform.position + Vector3.left * ((follower.transform.position.x < PlayerFarming.Instance.transform.position.x) ? 1.5f : (-1.5f)), delegate
			{
				StartCoroutine(DanceRoutine(true));
			});
			return;
		case FollowerCommands.Intimidate:
			if (follower.Brain.Stats.Intimidated)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("AlreadyIntimidated");
				return;
			}
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.Stats.Intimidated = true;
			Task.GoToAndStop(follower, PlayerFarming.Instance.transform.position + Vector3.left * ((follower.transform.position.x < PlayerFarming.Instance.transform.position.x) ? 1.5f : (-1.5f)), delegate
			{
				StartCoroutine(IntimidateRoutine(true));
			});
			return;
		case FollowerCommands.Bless:
			if (follower.Brain.Stats.ReceivedBlessing)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("AlreadyGivenBlessing");
				return;
			}
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.Stats.ReceivedBlessing = true;
			Task.GoToAndStop(follower, PlayerFarming.Instance.transform.position + Vector3.left * ((follower.transform.position.x < PlayerFarming.Instance.transform.position.x) ? 1.5f : (-1.5f)), delegate
			{
				StartCoroutine(BlessRoutine(true));
			});
			return;
		case FollowerCommands.Reeducate:
			if (follower.Brain.Stats.ReeducatedAction)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("AlreadyReeducated");
				return;
			}
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.Stats.ReeducatedAction = true;
			Task.GoToAndStop(follower, PlayerFarming.Instance.transform.position + Vector3.left * ((follower.transform.position.x < PlayerFarming.Instance.transform.position.x) ? 1.5f : (-1.5f)), delegate
			{
				StartCoroutine(ReeducateRoutine());
			});
			return;
		case FollowerCommands.Romance:
			if (follower.Brain.Stats.KissedAction)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				CloseAndSpeak("AlreadySmooched");
				return;
			}
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			follower.Brain.Stats.KissedAction = true;
			Task.GoToAndStop(follower, PlayerFarming.Instance.transform.position + Vector3.left * ((follower.transform.position.x < PlayerFarming.Instance.transform.position.x) ? 1f : (-1f)), delegate
			{
				StartCoroutine(RomanceRoutine());
			});
			return;
		case FollowerCommands.PetDog:
			if (follower.Brain.Stats.PetDog)
			{
				eventListener.PlayFollowerVO(negativeAcknowledgeVO);
				Close();
				return;
			}
			follower.Brain.Stats.PetDog = true;
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			Task.GoToAndStop(follower, PlayerFarming.Instance.transform.position + Vector3.left * ((follower.transform.position.x < PlayerFarming.Instance.transform.position.x) ? 1f : (-1f)), delegate
			{
				StartCoroutine(PetDogRoutine());
			});
			return;
		case FollowerCommands.WakeUp:
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			if ((previousTaskType == FollowerTaskType.Sleep && follower.Brain.CurrentOverrideTaskType == FollowerTaskType.Sleep) || (previousTaskType == FollowerTaskType.SleepBedRest && follower.Brain.CurrentOverrideTaskType == FollowerTaskType.SleepBedRest))
			{
				follower.Brain.ClearPersonalOverrideTaskProvider();
			}
			follower.Brain.AddThought(Thought.SleepInterrupted);
			follower.Brain._directInfoAccess.BrainwashedUntil = TimeManager.CurrentDay;
			if (TimeManager.IsNight)
			{
				CultFaithManager.AddThought(Thought.Cult_WokeUpFollower, follower.Brain.Info.ID, 1f);
			}
			Close(false, true, false);
			base.enabled = true;
			follower.TimedAnimation("tantrum", 3.1666667f, delegate
			{
				Close(true, true, false);
				follower.Brain._directInfoAccess.WakeUpDay = TimeManager.CurrentDay;
				follower.Brain.CheckChangeTask();
			});
			return;
		case FollowerCommands.ViewTraits:
			Close(true, true, false);
			MonoSingleton<UIManager>.Instance.ShowFollowerSummaryMenu(follower);
			return;
		default:
			Debug.Log(string.Format("Warning! Unhandled Follower Command: {0}", followerCommands2).Colour(Color.red));
			break;
		case FollowerCommands.ChangeRole:
		case FollowerCommands.Talk:
			break;
		}
		if (followerCommands.Length == 1 && followerCommands[0] == FollowerCommands.GiveWorkerCommand_2)
		{
			CloseAndSpeak("NoTasksAvailable");
		}
		else
		{
			Close(true, true, false);
		}
	}

	private IEnumerator ExtortMoneyRoutine()
	{
		yield return new WaitForEndOfFrame();
		follower.FacePosition(PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		yield return new WaitForSeconds(0.25f);
		for (int i = 0; i < UnityEngine.Random.Range(3, 7); i++)
		{
			ResourceCustomTarget.Create(state.gameObject, base.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, delegate
			{
				Inventory.AddItem(20, 1);
			});
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.25f);
		GameManager.GetInstance().OnConversationEnd();
		Close();
	}

	private IEnumerator FrameDelayCallback(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator DelayCallback(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	private Prison GetClosestPrison()
	{
		Prison result = null;
		float num = float.MaxValue;
		foreach (Prison prison in Prison.Prisons)
		{
			if (prison.StructureInfo.FollowerID == -1)
			{
				float num2 = Vector2.Distance(state.transform.position, prison.transform.position);
				if (num2 < num)
				{
					result = prison;
					num = num2;
				}
			}
		}
		return result;
	}

	public bool AvailablePrisons()
	{
		foreach (Prison prison in Prison.Prisons)
		{
			if (prison.StructureInfo.FollowerID == -1)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator MurderFollower()
	{
		follower.HideAllFollowerIcons();
		follower.FacePosition(PlayerFarming.Instance.transform.position);
		follower.Brain.DiedFromMurder = true;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "murder", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower");
		AudioManager.Instance.PlayOneShot("event:/player/murder_follower_sequence");
		follower.SetBodyAnimation("murder", false);
		float Duration = follower.Spine.AnimationState.GetCurrent(1).Animation.Duration;
		GameManager.GetInstance().AddToCamera(follower.gameObject);
		yield return new WaitForSeconds(0.1f);
		follower.Spine.CustomMaterialOverride.Clear();
		follower.Spine.CustomMaterialOverride.Add(follower.NormalMaterial, follower.BW_Material);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(PlayerFarming.Instance.originalMaterial, PlayerFarming.Instance.BW_Material);
		HUD_Manager.Instance.ShowBW(0.33f, 0f, 1f);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(follower.transform.position, new Vector3(0.5f, 0.5f, 1f));
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		yield return new WaitForSeconds(1.6f);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(follower.transform.position, new Vector3(1f, 1f, 1f));
		CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.3f);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		follower.Spine.CustomMaterialOverride.Clear();
		HUD_Manager.Instance.ShowBW(0.33f, 1f, 0f);
		yield return new WaitForSeconds(Duration - 0.1f - 1.7f);
		JudgementMeter.ShowModify(-1);
		int dir = (int)follower.Spine.Skeleton.ScaleX;
		Close(false, true, false);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.MurderFollower, follower.Brain.Info.ID);
		if (TimeManager.CurrentPhase == DayPhase.Night)
		{
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.MurderFollowerAtNight, follower.Brain.Info.ID);
		}
		follower.Die(NotificationCentre.NotificationType.MurderedByYou, false, dir);
		DataManager.Instance.STATS_Murders++;
	}

	private IEnumerator AscendFollower()
	{
		follower.FacePosition(PlayerFarming.Instance.transform.position);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "resurrect", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		float seconds = follower.SetBodyAnimation("sacrifice", false);
		GameManager.GetInstance().AddToCamera(follower.gameObject);
		follower.Spine.CustomMaterialOverride.Clear();
		follower.Spine.CustomMaterialOverride.Add(follower.NormalMaterial, follower.BW_Material);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(PlayerFarming.Instance.originalMaterial, PlayerFarming.Instance.BW_Material);
		HUD_Manager.Instance.ShowBW(0.33f, 0f, 1f);
		yield return new WaitForSeconds(seconds);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		follower.Spine.CustomMaterialOverride.Clear();
		HUD_Manager.Instance.ShowBW(0.33f, 1f, 0f);
		Close();
		follower.Die(NotificationCentre.NotificationType.Ascended, false);
	}

	private IEnumerator IntimidateRoutine(bool hostFollower)
	{
		if (hostFollower)
		{
			float num = 3f;
			List<Follower> list = new List<Follower>();
			foreach (Follower follower2 in Follower.Followers)
			{
				if (follower2 != this.follower && follower2.Brain.Info.CursedState != Thought.Dissenter && !FollowerManager.FollowerLocked(follower2.Brain.Info.ID) && Vector3.Distance(follower2.transform.position, base.transform.position) < num && !follower2.Brain.Stats.Intimidated && follower2.Brain.CurrentTaskType != FollowerTaskType.Sleep)
				{
					list.Add(follower2);
				}
			}
			foreach (Follower follower in list)
			{
				FollowerTask_ManualControl followerTask_ManualControl = new FollowerTask_ManualControl();
				follower.Brain.HardSwapToTask(followerTask_ManualControl);
				followerTask_ManualControl.GoToAndStop(follower, base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 1.5f, delegate
				{
					follower.Brain.Stats.Intimidated = true;
					follower.StartCoroutine(follower.GetComponent<interaction_FollowerInteraction>().IntimidateRoutine(false));
				});
			}
			GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
			GameManager.GetInstance().AddPlayerToCamera();
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
			if (list.Count > 0)
			{
				yield return new WaitForSeconds(0.35f);
			}
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower", PlayerFarming.Instance.transform.position);
			AudioManager.Instance.PlayOneShot("event:/Stings/white_eyes", PlayerFarming.Instance.transform.position);
			HUD_Manager.Instance.ShowBW(0.33f, 0f, 1f);
		}
		this.follower.FacePosition(PlayerFarming.Instance.transform.position);
		this.follower.Spine.CustomMaterialOverride.Clear();
		this.follower.Spine.CustomMaterialOverride.Add(this.follower.NormalMaterial, this.follower.BW_Material);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(PlayerFarming.Instance.originalMaterial, PlayerFarming.Instance.BW_Material);
		this.follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		float num2 = this.follower.SetBodyAnimation("Reactions/react-intimidate", false);
		this.follower.AddBodyAnimation("idle", true, 0f);
		if (hostFollower)
		{
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "intimidate", false);
			PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
			AudioManager.Instance.PlayOneShot("event:/player/intimidate_follower", PlayerFarming.Instance.gameObject);
		}
		yield return new WaitForSeconds(num2 - 2.25f);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		this.follower.Spine.CustomMaterialOverride.Clear();
		if (hostFollower)
		{
			PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
			HUD_Manager.Instance.ShowBW(0.33f, 1f, 0f);
			yield return new WaitForSeconds(0.5f);
			AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BlessAFollower);
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
		}
		this.follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Intimidate, delegate
		{
			this.follower.Brain.AddThought(Thought.Intimidated);
			if (hostFollower)
			{
				if (this.follower.Brain.Stats.Adoration >= this.follower.Brain.Stats.MAX_ADORATION)
				{
					StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType));
				}
				else
				{
					Close();
				}
			}
			else
			{
				this.follower.Brain.CompleteCurrentTask();
			}
		});
	}

	public IEnumerator BlessRoutine(bool hostFollower)
	{
		if (hostFollower)
		{
			float num = 3f;
			List<Follower> list = new List<Follower>();
			foreach (Follower follower2 in Follower.Followers)
			{
				if (follower2 != this.follower && follower2.Brain.Info.CursedState != Thought.Dissenter && !FollowerManager.FollowerLocked(follower2.Brain.Info.ID) && Vector3.Distance(follower2.transform.position, base.transform.position) < num && !follower2.Brain.Stats.ReceivedBlessing && follower2.Brain.CurrentTaskType != FollowerTaskType.Sleep)
				{
					list.Add(follower2);
				}
			}
			foreach (Follower follower in list)
			{
				FollowerTask_ManualControl followerTask_ManualControl = new FollowerTask_ManualControl();
				follower.Brain.HardSwapToTask(followerTask_ManualControl);
				followerTask_ManualControl.GoToAndStop(follower, base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 1.5f, delegate
				{
					follower.Brain.Stats.ReceivedBlessing = true;
					follower.StartCoroutine(follower.GetComponent<interaction_FollowerInteraction>().BlessRoutine(false));
				});
			}
			GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
			GameManager.GetInstance().AddPlayerToCamera();
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
			if (list.Count > 0)
			{
				yield return new WaitForSeconds(0.35f);
			}
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower", PlayerFarming.Instance.transform.position);
			AudioManager.Instance.PlayOneShot("event:/Stings/white_eyes", PlayerFarming.Instance.transform.position);
		}
		this.follower.FacePosition(PlayerFarming.Instance.transform.position);
		this.follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		this.follower.SetBodyAnimation("devotion/devotion-start", false);
		this.follower.AddBodyAnimation("devotion/devotion-waiting", true, 0f);
		if (hostFollower)
		{
			yield return PlayerFarming.Instance.Spine.YieldForAnimation("bless");
			PlayerFarming.Instance.simpleSpineAnimator.Animate("idle", 0, true, 0f);
		}
		else
		{
			yield return new WaitForSeconds(1.25f);
		}
		this.follower.SetBodyAnimation("idle", true);
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BlessAFollower);
		this.follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Bless, delegate
		{
			CultFaithManager.AddThought(Thought.Cult_Bless, -1, 1f);
			if (hostFollower)
			{
				if (this.follower.Brain.Stats.Adoration >= this.follower.Brain.Stats.MAX_ADORATION)
				{
					StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType));
				}
				else
				{
					Close();
				}
			}
			else
			{
				this.follower.Brain.CompleteCurrentTask();
			}
		});
	}

	private IEnumerator ReeducateRoutine()
	{
		follower.FacePosition(PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		int num = 1;
		if ((follower.Brain.Stats.Reeducation + 7.5f) / 100f >= 1f)
		{
			num = 3;
		}
		else if ((follower.Brain.Stats.Reeducation + 7.5f) / 100f > 0.5f)
		{
			num = 2;
		}
		PlayerFarming.Instance.simpleSpineAnimator.Animate("reeducate-" + num, 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower", PlayerFarming.Instance.transform.position);
		follower.SetBodyAnimation("reeducate-" + num, false);
		follower.AddBodyAnimation("idle", true, 0f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(1.5f);
		follower.Brain.Stats.Reeducation -= 7.5f;
		if (follower.Brain.Stats.Reeducation > 0f && follower.Brain.Stats.Reeducation < 2f)
		{
			follower.Brain.Stats.Reeducation = 0f;
		}
		AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
		BiomeConstants.Instance.EmitHeartPickUpVFX(base.gameObject.transform.position, 0f, "black", "burst_big", false);
		yield return new WaitForSeconds(1f);
		if (follower.Brain.Stats.Reeducation <= 100f)
		{
			follower.TimedAnimation("Reactions/react-enlightened1", 2f, delegate
			{
				Close();
			});
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
			Close();
		}
	}

	private IEnumerator RomanceRoutine()
	{
		follower.FacePosition(PlayerFarming.Instance.transform.position);
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "kiss-follower", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		follower.SetBodyAnimation("kiss", true);
		follower.AddBodyAnimation("idle", true, 0f);
		yield return new WaitForSeconds(5f / 6f);
		AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
		BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big", false);
		follower.Brain.AddThought(Thought.SpouseKiss);
		yield return new WaitForSeconds(3f);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		follower.Spine.CustomMaterialOverride.Clear();
		AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
		follower.Brain.AddAdoration(FollowerBrain.AdorationActions.SmoochSpouse, delegate
		{
			if (follower.Brain.Stats.Adoration >= follower.Brain.Stats.MAX_ADORATION)
			{
				StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType));
			}
			else
			{
				Close();
			}
		});
	}

	private IEnumerator PetDogRoutine()
	{
		follower.FacePosition(PlayerFarming.Instance.transform.position);
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "pet-dog", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		follower.SetBodyAnimation("pet-dog", true);
		follower.AddBodyAnimation("idle", true, 0f);
		yield return new WaitForSeconds(5f / 6f);
		AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
		BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big", false);
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		follower.Spine.CustomMaterialOverride.Clear();
		yield return new WaitForSeconds(0.5f);
		follower.Brain.AddAdoration(FollowerBrain.AdorationActions.PetDog, delegate
		{
			if (follower.Brain.Stats.Adoration >= follower.Brain.Stats.MAX_ADORATION)
			{
				StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType));
			}
			else
			{
				Close();
			}
		});
	}

	private IEnumerator DanceRoutine(bool hostFollower)
	{
		if (hostFollower)
		{
			float num = 3f;
			List<Follower> list = new List<Follower>();
			foreach (Follower follower2 in Follower.Followers)
			{
				if (follower2 != this.follower && follower2.Brain.Info.CursedState != Thought.Dissenter && !FollowerManager.FollowerLocked(follower2.Brain.Info.ID) && Vector3.Distance(follower2.transform.position, base.transform.position) < num && !follower2.Brain.Stats.Inspired && follower2.Brain.CurrentTaskType != FollowerTaskType.Sleep)
				{
					list.Add(follower2);
				}
			}
			foreach (Follower follower in list)
			{
				FollowerTask_ManualControl followerTask_ManualControl = new FollowerTask_ManualControl();
				follower.Brain.HardSwapToTask(followerTask_ManualControl);
				followerTask_ManualControl.GoToAndStop(follower, base.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 1.5f, delegate
				{
					follower.Brain.Stats.Inspired = true;
					follower.StartCoroutine(follower.GetComponent<interaction_FollowerInteraction>().DanceRoutine(false));
				});
			}
			GameManager.GetInstance().OnConversationNext(base.gameObject, 4f);
			GameManager.GetInstance().AddPlayerToCamera();
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
			if (list.Count > 0)
			{
				yield return new WaitForSeconds(0.35f);
			}
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower", PlayerFarming.Instance.transform.position);
			AudioManager.Instance.PlayOneShot("event:/Stings/white_eyes", PlayerFarming.Instance.transform.position);
		}
		this.follower.State.CURRENT_STATE = StateMachine.State.Dancing;
		yield return null;
		if (hostFollower)
		{
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "dance", true);
			AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower", PlayerFarming.Instance.transform.position);
			AudioManager.Instance.SetFollowersDance(1f);
		}
		this.follower.SetBodyAnimation("dance", true);
		yield return new WaitForSeconds(1.5f);
		if (hostFollower)
		{
			AudioManager.Instance.SetFollowersDance(0f);
			AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.BlessAFollower);
		}
		this.follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Inspire, delegate
		{
			this.follower.Brain.AddThought(Thought.DancedWithLeader);
			CultFaithManager.AddThought(Thought.Cult_Inspire, -1, 1f);
			if (hostFollower)
			{
				eventListener.PlayFollowerVO(bowVO);
				if (this.follower.Brain.Stats.Adoration >= this.follower.Brain.Stats.MAX_ADORATION)
				{
					StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType));
				}
				else
				{
					Close();
				}
			}
			else
			{
				this.follower.Brain.CompleteCurrentTask();
			}
		});
	}

	private IEnumerator GiveItemRoutine(InventoryItem.ITEM_TYPE itemToGive)
	{
		if (follower.Brain.Info.Necklace != 0 && DataManager.AllNecklaces.Contains(itemToGive))
		{
			eventListener.PlayFollowerVO(negativeAcknowledgeVO);
			CloseAndSpeak("AlreadyHaveNecklace");
			yield break;
		}
		eventListener.PlayFollowerVO(positiveAcknowledgeVO);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 3f);
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		yield return new WaitForSeconds(1f);
		DataManager.Instance.GivenFollowerGift = true;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		float faithMultiplier = 7f;
		if (itemToGive == InventoryItem.ITEM_TYPE.GIFT_MEDIUM)
		{
			PlayerFarming.Instance.simpleSpineAnimator.Animate("give-item/gift-medium", 0, false);
			faithMultiplier = 10f;
			JudgementMeter.ShowModify(1);
		}
		else if (itemToGive == InventoryItem.ITEM_TYPE.GIFT_SMALL)
		{
			PlayerFarming.Instance.simpleSpineAnimator.Animate("give-item/gift-small", 0, false);
			faithMultiplier = 5f;
		}
		else
		{
			PlayerFarming.Instance.simpleSpineAnimator.Animate("give-item/generic", 0, false);
		}
		CultFaithManager.AddThought(Thought.Cult_GaveFollowerItem, -1, faithMultiplier, InventoryItem.LocalizedName(itemToGive));
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 3f);
		switch (itemToGive)
		{
		case InventoryItem.ITEM_TYPE.Necklace_1:
		case InventoryItem.ITEM_TYPE.Necklace_2:
		case InventoryItem.ITEM_TYPE.Necklace_3:
		case InventoryItem.ITEM_TYPE.Necklace_4:
		case InventoryItem.ITEM_TYPE.Necklace_5:
		case InventoryItem.ITEM_TYPE.Necklace_Loyalty:
		case InventoryItem.ITEM_TYPE.Necklace_Demonic:
		case InventoryItem.ITEM_TYPE.Necklace_Dark:
		case InventoryItem.ITEM_TYPE.Necklace_Light:
		case InventoryItem.ITEM_TYPE.Necklace_Missionary:
		case InventoryItem.ITEM_TYPE.Necklace_Gold_Skull:
			follower.Brain.GetWillLevelUp(FollowerBrain.AdorationActions.Necklace);
			break;
		case InventoryItem.ITEM_TYPE.GIFT_SMALL:
			follower.Brain.GetWillLevelUp(FollowerBrain.AdorationActions.Gift);
			break;
		case InventoryItem.ITEM_TYPE.GIFT_MEDIUM:
			follower.Brain.GetWillLevelUp(FollowerBrain.AdorationActions.BigGift);
			break;
		}
		int Waiting = 0;
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
		ResourceCustomTarget.Create(follower.gameObject, PlayerFarming.Instance.CameraBone.transform.position, itemToGive, delegate
		{
			switch (itemToGive)
			{
			case InventoryItem.ITEM_TYPE.Necklace_1:
			case InventoryItem.ITEM_TYPE.Necklace_2:
			case InventoryItem.ITEM_TYPE.Necklace_3:
			case InventoryItem.ITEM_TYPE.Necklace_4:
			case InventoryItem.ITEM_TYPE.Necklace_5:
			case InventoryItem.ITEM_TYPE.Necklace_Loyalty:
			case InventoryItem.ITEM_TYPE.Necklace_Demonic:
			case InventoryItem.ITEM_TYPE.Necklace_Dark:
			case InventoryItem.ITEM_TYPE.Necklace_Light:
			case InventoryItem.ITEM_TYPE.Necklace_Missionary:
			case InventoryItem.ITEM_TYPE.Necklace_Gold_Skull:
			{
				AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
				follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Necklace, delegate
				{
					int num5 = Waiting + 1;
					Waiting = num5;
				});
				Action<Follower, InventoryItem.ITEM_TYPE, Action> action = InventoryItem.GiveToFollowerCallbacks(itemToGive);
				if (action != null)
				{
					action(follower, itemToGive, delegate
					{
						int num4 = Waiting + 1;
						Waiting = num4;
					});
				}
				break;
			}
			case InventoryItem.ITEM_TYPE.GIFT_SMALL:
				follower.TimedAnimation(string.Format("Gifts/gift-small-{0}", UnityEngine.Random.Range(1, 4)), 3.6666667f, delegate
				{
					AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
					follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Gift, delegate
					{
						int num3 = Waiting + 1;
						Waiting = num3;
					});
					Action<Follower, InventoryItem.ITEM_TYPE, Action> action2 = InventoryItem.GiveToFollowerCallbacks(itemToGive);
					if (action2 != null)
					{
						action2(follower, itemToGive, delegate
						{
							int num2 = Waiting + 1;
							Waiting = num2;
						});
					}
				});
				break;
			case InventoryItem.ITEM_TYPE.GIFT_MEDIUM:
				follower.TimedAnimation(string.Format("Gifts/gift-medium-{0}", UnityEngine.Random.Range(1, 4)), 3.6666667f, delegate
				{
					AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
					follower.Brain.AddAdoration(FollowerBrain.AdorationActions.BigGift, delegate
					{
						int num7 = Waiting + 1;
						Waiting = num7;
					});
					Action<Follower, InventoryItem.ITEM_TYPE, Action> action3 = InventoryItem.GiveToFollowerCallbacks(itemToGive);
					if (action3 != null)
					{
						action3(follower, itemToGive, delegate
						{
							int num6 = Waiting + 1;
							Waiting = num6;
						});
					}
				});
				break;
			default:
			{
				int num = Waiting + 1;
				Waiting = num;
				break;
			}
			}
			Inventory.ChangeItemQuantity((int)itemToGive, -1);
		}, false);
		while (Waiting < 2)
		{
			yield return null;
		}
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.GiveGift);
		if (follower.Brain.Stats.Adoration >= follower.Brain.Stats.MAX_ADORATION)
		{
			StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType, Close));
		}
		else
		{
			Close();
		}
	}

	private IEnumerator BribeRoutine()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("give-item/generic", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		follower.AddThought(Thought.Bribed);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num > 2)
			{
				break;
			}
			if (i < 2)
			{
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
				ResourceCustomTarget.Create(follower.gameObject, PlayerFarming.Instance.CameraBone.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null, false);
				yield return new WaitForSeconds(0.3f);
				continue;
			}
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
			ResourceCustomTarget.Create(follower.gameObject, PlayerFarming.Instance.CameraBone.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, delegate
			{
				follower.SetBodyAnimation("Reactions/react-love2", false);
				follower.AddBodyAnimation("idle", true, 0f);
				AudioManager.Instance.PlayOneShot("event:/followers/gain_loyalty", base.gameObject.transform.position);
				follower.Brain.Stats.Bribed = true;
				Inventory.ChangeItemQuantity(20, -3);
				follower.Brain.AddAdoration(FollowerBrain.AdorationActions.Bribe, delegate
				{
					if (follower.Brain.Stats.Adoration >= follower.Brain.Stats.MAX_ADORATION)
					{
						Debug.Log("previousTaskType: " + previousTaskType);
						StartCoroutine(GiveDiscipleRewardRoutine(previousTaskType, Close));
					}
					else
					{
						Close();
					}
				});
			}, false);
		}
	}

	private List<ConversationEntry> GetConversationEntry(Follower.ComplaintType ComplaintForBark, ObjectivesData objective)
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		if (ComplaintForBark == Follower.ComplaintType.GiveOnboarding)
		{
			string termToSpeak = DataManager.Instance.CurrentOnboardingFollowerTerm;
			if (DataManager.Instance.CurrentOnboardingFollowerID == -1)
			{
				termToSpeak = LocalizationManager.GetTranslation("FollowerInteractions/AbortGivingQuest");
			}
			list = new List<ConversationEntry>
			{
				new ConversationEntry(base.gameObject, termToSpeak)
			};
			string animation = "worship";
			string currentOnboardingFollowerTerm = DataManager.Instance.CurrentOnboardingFollowerTerm;
			if (!(currentOnboardingFollowerTerm == "Conversation_NPC/FollowerOnboarding/SickFollower"))
			{
				if (currentOnboardingFollowerTerm == "Conversation_NPC/FollowerOnboarding/CureDissenter")
				{
					animation = "Worship/worship-dissenter";
					follower.SetBodyAnimation("Worship/worship-dissenter", true);
				}
			}
			else
			{
				animation = "Emotions/emotion-sick";
			}
			list[0].pitchValue = follower.Brain._directInfoAccess.follower_pitch;
			list[0].vibratoValue = follower.Brain._directInfoAccess.follower_vibrato;
			list[0].soundPath = generalTalkVO;
			list[0].SkeletonData = follower.Spine;
			list[0].CharacterName = "<color=yellow>" + follower.Brain.Info.Name + "</color>";
			list[0].Animation = animation;
			list[0].SetZoom = true;
			list[0].Zoom = 4f;
			int num = 0;
			if (list[0].TermToSpeak[list[0].TermToSpeak.Length - 1] == '0')
			{
				while (true)
				{
					string termToSpeak2 = ConversationEntry.Clone(list[0]).TermToSpeak;
					int num2 = ++num;
					if (LocalizationManager.GetTermData(termToSpeak2.Replace("0", num2.ToString())) == null)
					{
						break;
					}
					ConversationEntry conversationEntry = ConversationEntry.Clone(list[0]);
					conversationEntry.TermToSpeak = conversationEntry.TermToSpeak.Replace("0", num.ToString());
					list.Add(conversationEntry);
				}
			}
			return list;
		}
		if (ComplaintForBark == Follower.ComplaintType.GiveQuest)
		{
			string text = "";
			if (objective == null)
			{
				text = LocalizationManager.GetTranslation("FollowerInteractions/AbortGivingQuest");
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective != null && objective is Objectives_Story)
			{
				Objectives_Story objectives_Story = objective as Objectives_Story;
				text = LocalizationManager.GetTranslation("FollowerInteractions/" + objectives_Story.StoryDataItem.StoryObjectiveData.GiveQuestTerm);
				List<string> list2 = new List<string>();
				if (objectives_Story.StoryDataItem.TargetFollowerID_1 != -1 && objectives_Story.StoryDataItem.TargetFollowerID_1 != objectives_Story.StoryDataItem.QuestGiverFollowerID)
				{
					list2.Add(FollowerInfo.GetInfoByID(objectives_Story.StoryDataItem.TargetFollowerID_1, true).Name);
				}
				if (objectives_Story.StoryDataItem.TargetFollowerID_2 != -1)
				{
					list2.Add(FollowerInfo.GetInfoByID(objectives_Story.StoryDataItem.TargetFollowerID_2, true).Name);
				}
				if (objectives_Story.StoryDataItem.DeadFollowerID != -1 && FollowerInfo.GetInfoByID(objectives_Story.StoryDataItem.DeadFollowerID, true) != null)
				{
					list2.Add(FollowerInfo.GetInfoByID(objectives_Story.StoryDataItem.DeadFollowerID, true).Name);
				}
				string[] array = list2.ToArray();
				string format = text;
				object[] args = array;
				text = string.Format(format, args);
				List<ConversationEntry> list3 = new List<ConversationEntry>();
				list3.Add(new ConversationEntry(base.gameObject, text));
				int num3 = 0;
				while (LocalizationManager.GetTermData("FollowerInteractions/" + objectives_Story.StoryDataItem.StoryObjectiveData.GiveQuestTerm + string.Format("/{0}", ++num3)) != null)
				{
					list3.Add(new ConversationEntry(base.gameObject, "FollowerInteractions/" + objectives_Story.StoryDataItem.StoryObjectiveData.GiveQuestTerm + string.Format("/{0}", num3)));
				}
				list = list3;
			}
			else if (objective.Type == Objectives.TYPES.PERFORM_RITUAL)
			{
				Objectives_PerformRitual objectives_PerformRitual = objective as Objectives_PerformRitual;
				text = LocalizationManager.GetTranslation("FollowerInteractions/GiveQuest/PerformRitual/" + objectives_PerformRitual.Ritual);
				string format2 = text;
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(objectives_PerformRitual.TargetFollowerID_1, true);
				string arg = ((infoByID != null) ? infoByID.Name : null);
				FollowerInfo infoByID2 = FollowerInfo.GetInfoByID(objectives_PerformRitual.TargetFollowerID_2, true);
				text = string.Format(format2, arg, (infoByID2 != null) ? infoByID2.Name : null);
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective is Objectives_Custom && (((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.SendFollowerOnMissionary || ((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.SendFollowerToPrison || ((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.UseFirePit || ((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.UseFeastTable || ((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.MurderFollower || ((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.MurderFollowerAtNight))
			{
				Objectives_Custom objectives_Custom = objective as Objectives_Custom;
				text = LocalizationManager.GetTranslation("FollowerInteractions/GiveQuest/" + objectives_Custom.CustomQuestType);
				string format3 = text;
				FollowerInfo infoByID3 = FollowerInfo.GetInfoByID(objectives_Custom.TargetFollowerID);
				text = string.Format(format3, (infoByID3 != null) ? infoByID3.Name : null);
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective is Objectives_RecruitCursedFollower)
			{
				Objectives_RecruitCursedFollower objectives_RecruitCursedFollower = objective as Objectives_RecruitCursedFollower;
				text = LocalizationManager.GetTranslation("FollowerInteractions/GiveQuest/RecruitCursedFollower/" + objectives_RecruitCursedFollower.CursedState);
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective.Type == Objectives.TYPES.FIND_FOLLOWER)
			{
				Objectives_FindFollower objectives_FindFollower = objective as Objectives_FindFollower;
				text = LocalizationManager.GetTranslation(string.Format("FollowerInteractions/GiveQuest/FindFollower/Variant_{0}", objectives_FindFollower.ObjectiveVariant));
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective.Type == Objectives.TYPES.COLLECT_ITEM)
			{
				Objectives_CollectItem objectives_CollectItem = objective as Objectives_CollectItem;
				text = LocalizationManager.GetTranslation("FollowerInteractions/GiveQuest/CollectItem/" + objectives_CollectItem.ItemType);
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective.Type == Objectives.TYPES.COOK_MEALS)
			{
				Objectives_CookMeal objectives_CookMeal = objective as Objectives_CookMeal;
				text = LocalizationManager.GetTranslation("FollowerInteractions/GiveQuest/CookMeal/" + objectives_CookMeal.MealType);
				text = string.Format(text, CookingData.GetLocalizedName(objectives_CookMeal.MealType));
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective.Type == Objectives.TYPES.PLACE_STRUCTURES)
			{
				Objectives_PlaceStructure objectives_PlaceStructure = objective as Objectives_PlaceStructure;
				text = LocalizationManager.GetTranslation("FollowerInteractions/GiveQuest/PlaceStructure/" + objectives_PlaceStructure.category);
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective.Type == Objectives.TYPES.BUILD_STRUCTURE)
			{
				Objectives_BuildStructure objectives_BuildStructure = objective as Objectives_BuildStructure;
				text = LocalizationManager.GetTranslation("FollowerInteractions/GiveQuest/BuildStructure/" + objectives_BuildStructure.StructureType);
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else if (objective.Type == Objectives.TYPES.EAT_MEAL)
			{
				Objectives_EatMeal objectives_EatMeal = objective as Objectives_EatMeal;
				text = LocalizationManager.GetTranslation("FollowerInteractions/GiveQuest/EatMeal/" + objectives_EatMeal.MealType);
				list = new List<ConversationEntry>
				{
					new ConversationEntry(base.gameObject, text)
				};
			}
			else
			{
				int num4 = -1;
				while (LocalizationManager.GetTermData(string.Concat("FollowerInteractions/", ComplaintForBark, "_", ++num4)) != null)
				{
				}
				list = GetConversationEntry(string.Concat(ComplaintForBark, "_", UnityEngine.Random.Range(0, num4)));
			}
			{
				foreach (ConversationEntry item in list)
				{
					item.pitchValue = follower.Brain._directInfoAccess.follower_pitch;
					item.vibratoValue = follower.Brain._directInfoAccess.follower_vibrato;
					item.CharacterName = "<color=yellow>" + follower.Brain.Info.Name + "</color>";
					item.Animation = "talk-nice1";
					item.SetZoom = true;
					item.Zoom = 4f;
					item.soundPath = generalTalkVO;
				}
				return list;
			}
		}
		if (ComplaintForBark == Follower.ComplaintType.CompletedQuest && !string.IsNullOrEmpty(objective.CompleteTerm))
		{
			list = GetConversationEntry(objective.CompleteTerm);
		}
		else
		{
			int num5 = -1;
			while (LocalizationManager.GetTermData(string.Concat("FollowerInteractions/", ComplaintForBark, "_", ++num5)) != null)
			{
			}
			list = GetConversationEntry(string.Concat(ComplaintForBark, "_", UnityEngine.Random.Range(0, num5)));
		}
		foreach (ConversationEntry item2 in list)
		{
			item2.Speaker = follower.gameObject;
			item2.soundPath = generalTalkVO;
			item2.SkeletonData = follower.Spine;
		}
		switch (ComplaintForBark)
		{
		case Follower.ComplaintType.Hunger:
			follower.SetBodyAnimation("Worship/worship-hungry", true);
			break;
		case Follower.ComplaintType.Homeless:
		case Follower.ComplaintType.NeedBetterHouse:
			follower.SetBodyAnimation("Worship/worship-unhappy", true);
			break;
		default:
			if (follower.Brain.Stats.Happiness >= 0.7f)
			{
				follower.SetBodyAnimation("Worship/worship-happy", true);
			}
			else
			{
				follower.SetBodyAnimation("Worship/worship", true);
			}
			break;
		}
		return list;
	}

	private List<ConversationEntry> GetConversationEntry(string Entry)
	{
		List<ConversationEntry> list = new List<ConversationEntry>
		{
			new ConversationEntry(base.gameObject, "FollowerInteractions/" + Entry, "worship")
		};
		list[0].CharacterName = "<color=yellow>" + follower.Brain.Info.Name + "</color>";
		list[0].Animation = "worship";
		list[0].pitchValue = follower.Brain._directInfoAccess.follower_pitch;
		list[0].vibratoValue = follower.Brain._directInfoAccess.follower_vibrato;
		list[0].SetZoom = true;
		list[0].Zoom = 4f;
		foreach (ConversationEntry item in list)
		{
			item.Speaker = follower.gameObject;
			item.soundPath = generalTalkVO;
			item.SkeletonData = follower.Spine;
			item.soundPath = generalTalkVO;
			item.pitchValue = follower.Brain._directInfoAccess.follower_pitch;
			item.vibratoValue = follower.Brain._directInfoAccess.follower_vibrato;
		}
		return list;
	}

	private void CloseAndSpeak(string ConversationEntryTerm, Action callback = null, bool PlayBow = true)
	{
		Close(false, false, false);
		follower.FacePosition(PlayerFarming.Instance.transform.position);
		List<ConversationEntry> conversationEntry = GetConversationEntry(ConversationEntryTerm);
		foreach (ConversationEntry item in conversationEntry)
		{
			item.soundPath = (PlayVO ? item.soundPath : "");
		}
		MMConversation.Play(new ConversationObject(conversationEntry, null, delegate
		{
			UnPause();
			eventListener.PlayFollowerVO(generalAcknowledgeVO);
			if (PlayBow)
			{
				eventListener.PlayFollowerVO(bowVO);
				follower.TimedAnimation("Reactions/react-bow", 1.8666667f, delegate
				{
					follower.Brain.CompleteCurrentTask();
					Action action = callback;
					if (action != null)
					{
						action();
					}
				});
			}
			else
			{
				StartCoroutine(WaitFrameToClose(callback));
			}
		}));
		MMConversation.PlayVO = PlayVO;
		follower.SetBodyAnimation("worship-talk", true);
	}

	private IEnumerator WaitFrameToClose(Action callback = null)
	{
		yield return null;
		follower.Brain.CompleteCurrentTask();
		if (callback != null)
		{
			callback();
		}
	}

	private void ConvertToWorker()
	{
		if (follower.Brain.Info.FollowerRole != FollowerRole.Worker)
		{
			follower.Brain.Info.FollowerRole = FollowerRole.Worker;
			follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
			follower.SetOutfit(FollowerOutfitType.Follower, false);
		}
	}

	private void Close()
	{
		Close(true);
	}

	private void Close(bool DoResetFollower, bool unpause = true, bool reshowMenu = true)
	{
		if (unpause)
		{
			UnPause();
		}
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		if (state != null)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
		Activated = false;
		if (DoResetFollower)
		{
			ResetFollower();
		}
		BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
		if (MMConversation.CURRENT_CONVERSATION == null)
		{
			GameManager.GetInstance().OnConversationEnd();
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		}
		AutomaticallyInteract = false;
		GameManager.GetInstance().CamFollowTarget.SnappyMovement = false;
		if (follower.Brain._directInfoAccess.StartingCursedState == Thought.Ill)
		{
			follower.Brain.MakeSick();
		}
		else if (follower.Brain._directInfoAccess.StartingCursedState == Thought.BecomeStarving)
		{
			follower.Brain.MakeStarve();
		}
		else if (follower.Brain._directInfoAccess.StartingCursedState == Thought.Dissenter)
		{
			follower.Brain.MakeDissenter();
		}
		else if (follower.Brain._directInfoAccess.StartingCursedState == Thought.OldAge)
		{
			follower.Brain.ApplyCurseState(Thought.OldAge);
			follower.Brain.Info.Age = follower.Brain.Info.LifeExpectancy;
		}
		follower.Brain._directInfoAccess.StartingCursedState = Thought.None;
		if (GiveDoctrinePieceOnClose && DoctrineUpgradeSystem.TrySermonsStillAvailable() && DoctrineUpgradeSystem.TryGetStillDoctrineStone() && DataManager.Instance.GetVariable(DataManager.Variables.FirstDoctrineStone))
		{
			Debug.Log("CALL BACK 2!");
			PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.DOCTRINE_STONE, 1, base.transform.position);
			if (pickUp != null)
			{
				Interaction_DoctrineStone component = pickUp.GetComponent<Interaction_DoctrineStone>();
				if (component != null)
				{
					component.MagnetToPlayer();
					component.AutomaticallyInteract = true;
				}
			}
		}
		if (ShowDivineInspirationTutorialOnClose && DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.DivineInspiration))
		{
			ShowDivineInspirationTutorialOnClose = false;
			MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.DivineInspiration);
		}
		follower.WorshipperBubble.StopAllCoroutines();
		follower.WorshipperBubble.Close();
		ShowOtherFollowers();
		base.HasChanged = true;
		if (reshowMenu)
		{
			follower.AdorationUI.Hide();
			UIFollowerName componentInChildren = follower.GetComponentInChildren<UIFollowerName>();
			if ((object)componentInChildren != null)
			{
				componentInChildren.Hide();
			}
			OnInteract(state);
		}
		else
		{
			follower.AdorationUI.Show();
		}
	}

	private void UnPause()
	{
		SimulationManager.UnPause();
	}

	private void HideOtherFollowers()
	{
	}

	private void ShowOtherFollowers()
	{
	}
}
