using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI.FollowerSelect
{
	public class UIFollowerSelectMenuController : UIFollowerSelectBase<FollowerInformationBox>
	{
		[SerializeField]
		private TMP_Text _header;

		private UpgradeSystem.Type _followerSelectionType;

		private List<ObjectivesData> _cachedObjectives;

		public void Show(List<FollowerBrain> followerBrains, List<FollowerBrain> blackList = null, bool instant = false, UpgradeSystem.Type followerSelectionType = UpgradeSystem.Type.Count, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			_followerSelectionType = followerSelectionType;
			Show(followerBrains, blackList, instant, hideOnSelection, cancellable, hasSelection);
		}

		public void Show(List<Follower> followers, List<Follower> blackList = null, bool instant = false, UpgradeSystem.Type followerSelectionType = UpgradeSystem.Type.Count, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			_followerSelectionType = followerSelectionType;
			Show(followers, blackList, instant, hideOnSelection, cancellable, hasSelection);
		}

		public void Show(List<SimFollower> simFollowers, List<SimFollower> blackList = null, bool instant = false, UpgradeSystem.Type followerSelectionType = UpgradeSystem.Type.Count, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			_followerSelectionType = followerSelectionType;
			Show(simFollowers, blackList, instant, hideOnSelection, cancellable, hasSelection);
		}

		public void Show(List<FollowerInfo> followerInfo, List<FollowerInfo> blackList = null, bool instant = false, UpgradeSystem.Type followerSelectionType = UpgradeSystem.Type.Count, bool hideOnSelection = true, bool cancellable = true, bool hasSelection = true)
		{
			_followerSelectionType = followerSelectionType;
			Show(followerInfo, blackList, instant, hideOnSelection, cancellable, hasSelection);
		}

		protected override void OnShowStarted()
		{
			if (_cachedObjectives != null)
			{
				_cachedObjectives.Clear();
			}
			if (!_hasSelection)
			{
				_header.text = ScriptLocalization.Inventory.FOLLOWERS;
			}
			base.OnShowStarted();
		}

		protected override void OnShowCompleted()
		{
			base.OnShowCompleted();
			foreach (FollowerInformationBox followerInfoBox in _followerInfoBoxes)
			{
				if (DoesFollowerHaveObjective(followerInfoBox.FollowerInfo))
				{
					followerInfoBox.ShowObjective();
				}
			}
		}

		private bool DoesFollowerHaveObjective(FollowerInfo followerInfo)
		{
			if (_cachedObjectives == null)
			{
				_cachedObjectives = new List<ObjectivesData>();
				foreach (ObjectivesData objective in DataManager.Instance.Objectives)
				{
					Objectives_PerformRitual objectives_PerformRitual;
					if ((objectives_PerformRitual = objective as Objectives_PerformRitual) != null && objectives_PerformRitual.Ritual == _followerSelectionType)
					{
						_cachedObjectives.Add(objective);
					}
					else
					{
						Objectives_Custom objectives_Custom;
						if ((objectives_Custom = objective as Objectives_Custom) == null)
						{
							continue;
						}
						switch (_followerSelectionType)
						{
						case UpgradeSystem.Type.Building_Prison:
							if (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.SendFollowerToPrison)
							{
								_cachedObjectives.Add(objective);
							}
							break;
						case UpgradeSystem.Type.Building_Missionary:
							if (objectives_Custom.CustomQuestType == Objectives.CustomQuestTypes.SendFollowerOnMissionary)
							{
								_cachedObjectives.Add(objective);
							}
							break;
						}
					}
				}
			}
			foreach (ObjectivesData cachedObjective in _cachedObjectives)
			{
				Objectives_PerformRitual objectives_PerformRitual2;
				if ((objectives_PerformRitual2 = cachedObjective as Objectives_PerformRitual) != null && (objectives_PerformRitual2.TargetFollowerID_1 == followerInfo.ID || objectives_PerformRitual2.TargetFollowerID_2 == followerInfo.ID))
				{
					return true;
				}
				Objectives_Custom objectives_Custom2;
				if ((objectives_Custom2 = cachedObjective as Objectives_Custom) != null && objectives_Custom2.TargetFollowerID == followerInfo.ID)
				{
					return true;
				}
			}
			return false;
		}

		protected override FollowerInformationBox PrefabTemplate()
		{
			return MonoSingleton<UIManager>.Instance.FollowerInformationBoxTemplate;
		}
	}
}
