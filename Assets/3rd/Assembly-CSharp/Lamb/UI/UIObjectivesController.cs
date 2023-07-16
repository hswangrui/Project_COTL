using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI
{
	public class UIObjectivesController : BaseMonoBehaviour
	{
		private const float kShowDuration = 0.5f;

		private const float kHideDuration = 0.5f;

		[Header("Objectives Controller")]
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _contentContainer;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[Header("Templates")]
		[SerializeField]
		private UIObjectiveGroup _objectiveGroupTemplate;

		private Dictionary<string, UIObjectiveGroup> _objectiveGroups = new Dictionary<string, UIObjectiveGroup>();

		private Vector2 _onScreenPosition;

		private Vector2 _offScreenPosition;

		private bool _initialized;

		public bool Hidden { get; private set; }

		private void Awake()
		{
			_onScreenPosition = (_offScreenPosition = _rectTransform.anchoredPosition);
			_offScreenPosition.x = _rectTransform.sizeDelta.x + 50f;
		}

		private void Start()
		{
			OnLoadComplete();
		}

		private void OnEnable()
		{
			SaveAndLoad.OnLoadComplete = (Action)Delegate.Combine(SaveAndLoad.OnLoadComplete, new Action(OnLoadComplete));
			ObjectiveManager.OnObjectiveAdded += OnObjectiveAdded;
			ObjectiveManager.OnObjectiveCompleted += OnObjectiveCompleted;
			ObjectiveManager.OnObjectiveRemoved += OnObjectiveRemoved;
			ObjectiveManager.OnObjectiveFailed += OnObjectiveFailed;
			ObjectiveManager.OnObjectiveTracked = (Action<string>)Delegate.Combine(ObjectiveManager.OnObjectiveTracked, new Action<string>(OnObjectiveTracked));
			ObjectiveManager.OnObjectiveUntracked = (Action<string>)Delegate.Combine(ObjectiveManager.OnObjectiveUntracked, new Action<string>(OnObjectiveUntracked));
		}

		private void OnDisable()
		{
			SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(OnLoadComplete));
			ObjectiveManager.OnObjectiveAdded -= OnObjectiveAdded;
			ObjectiveManager.OnObjectiveCompleted -= OnObjectiveCompleted;
			ObjectiveManager.OnObjectiveRemoved -= OnObjectiveRemoved;
			ObjectiveManager.OnObjectiveFailed -= OnObjectiveFailed;
			ObjectiveManager.OnObjectiveTracked = (Action<string>)Delegate.Remove(ObjectiveManager.OnObjectiveTracked, new Action<string>(OnObjectiveTracked));
			ObjectiveManager.OnObjectiveUntracked = (Action<string>)Delegate.Remove(ObjectiveManager.OnObjectiveUntracked, new Action<string>(OnObjectiveUntracked));
		}

		public void Show(bool instant = false)
		{
			_rectTransform.DOKill();
			StopAllCoroutines();
			if (instant)
			{
				Hidden = false;
				_rectTransform.anchoredPosition = _onScreenPosition;
			}
			else
			{
				StartCoroutine(DoShow());
			}
		}

		private IEnumerator DoShow()
		{
			_rectTransform.DOKill();
			_rectTransform.DOAnchorPos(_onScreenPosition, 0.5f).SetEase(Ease.InOutSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
			Hidden = false;
		}

		public void Hide(bool instant = false)
		{
			Debug.Log("UIObjectivesController - Hide".Colour(Color.yellow));
			Hidden = true;
			_rectTransform.DOKill();
			StopAllCoroutines();
			if (instant)
			{
				_rectTransform.anchoredPosition = _offScreenPosition;
			}
			else
			{
				StartCoroutine(DoHide());
			}
		}

		private IEnumerator DoHide()
		{
			_rectTransform.DOKill();
			_rectTransform.DOAnchorPos(_offScreenPosition, 0.5f).SetEase(Ease.InOutSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.5f);
		}

		public void HideAllExcluding(string groupID)
		{
			foreach (UIObjectiveGroup value in _objectiveGroups.Values)
			{
				if (value.GroupID != groupID)
				{
					value.HideSoft();
				}
			}
		}

		public void ShowAll()
		{
			foreach (KeyValuePair<string, UIObjectiveGroup> objectiveGroup in _objectiveGroups)
			{
				objectiveGroup.Value.Show();
			}
		}

		private void OnLoadComplete()
		{
			if (_initialized || !SaveAndLoad.Loaded)
			{
				return;
			}
			Debug.Log("UIObjectivesController - Load Complete".Colour(Color.yellow));
			foreach (ObjectivesData objective in DataManager.Instance.Objectives)
			{
				if (ObjectiveManager.IsTracked(objective.UniqueGroupID))
				{
					AddObjectiveInstance(objective);
				}
				objective.Init(false);
			}
			foreach (ObjectivesData completedObjective in DataManager.Instance.CompletedObjectives)
			{
				if (ObjectiveManager.IsTracked(completedObjective.UniqueGroupID))
				{
					AddObjectiveInstance(completedObjective);
				}
			}
			foreach (ObjectivesData failedObjective in DataManager.Instance.FailedObjectives)
			{
				if (ObjectiveManager.IsTracked(failedObjective.UniqueGroupID))
				{
					AddObjectiveInstance(failedObjective);
				}
			}
			_initialized = true;
		}

		private void OnObjectiveTracked(string uniqueGroupID)
		{
			UIObjectiveGroup value;
			if (_objectiveGroups.TryGetValue(uniqueGroupID, out value))
			{
				value.UpdateTrackingState(true);
				return;
			}
			foreach (ObjectivesData item in ObjectiveManager.GetAllObjectivesOfGroup(uniqueGroupID))
			{
				AddObjectiveInstance(item);
			}
		}

		private void OnObjectiveUntracked(string uniqueGroupID)
		{
			UIObjectiveGroup value;
			if (_objectiveGroups.TryGetValue(uniqueGroupID, out value))
			{
				value.Hide();
				_objectiveGroups.Remove(uniqueGroupID);
			}
		}

		private void OnObjectiveAdded(ObjectivesData objective)
		{
			if (!objective.IsComplete)
			{
				AddObjectiveInstance(objective);
			}
		}

		private void OnObjectiveCompleted(ObjectivesData objective)
		{
			UIObjectiveGroup objectiveGroup;
			if (_objectiveGroups.TryGetValue(objective.UniqueGroupID, out objectiveGroup))
			{
				AddObjectiveInstance(objective, !objectiveGroup.Shown);
			}
			else
			{
				objectiveGroup = AddObjectiveInstance(objective, true);
				foreach (ObjectivesData item in ObjectiveManager.GetAllObjectivesOfGroup(objective.UniqueGroupID))
				{
					objectiveGroup.AddObjective(item);
				}
			}
			if (!objectiveGroup.Shown)
			{
				if (!objectiveGroup.Showing)
				{
					objectiveGroup.Show();
				}
				UIObjectiveGroup uIObjectiveGroup = objectiveGroup;
				uIObjectiveGroup.OnShown = (Action)Delegate.Combine(uIObjectiveGroup.OnShown, (Action)delegate
				{
					objectiveGroup.CompleteObjective(objective);
				});
			}
			else
			{
				objectiveGroup.CompleteObjective(objective);
			}
		}

		private void OnObjectiveFailed(ObjectivesData objective)
		{
			UIObjectiveGroup objectiveGroup;
			if (_objectiveGroups.TryGetValue(objective.UniqueGroupID, out objectiveGroup))
			{
				AddObjectiveInstance(objective, !objectiveGroup.Shown);
			}
			else
			{
				objectiveGroup = AddObjectiveInstance(objective, true);
				foreach (ObjectivesData item in ObjectiveManager.GetAllObjectivesOfGroup(objective.UniqueGroupID))
				{
					objectiveGroup.AddObjective(item);
				}
			}
			if (!objectiveGroup.Shown)
			{
				if (!objectiveGroup.Showing)
				{
					objectiveGroup.Show();
				}
				UIObjectiveGroup uIObjectiveGroup = objectiveGroup;
				uIObjectiveGroup.OnShown = (Action)Delegate.Combine(uIObjectiveGroup.OnShown, (Action)delegate
				{
					objectiveGroup.FailObjective(objective);
				});
			}
			else
			{
				objectiveGroup.FailObjective(objective);
			}
		}

		private void OnObjectiveRemoved(ObjectivesData objective)
		{
		}

		private UIObjectiveGroup AddObjectiveInstance(ObjectivesData objectivesData, bool ignoreStatus = false, bool instant = false)
		{
			UIObjectiveGroup objectiveGroup;
			if (_objectiveGroups.TryGetValue(objectivesData.UniqueGroupID, out objectiveGroup))
			{
				if (!objectiveGroup.HasObjective(objectivesData))
				{
					objectiveGroup.AddObjective(objectivesData, ignoreStatus);
				}
				return objectiveGroup;
			}
			objectiveGroup = GameObjectExtensions.Instantiate(_objectiveGroupTemplate, _contentContainer);
			objectiveGroup.transform.SetSiblingIndex(10);
			objectiveGroup.Configure(objectivesData.UniqueGroupID, objectivesData.GroupId);
			objectiveGroup.AddObjective(objectivesData, ignoreStatus);
			objectiveGroup.Show(instant);
			UIObjectiveGroup uIObjectiveGroup = objectiveGroup;
			uIObjectiveGroup.OnHidden = (Action)Delegate.Combine(uIObjectiveGroup.OnHidden, (Action)delegate
			{
				_objectiveGroups.Remove(objectiveGroup.UniqueGroupID);
			});
			_objectiveGroups.Add(objectivesData.UniqueGroupID, objectiveGroup);
			return objectiveGroup;
		}
	}
}
