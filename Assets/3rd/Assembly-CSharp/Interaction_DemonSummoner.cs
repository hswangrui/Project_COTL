using System;
using System.Collections;
using I2.Loc;
using Lamb.UI;
using Spine.Unity;
using src.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_DemonSummoner : Interaction
{
	public Canvas UICanvas;

	public Image UIProgress;

	[SerializeField]
	private SkeletonAnimation[] demons;

	private Structures_Demon_Summoner _StructureInfo;

	private FollowerBrain follower;

	public GameObject OnEffects;

	public static string[] DemonSkins = new string[8] { "Projectile", "Chomp", "Arrows", "Heart", "Explode", "Spirit", "Baal", "Aym" };

	public Structure Structure { get; private set; }

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Demon_Summoner structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Demon_Summoner;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)Structure)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
			Structure structure2 = Structure;
			structure2.OnBrainRemoved = (Action)Delegate.Remove(structure2.OnBrainRemoved, new Action(OnBrainRemoved));
		}
	}

	private void OnBrainAssigned()
	{
		SkeletonAnimation[] array = demons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(false);
		}
		for (int num = StructureInfo.MultipleFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (StructureInfo.MultipleFollowerIDs[num] != -1 && DataManager.Instance.Followers_Demons_IDs.Contains(StructureInfo.MultipleFollowerIDs[num]))
			{
				OnEffects.SetActive(true);
				demons[num].gameObject.SetActive(true);
				for (int j = 0; j < DataManager.Instance.Followers_Demons_IDs.Count; j++)
				{
					if (DataManager.Instance.Followers_Demons_IDs[j] == StructureInfo.MultipleFollowerIDs[num])
					{
						FollowerInfo infoByID = FollowerInfo.GetInfoByID(DataManager.Instance.Followers_Demons_IDs[j]);
						string text = DemonSkins[DataManager.Instance.Followers_Demons_Types[j]];
						int demonLevel = infoByID.GetDemonLevel();
						text += ((demonLevel > 1 && DataManager.Instance.Followers_Demons_Types[j] < 6) ? "+" : "");
						demons[num].Skeleton.SetSkin(text);
						break;
					}
				}
			}
		}
		DemonPreserved(false);
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if (StructureInfo == null)
		{
			return;
		}
		for (int num = StructureInfo.MultipleFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.Followers_Transitioning_IDs.Contains(StructureInfo.MultipleFollowerIDs[num]))
			{
				Follower follower = FollowerManager.FindFollowerByID(StructureInfo.MultipleFollowerIDs[num]);
				int demonType = DemonModel.GetDemonType(follower.Brain._directInfoAccess);
				demons[num].gameObject.SetActive(true);
				string text = DemonSkins[demonType];
				text += ((follower.Brain.Stats.HasLevelledUp && demonType < 6) ? "+" : "");
				demons[num].Skeleton.SetSkin(text);
				demons[num].AnimationState.SetAnimation(0, "reveal", false);
				demons[num].AnimationState.AddAnimation(0, "idle", true, 0f);
				follower.Brain.CompleteCurrentTask();
				follower.Brain.HardSwapToTask(new FollowerTask_IsDemon());
				follower.Brain.CurrentTask.Arrive();
				NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.DemonConverted, follower.Brain.Info, NotificationFollower.Animation.Normal);
				DataManager.Instance.Followers_Demons_IDs.Add(follower.Brain.Info.ID);
				DataManager.Instance.Followers_Demons_Types.Add(demonType);
				DataManager.Instance.Followers_Transitioning_IDs.Remove(follower.Brain.Info.ID);
			}
		}
	}

	private void OnBrainRemoved()
	{
		DataManager.Instance.Followers_Demons_IDs.Clear();
		DataManager.Instance.Followers_Demons_Types.Clear();
		DemonPreserved(true);
	}

	private void DemonPreserved(bool structureRemoved)
	{
		for (int num = StructureInfo.MultipleFollowerIDs.Count - 1; num >= 0; num--)
		{
			if (StructureInfo.MultipleFollowerIDs[num] != -1 && !DataManager.Instance.Followers_Demons_IDs.Contains(StructureInfo.MultipleFollowerIDs[num]) && !DataManager.Instance.Followers_Transitioning_IDs.Contains(StructureInfo.MultipleFollowerIDs[num]))
			{
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(StructureInfo.MultipleFollowerIDs[num]);
				if (infoByID != null)
				{
					FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(infoByID);
					GameManager.GetInstance().StartCoroutine(SetFollowerPosition(orCreateBrain));
					NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.DemonPreserved, orCreateBrain.Info, NotificationFollower.Animation.Normal);
					if (!structureRemoved)
					{
						orCreateBrain.MakeExhausted();
						orCreateBrain._directInfoAccess.MissionaryExhaustion += 3f;
					}
				}
				StructureInfo.MultipleFollowerIDs.RemoveAt(num);
			}
		}
	}

	private IEnumerator SetFollowerPosition(FollowerBrain brain)
	{
		Vector3 pos = base.transform.position;
		while (Time.timeScale == 0f)
		{
			yield return null;
		}
		brain.HardSwapToTask(new FollowerTask_Idle());
		brain.CurrentTask.Arrive();
		while (FollowerManager.FindFollowerByID(brain.Info.ID) == null)
		{
			yield return null;
		}
		Follower follower = FollowerManager.FindFollowerByID(brain.Info.ID);
		if ((bool)follower)
		{
			brain.HardSwapToTask(new FollowerTask_ManualControl());
			follower.transform.position = pos;
			brain.CompleteCurrentTask();
		}
	}

	public override void OnEnableInteraction()
	{
		Structure = GetComponentInParent<Structure>();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structure structure2 = Structure;
		structure2.OnBrainRemoved = (Action)Delegate.Combine(structure2.OnBrainRemoved, new Action(OnBrainRemoved));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
		base.OnEnableInteraction();
	}

	public override void GetLabel()
	{
		if (!AtDemonLimit())
		{
			base.Label = ScriptLocalization.Interactions.SummonDemon;
		}
		else
		{
			base.Label = "";
		}
	}

	public override void GetSecondaryLabel()
	{
		if (StructureInfo.MultipleFollowerIDs.Count > 0)
		{
			base.SecondaryLabel = ScriptLocalization.UI_Settings_Controls.Interact;
		}
		else
		{
			base.SecondaryLabel = "";
		}
	}

	public override void OnBecomeCurrent()
	{
		Interactable = !AtDemonLimit();
		HasSecondaryInteraction = StructureInfo.MultipleFollowerIDs.Count > 0;
		base.OnBecomeCurrent();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (!AtDemonLimit())
		{
			GameManager.GetInstance().OnConversationNew();
			Time.timeScale = 0f;
			HUD_Manager.Instance.Hide(false, 0);
			UIDemonSummonMenuController demonSummonMenuInstance = MonoSingleton<UIManager>.Instance.DemonSummonTemplate.Instantiate();
			demonSummonMenuInstance.Show(DemonModel.AvailableFollowersForDemonConversion());
			demonSummonMenuInstance.UpdateDemonCounts(StructureInfo.MultipleFollowerIDs);
			UIDemonSummonMenuController uIDemonSummonMenuController = demonSummonMenuInstance;
			uIDemonSummonMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIDemonSummonMenuController.OnFollowerSelected, new Action<FollowerInfo>(OnFollowerChosenForConversion));
			UIDemonSummonMenuController uIDemonSummonMenuController2 = demonSummonMenuInstance;
			uIDemonSummonMenuController2.OnHidden = (Action)Delegate.Combine(uIDemonSummonMenuController2.OnHidden, (Action)delegate
			{
				demonSummonMenuInstance = null;
				Time.timeScale = 1f;
				HUD_Manager.Instance.Show();
				GameManager.GetInstance().OnConversationEnd();
			});
		}
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		GameManager.GetInstance().OnConversationNew();
		UIDemonMenuController demonMenuInstance = MonoSingleton<UIManager>.Instance.DemonMenuTemplate.Instantiate();
		demonMenuInstance.Show(StructureInfo.MultipleFollowerIDs);
		UIDemonMenuController uIDemonMenuController = demonMenuInstance;
		uIDemonMenuController.OnHidden = (Action)Delegate.Combine(uIDemonMenuController.OnHidden, (Action)delegate
		{
			demonMenuInstance = null;
			Time.timeScale = 1f;
			HUD_Manager.Instance.Show();
			GameManager.GetInstance().OnConversationEnd();
		});
	}

	private bool AtDemonLimit()
	{
		return StructureInfo.MultipleFollowerIDs.Count >= structureBrain.DemonSlots;
	}

	private void OnFollowerChosenForConversion(FollowerInfo followerInfo)
	{
		StructureInfo.MultipleFollowerIDs.Add(followerInfo.ID);
		FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(followerInfo);
		Follower follower = FollowerManager.FindFollowerByID(followerInfo.ID);
		orCreateBrain.HardSwapToTask(new FollowerTask_ManualControl());
		follower.transform.position = base.transform.position;
		DataManager.Instance.Followers_Transitioning_IDs.Add(followerInfo.ID);
		StartCoroutine(SentFollowerIE(follower));
	}

	private IEnumerator SentFollowerIE(Follower follower)
	{
		follower.HideAllFollowerIcons();
		int demonType = DemonModel.GetDemonType(follower.Brain._directInfoAccess);
		yield return new WaitForSeconds(0.5f);
		follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		OnEffects.SetActive(true);
		follower.SetBodyAnimation("summon-demon", false);
		AudioManager.Instance.PlayOneShot("event:/followers/follower_to_demon_sequence", follower.transform.position);
		yield return new WaitForSeconds(3f);
		int num = 0;
		for (int i = 0; i < StructureInfo.MultipleFollowerIDs.Count; i++)
		{
			if (StructureInfo.MultipleFollowerIDs[i] == follower.Brain.Info.ID)
			{
				num = i;
				break;
			}
		}
		demons[num].gameObject.SetActive(true);
		string text = DemonSkins[demonType];
		text += ((follower.Brain.Info.XPLevel > 1 && demonType < 6) ? "+" : "");
		demons[num].Skeleton.SetSkin(text);
		demons[num].AnimationState.SetAnimation(0, "reveal", false);
		demons[num].AnimationState.AddAnimation(0, "idle", true, 0f);
		follower.Brain.CompleteCurrentTask();
		follower.Brain.HardSwapToTask(new FollowerTask_IsDemon());
		follower.Brain.CurrentTask.Arrive();
		NotificationCentre.Instance.PlayFollowerNotification(NotificationCentre.NotificationType.DemonConverted, follower.Brain.Info, NotificationFollower.Animation.Normal);
		DataManager.Instance.Followers_Demons_IDs.Add(follower.Brain.Info.ID);
		DataManager.Instance.Followers_Demons_Types.Add(demonType);
		DataManager.Instance.Followers_Transitioning_IDs.Remove(follower.Brain.Info.ID);
		yield return new WaitForSeconds(0.8f);
	}
}
