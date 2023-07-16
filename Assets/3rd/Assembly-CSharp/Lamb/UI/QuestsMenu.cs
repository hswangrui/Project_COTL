using System.Collections.Generic;
using System.Linq;
using Lamb.UI.PauseDetails;
using src.Extensions;
using src.UI.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lamb.UI
{
	public class QuestsMenu : UISubmenuBase
	{
		[FormerlySerializedAs("_questItemTemplate")]
		[Header("Templates")]
		[SerializeField]
		private QuestItemActive _questItemActiveTemplate;

		[SerializeField]
		private QuestItemInactive _questItemInActiveTemplate;

		[Header("Quests Menu")]
		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _activeQuestsContent;

		[SerializeField]
		private GameObject _completedQuestsHeader;

		[SerializeField]
		private RectTransform _completedQuestsContent;

		[SerializeField]
		private GameObject _failedQuestsHeader;

		[SerializeField]
		private RectTransform _failedQuestsContent;

		[SerializeField]
		private GameObject _noActiveQuestsText;

		[SerializeField]
		private GameObject _noCompletedQuestsText;

		[SerializeField]
		private GameObject _noFailedQuestsText;

		private Dictionary<string, QuestItemActive> _activeQuestItems = new Dictionary<string, QuestItemActive>();

		private Dictionary<string, QuestItemInactive> _completedQuestItems = new Dictionary<string, QuestItemInactive>();

		private Dictionary<string, QuestItemInactive> _failedQuestItems = new Dictionary<string, QuestItemInactive>();

		protected override void OnShowStarted()
		{
			_scrollRect.enabled = false;
			_scrollRect.normalizedPosition = Vector3.one;
			if (_activeQuestItems.Count + _completedQuestItems.Count + _failedQuestItems.Count == 0)
			{
				foreach (ObjectivesData objective in DataManager.Instance.Objectives)
				{
					QuestItemActive value;
					if (_activeQuestItems.TryGetValue(objective.UniqueGroupID, out value))
					{
						value.AddObjectivesData(objective);
						continue;
					}
					QuestItemActive questItemActive = GameObjectExtensions.Instantiate(_questItemActiveTemplate, _activeQuestsContent);
					questItemActive.AddObjectivesData(objective);
					_activeQuestItems.Add(objective.UniqueGroupID, questItemActive);
				}
				foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
				{
					QuestItemActive value2;
					if (_activeQuestItems.TryGetValue(completedObjective.UniqueGroupID, out value2))
					{
						value2.AddObjectivesData(completedObjective);
						continue;
					}
					QuestItemActive questItemActive2 = GameObjectExtensions.Instantiate(_questItemActiveTemplate, _activeQuestsContent);
					questItemActive2.AddObjectivesData(completedObjective);
					_activeQuestItems.Add(completedObjective.UniqueGroupID, questItemActive2);
				}
				foreach (ObjectivesDataFinalized item in DataManager.Instance.CompletedObjectivesHistory)
				{
					if (!_activeQuestItems.ContainsKey(item.UniqueGroupID))
					{
						QuestItemInactive value3;
						if (_completedQuestItems.TryGetValue(item.UniqueGroupID, out value3))
						{
							value3.AddObjectivesData(item);
							continue;
						}
						QuestItemInactive questItemInactive = GameObjectExtensions.Instantiate(_questItemInActiveTemplate, _completedQuestsContent);
						questItemInactive.AddObjectivesData(item);
						_completedQuestItems.Add(item.UniqueGroupID, questItemInactive);
					}
				}
				_completedQuestsHeader.SetActive(_completedQuestItems.Count > 0);
				_completedQuestsContent.gameObject.SetActive(_completedQuestItems.Count > 0);
				foreach (ObjectivesDataFinalized item2 in DataManager.Instance.FailedObjectivesHistory)
				{
					if (!_completedQuestItems.ContainsKey(item2.UniqueGroupID))
					{
						QuestItemInactive value4;
						if (_failedQuestItems.TryGetValue(item2.UniqueGroupID, out value4))
						{
							value4.AddObjectivesData(item2);
							continue;
						}
						QuestItemInactive questItemInactive2 = GameObjectExtensions.Instantiate(_questItemInActiveTemplate, _failedQuestsContent);
						questItemInactive2.AddObjectivesData(item2);
						_failedQuestItems.Add(item2.UniqueGroupID, questItemInactive2);
					}
				}
				_failedQuestsHeader.SetActive(_failedQuestItems.Count > 0);
				_failedQuestsContent.gameObject.SetActive(_failedQuestItems.Count > 0);
				foreach (QuestItemActive item3 in _activeQuestItems.Values.ToList())
				{
					item3.Configure();
				}
				foreach (QuestItemInactive item4 in _completedQuestItems.Values.ToList())
				{
					item4.Configure();
				}
				foreach (QuestItemInactive item5 in _failedQuestItems.Values.ToList())
				{
					item5.Configure(true);
				}
				_noActiveQuestsText.SetActive(_activeQuestItems.Count == 0);
				_noCompletedQuestsText.SetActive(_completedQuestItems.Count == 0);
				_noFailedQuestsText.SetActive(_failedQuestItems.Count == 0);
				if (_activeQuestItems.Count > 0)
				{
					OverrideDefault(_activeQuestItems.Values.ToList()[0].Button);
				}
				else if (_completedQuestItems.Count > 0)
				{
					OverrideDefaultOnce(_completedQuestItems.Values.ToList()[0].Button);
				}
				else if (_failedQuestItems.Count > 0)
				{
					OverrideDefault(_failedQuestItems.Values.ToList()[0].Button);
				}
			}
			ActivateNavigation();
			_scrollRect.enabled = true;
		}
	}
}
