using System;
using System.Collections;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.PrisonerMenu;
using src.Extensions;
using src.UI.Menus;
using UnityEngine;

public class Interaction_Prison : Interaction
{
	public Structure Structure;

	private Structures_Prison _StructureInfo;

	[SerializeField]
	private GameObject playerPosition;

	private EventInstance LoopInstance;

	private float count;

	private UIPrisonerMenuController prisonerMenu;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Prison structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Prison;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public int PrisonerID
	{
		get
		{
			return StructureInfo.FollowerID;
		}
	}

	private void Awake()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void Start()
	{
		ActivateDistance = 2f;
		UpdateLocalisation();
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (!DataManager.Instance.Followers_Imprisoned_IDs.Contains(StructureInfo.FollowerID))
		{
			StructureInfo.FollowerID = -1;
		}
	}

	public override void GetLabel()
	{
		base.Label = ((StructureInfo != null && (FollowerInfo.GetInfoByID(StructureInfo.FollowerID) != null || StructureInfo.FollowerID == -1)) ? ScriptLocalization.Structures.PRISON : "");
	}

	public override void GetSecondaryLabel()
	{
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
	}

	private IEnumerator ReEducate()
	{
		yield return new WaitForSeconds(0.25f);
		count = 0f;
		PlayerFarming.Instance.GoToAndStop(playerPosition, base.gameObject, true);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			if (count > 60f)
			{
				PlayerFarming.Instance.EndGoToAndStop();
			}
			count += 1f;
			yield return null;
		}
		Follower prisoner = FollowerManager.FindFollowerByID(StructureInfo.FollowerID);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().OnConversationNext(base.gameObject, 8f);
		AudioManager.Instance.PlayOneShot("event:/dialogue/followers/general_acknowledge", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/sermon/book_pickup", base.gameObject);
		PlayerFarming.Instance.Spine.state.SetAnimation(0, "build", true);
		yield return new WaitForSeconds(0.3f);
		AudioManager.Instance.PlayOneShot("event:/sermon/sermon_speech_bubble", base.gameObject);
		LoopInstance = AudioManager.Instance.CreateLoop("event:/sermon/preach_loop", base.gameObject);
		prisoner.Spine.state.SetAnimation(0, "Prison/stocks-reeducate", false);
		prisoner.SetBodyAnimation("Prison/stocks-reeducate", false);
		prisoner.AddBodyAnimation("Prison/stocks", false, 0f);
		yield return new WaitForSeconds(2f);
		prisoner.SetFaceAnimation("Reactions/react-brainwashed", false);
		AudioManager.Instance.StopLoop(LoopInstance);
		CameraManager.shakeCamera(1f, UnityEngine.Random.Range(0, 360));
		AudioManager.Instance.PlayOneShot("event:/sermon/end_sermon", base.gameObject);
		prisoner.Spine.state.AddAnimation(0, "Prison/stocks", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/dialogue/followers/positive_acknowledge", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
		BiomeConstants.Instance.EmitHeartPickUpVFX(prisoner.gameObject.transform.position, 0f, "black", "burst_big");
		structureBrain.Reeducate(prisoner.Brain);
		prisoner.Brain.Stats.ReeducatedAction = true;
		ReeducatedThoughts(prisoner);
		ReeducatedThoughts(prisoner);
		yield return new WaitForSeconds(2f);
		prisoner.SetFaceAnimation("Reactions/react-loved", false);
		GameManager.GetInstance().OnConversationEnd();
		AudioManager.Instance.PlayOneShot("event:/sermon/book_put_down", base.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		GameManager.GetInstance().OnConversationNew();
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		if (StructureInfo.FollowerID != -1)
		{
			prisonerMenu = MonoSingleton<UIManager>.Instance.PrisonerMenuTemplate.Instantiate();
			prisonerMenu.Show(FollowerInfo.GetInfoByID(StructureInfo.FollowerID), StructureInfo);
			UIPrisonerMenuController uIPrisonerMenuController = prisonerMenu;
			uIPrisonerMenuController.OnFollowerReleased = (Action<FollowerInfo>)Delegate.Combine(uIPrisonerMenuController.OnFollowerReleased, (Action<FollowerInfo>)delegate
			{
				ReleaseFollower();
			});
			UIPrisonerMenuController uIPrisonerMenuController2 = prisonerMenu;
			uIPrisonerMenuController2.OnReEducate = (Action<FollowerInfo>)Delegate.Combine(uIPrisonerMenuController2.OnReEducate, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
			{
				Follower follower = FollowerManager.FindFollowerByID(followerInfo.ID);
				if (follower != null && !follower.Brain.Stats.ReeducatedAction)
				{
					prisonerMenu.Hide();
					StartCoroutine(ReEducate());
				}
			});
			UIPrisonerMenuController uIPrisonerMenuController3 = prisonerMenu;
			uIPrisonerMenuController3.OnHidden = (Action)Delegate.Combine(uIPrisonerMenuController3.OnHidden, (Action)delegate
			{
				Time.timeScale = 1f;
			});
			UIPrisonerMenuController uIPrisonerMenuController4 = prisonerMenu;
			uIPrisonerMenuController4.OnCancel = (Action)Delegate.Combine(uIPrisonerMenuController4.OnCancel, (Action)delegate
			{
				GameManager.GetInstance().OnConversationEnd();
			});
			return;
		}
		UIPrisonMenuController uIPrisonMenuController = MonoSingleton<UIManager>.Instance.PrisonMenuTemplate.Instantiate();
		uIPrisonMenuController.Show(Prison.ImprisonableFollowers(), null, false, UpgradeSystem.Type.Building_Prison);
		uIPrisonMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIPrisonMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			ImprisonFollower(FollowerBrain.GetOrCreateBrain(followerInfo));
			BiomeConstants instance = BiomeConstants.Instance;
			if ((object)instance != null)
			{
				instance.EmitSmokeExplosionVFX(base.gameObject.transform.position);
			}
			AudioManager.Instance.PlayOneShot("event:/dialogue/followers/general_acknowledge", base.gameObject);
		});
		uIPrisonMenuController.OnHidden = (Action)Delegate.Combine(uIPrisonMenuController.OnHidden, (Action)delegate
		{
			Time.timeScale = 1f;
		});
		uIPrisonMenuController.OnCancel = (Action)Delegate.Combine(uIPrisonMenuController.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().OnConversationEnd();
		});
	}

	private void ImprisonFollower(FollowerBrain follower)
	{
		StructureInfo.FollowerID = follower.Info.ID;
		StructureInfo.FollowerImprisonedTimestamp = TimeManager.TotalElapsedGameTime;
		StructureInfo.FollowerImprisonedFaith = follower.Stats.Reeducation;
		StartCoroutine(ImprisonFollowerIE(follower));
	}

	public void ReleaseFollower()
	{
		StartCoroutine(ReleaseFollowerIE());
	}

	private IEnumerator ReleaseFollowerIE()
	{
		yield return new WaitForEndOfFrame();
		if (StructureInfo == null)
		{
			yield break;
		}
		FollowerInfo infoByID = FollowerInfo.GetInfoByID(StructureInfo.FollowerID);
		if (infoByID == null)
		{
			yield break;
		}
		FollowerBrain brain = FollowerBrain.GetOrCreateBrain(infoByID);
		if (brain != null)
		{
			Follower f = FollowerManager.FindFollowerByID(StructureInfo.FollowerID);
			if (!(f == null))
			{
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(f.gameObject, 5f);
				yield return new WaitForSeconds(1f);
				AudioManager.Instance.PlayOneShot("event:/followers/imprison", f.transform.position);
				brain.CompleteCurrentTask();
				DataManager.Instance.Followers_Imprisoned_IDs.Remove(brain.Info.ID);
				StructureInfo.FollowerID = -1;
				yield return new WaitForSeconds(1f);
				GameManager.GetInstance().OnConversationEnd();
			}
		}
	}

	private IEnumerator ImprisonFollowerIE(FollowerBrain follower)
	{
		yield return new WaitForEndOfFrame();
		Follower follower2 = FollowerManager.FindFollowerByID(follower.Info.ID);
		follower.CompleteCurrentTask();
		follower.HardSwapToTask(new FollowerTask_ManualControl());
		follower2.transform.position = GetComponent<Prison>().PrisonerLocation.position;
		ImprisonedThoughts(follower2);
		AudioManager.Instance.PlayOneShot("event:/followers/imprison", follower2.transform.position);
		yield return new WaitForEndOfFrame();
		follower.CompleteCurrentTask();
		follower.HardSwapToTask(new FollowerTask_Imprisoned(StructureInfo.ID));
		if (!DataManager.Instance.Followers_Imprisoned_IDs.Contains(follower.Info.ID))
		{
			DataManager.Instance.Followers_Imprisoned_IDs.Add(follower.Info.ID);
		}
		if (follower.Info.CursedState != Thought.Dissenter)
		{
			bool flag = false;
			foreach (ObjectivesData objective in DataManager.Instance.Objectives)
			{
				if (objective.Type == Objectives.TYPES.CUSTOM && ((Objectives_Custom)objective).CustomQuestType == Objectives.CustomQuestTypes.SendFollowerToPrison && ((Objectives_Custom)objective).TargetFollowerID == follower.Info.ID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
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
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.SendFollowerToPrison, follower.Info.ID);
		GameManager.GetInstance().OnConversationEnd();
	}

	private void ImprisonedThoughts(Follower prisoner)
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.Location != prisoner.Brain.Location || allBrain == prisoner.Brain)
			{
				continue;
			}
			if (allBrain.Stats.Reeducation > 0f)
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

	public void ReeducatedThoughts(Follower prisoner)
	{
		if (prisoner.Brain.Stats.Reeducation > 0f)
		{
			foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
			{
				if (allBrain != prisoner.Brain)
				{
					if (allBrain.HasTrait(FollowerTrait.TraitType.Cynical))
					{
						if (allBrain.CurrentTaskType == FollowerTaskType.Sleep)
						{
							allBrain.AddThought(Thought.DissenterCultMemberReeducatedSleepingCynicalTrait);
						}
						else
						{
							allBrain.AddThought(Thought.DissenterCultMemberReeducatedCynicalTrait);
						}
					}
					else if (allBrain.HasTrait(FollowerTrait.TraitType.Gullible))
					{
						if (allBrain.CurrentTaskType == FollowerTaskType.Sleep)
						{
							allBrain.AddThought(Thought.DissenterCultMemberReeducatedSleepingGullibleTrait);
						}
						else
						{
							allBrain.AddThought(Thought.DissenterCultMemberReeducatedGullibleTrait);
						}
					}
					else if (allBrain.CurrentTaskType == FollowerTaskType.Sleep)
					{
						allBrain.AddThought(Thought.DissenterCultMemberReeducatedSleeping);
					}
					else
					{
						allBrain.AddThought(Thought.DissenterCultMemberReeducated);
					}
				}
			}
			return;
		}
		JudgementMeter.ShowModify(1);
		prisoner.Brain.AddThought(Thought.InnocentReeducated);
		foreach (FollowerBrain allBrain2 in FollowerBrain.AllBrains)
		{
			if (allBrain2 == prisoner.Brain)
			{
				continue;
			}
			if (allBrain2.HasTrait(FollowerTrait.TraitType.Cynical))
			{
				if (allBrain2.CurrentTaskType == FollowerTaskType.Sleep)
				{
					allBrain2.AddThought(Thought.InnocentCultMemberReeducatedCynicalTraitSleeping);
				}
				else
				{
					allBrain2.AddThought(Thought.InnocentCultMemberReeducatedCynicalTrait);
				}
			}
			else if (allBrain2.HasTrait(FollowerTrait.TraitType.Gullible))
			{
				if (allBrain2.CurrentTaskType == FollowerTaskType.Sleep)
				{
					allBrain2.AddThought(Thought.InnocentCultMemberReeducatedGullibleTraitSleeping);
				}
				else
				{
					allBrain2.AddThought(Thought.InnocentCultMemberReeducatedGullibleTrait);
				}
			}
			else if (allBrain2.CurrentTaskType == FollowerTaskType.Sleep)
			{
				allBrain2.AddThought(Thought.InnocentCultMemberReeducatedSleeping);
			}
			else
			{
				allBrain2.AddThought(Thought.InnocentCultMemberReeducated);
			}
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		AudioManager.Instance.StopLoop(LoopInstance);
	}
}
