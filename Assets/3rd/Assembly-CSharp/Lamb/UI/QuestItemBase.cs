using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public abstract class QuestItemBase<T> : MonoBehaviour
	{
		protected const string kTrackingOnAnimation = "on";

		protected const string kTrackignTurnOnAnimation = "turn-on";

		protected const string kTrackingOffAnimation = "off";

		[Header("General")]
		[SerializeField]
		protected MMButton _button;

		[SerializeField]
		protected TextMeshProUGUI _title;

		[SerializeField]
		protected RectTransform _objectivesContainer;

		[SerializeField]
		protected QuestItemObjective _questItemObjectiveTemplate;

		protected string _uniqueGroupID = "";

		protected bool _trackedQuest;

		protected List<T> _datas = new List<T>();

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public abstract void AddObjectivesData(T data);

		protected abstract void AddObjective(T data, bool failed = false);

		public virtual void Configure(bool failed = false)
		{
		}
	}
}
