using I2.Loc;
using Spine.Unity;
using src.Extensions;
using UnityEngine;

namespace Lamb.UI.PauseDetails
{
	public class QuestItemActive : QuestItemBase<ObjectivesData>
	{
		[Header("Tracking")]
		[SerializeField]
		protected GameObject _trackingContainer;

		[SerializeField]
		protected SkeletonGraphic _trackingTick;

		private void Start()
		{
			_button.onClick.AddListener(OnClicked);
		}

		public override void Configure(bool failed = false)
		{
			base.Configure();
			_datas.Sort((ObjectivesData a, ObjectivesData b) => a.Index.CompareTo(b.Index));
			foreach (ObjectivesData data in _datas)
			{
				AddObjective(data, failed);
			}
		}

		public override void AddObjectivesData(ObjectivesData data)
		{
			if (string.IsNullOrEmpty(_uniqueGroupID))
			{
				_uniqueGroupID = data.UniqueGroupID;
				_title.text = LocalizationManager.GetTranslation(data.GroupId);
				_trackedQuest = DataManager.Instance.TrackedObjectiveGroupIDs.Contains(_uniqueGroupID);
				if (_trackedQuest)
				{
					_trackingTick.SetAnimation("on");
				}
			}
			if (_uniqueGroupID == data.UniqueGroupID)
			{
				_datas.Add(data);
			}
		}

		protected override void AddObjective(ObjectivesData objectivesData, bool failed = false)
		{
			GameObjectExtensions.Instantiate(_questItemObjectiveTemplate, _objectivesContainer).Configure(objectivesData);
		}

		private void OnClicked()
		{
			_trackedQuest = !_trackedQuest;
			if (_trackedQuest)
			{
				ObjectiveManager.TrackGroup(_uniqueGroupID);
			}
			else
			{
				ObjectiveManager.UntrackGroup(_uniqueGroupID);
			}
			_trackingTick.SetAnimation(_trackedQuest ? "turn-on" : "off");
		}
	}
}
