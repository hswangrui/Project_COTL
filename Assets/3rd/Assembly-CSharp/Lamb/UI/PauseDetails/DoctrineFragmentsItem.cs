using UnityEngine;

namespace Lamb.UI.PauseDetails
{
	public class DoctrineFragmentsItem : PlayerMenuItem<int>
	{
		[SerializeField]
		private GameObject _lockedContainer;

		[SerializeField]
		private GameObject _top;

		[SerializeField]
		private GameObject _right;

		[SerializeField]
		private GameObject _left;

		[SerializeField]
		private MMSelectable _selectable;

		public MMSelectable Selectable
		{
			get
			{
				return _selectable;
			}
		}

		public override void Configure(int pieces)
		{
			_lockedContainer.SetActive(DataManager.Instance.CompletedDoctrineStones == 0 && !DataManager.Instance.FirstDoctrineStone);
			_left.SetActive(pieces >= 1);
			_right.SetActive(pieces >= 2);
			_top.SetActive(pieces >= 3);
		}
	}
}
