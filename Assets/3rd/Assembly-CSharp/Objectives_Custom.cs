using System;
using System.Collections;
using I2.Loc;
using UnityEngine;

[Serializable]
public class Objectives_Custom : ObjectivesData
{
	[Serializable]
	public class FinalizedData_Custom : ObjectivesDataFinalized
	{
		public Objectives.CustomQuestTypes CustomQuestType;

		public string TargetFollowerName;

		public override string GetText()
		{
			string translation = LocalizationManager.GetTranslation(string.Format("Objectives/Custom/{0}", CustomQuestType));
			if (CustomQuestType == Objectives.CustomQuestTypes.MysticShopReturn)
			{
				return string.Format(translation, string.IsNullOrEmpty(DataManager.Instance.MysticKeeperName) ? ScriptLocalization.NAMES.MysticShopSellerDefault : DataManager.Instance.MysticKeeperName);
			}
			return string.Format(translation, TargetFollowerName);
		}
	}

	public Objectives.CustomQuestTypes CustomQuestType;

	public int TargetFollowerID = -1;

	public int ResultFollowerID = -1;

	public override string Text
	{
		get
		{
			string text = LocalizationManager.GetTranslation(string.Format("Objectives/Custom/{0}", CustomQuestType));
			if (CustomQuestType == Objectives.CustomQuestTypes.MysticShopReturn)
			{
				text = string.Format(text, string.IsNullOrEmpty(DataManager.Instance.MysticKeeperName) ? ScriptLocalization.NAMES.MysticShopSellerDefault : DataManager.Instance.MysticKeeperName);
			}
			else if (CustomQuestType == Objectives.CustomQuestTypes.BlessAFollower)
			{
				if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Intimidate))
				{
					text = ScriptLocalization.DoctrineUpgradeSystem.WorkWorship_Intimidate;
				}
				else if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Inspire))
				{
					text = ScriptLocalization.DoctrineUpgradeSystem.WorkWorship_Inspire;
				}
			}
			string result;
			if (TargetFollowerID == -1)
			{
				result = text;
			}
			else
			{
				string format = text;
				FollowerInfo infoByID = FollowerInfo.GetInfoByID(TargetFollowerID, true);
				result = string.Format(format, (infoByID != null) ? infoByID.Name : null);
			}
			return result;
		}
	}

	public Objectives_Custom()
	{
	}

	public Objectives_Custom(string groupId, Objectives.CustomQuestTypes customQuestType, int targetFollowerID = -1, float questExpireDuration = -1f)
		: base(groupId, questExpireDuration)
	{
		Type = Objectives.TYPES.CUSTOM;
		CustomQuestType = customQuestType;
		TargetFollowerID = targetFollowerID;
	}

	public override void Init(bool initialAssigning)
	{
		if (!initialised)
		{
			FollowerManager.OnFollowerDie = (FollowerManager.FollowerGoneEvent)Delegate.Combine(FollowerManager.OnFollowerDie, new FollowerManager.FollowerGoneEvent(OnFollowerDied));
		}
		base.Init(initialAssigning);
		GameManager instance = GameManager.GetInstance();
		if ((object)instance != null)
		{
			instance.StartCoroutine(DelayedCompleteCheck());
		}
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		FinalizedData_Custom obj = new FinalizedData_Custom
		{
			GroupId = GroupId,
			Index = Index,
			CustomQuestType = CustomQuestType
		};
		object targetFollowerName;
		if (TargetFollowerID == -1)
		{
			targetFollowerName = "";
		}
		else
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(TargetFollowerID, true);
			targetFollowerName = ((infoByID != null) ? infoByID.Name : null);
		}
		obj.TargetFollowerName = (string)targetFollowerName;
		obj.UniqueGroupID = UniqueGroupID;
		return obj;
	}

	private IEnumerator DelayedCompleteCheck()
	{
		yield return new WaitForSeconds(0.5f);
		if (CustomQuestType == Objectives.CustomQuestTypes.CookFirstMeal && base.Follower != -1)
		{
			if (StructureManager.GetAllStructuresOfType(FollowerLocation.Base, StructureBrain.TYPES.MEAL).Count > 0 || FollowerInfo.GetInfoByID(base.Follower).Satiation > 60f)
			{
				ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CookFirstMeal);
			}
		}
		else if (CustomQuestType == Objectives.CustomQuestTypes.CollectDivineInspiration && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Building_Temple))
		{
			ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CollectDivineInspiration);
		}
	}

	protected override bool CheckComplete()
	{
		if (!IsFailed && TargetFollowerID != -1 && FollowerInfo.GetInfoByID(TargetFollowerID) == null && !FailLocked)
		{
			Failed();
		}
		if (IsFailed)
		{
			return false;
		}
		if (ResultFollowerID == TargetFollowerID)
		{
			return true;
		}
		if (TargetFollowerID != -1)
		{
			return false;
		}
		return base.CheckComplete();
	}

	public override void Complete()
	{
		base.Complete();
		FollowerManager.OnFollowerDie = (FollowerManager.FollowerGoneEvent)Delegate.Remove(FollowerManager.OnFollowerDie, new FollowerManager.FollowerGoneEvent(OnFollowerDied));
	}

	private void OnFollowerDied(int followerID, NotificationCentre.NotificationType notificationType)
	{
		if ((CustomQuestType == Objectives.CustomQuestTypes.MurderFollower || CustomQuestType == Objectives.CustomQuestTypes.MurderFollowerAtNight) && followerID == TargetFollowerID && !IsFailed && !IsComplete && !FailLocked)
		{
			Failed();
			FollowerManager.OnFollowerDie = (FollowerManager.FollowerGoneEvent)Delegate.Remove(FollowerManager.OnFollowerDie, new FollowerManager.FollowerGoneEvent(OnFollowerDied));
		}
	}

	public override void Update()
	{
		base.Update();
		if (IsFailed || IsComplete || TargetFollowerID == -1)
		{
			return;
		}
		if (!TargetFollowerAllowOldAge)
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(TargetFollowerID);
			if (infoByID != null && infoByID.CursedState == Thought.OldAge)
			{
				goto IL_0056;
			}
		}
		if (FollowerInfo.GetInfoByID(TargetFollowerID) != null)
		{
			return;
		}
		goto IL_0056;
		IL_0056:
		Failed();
	}
}
