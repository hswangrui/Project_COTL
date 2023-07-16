using UnityEngine;

namespace Lamb.UI.PauseDetails
{
	public class TalismanPiecesItem : PlayerMenuItem<int>
	{
		[SerializeField]
		private GameObject _lockedContainer;

		[SerializeField]
		private GameObject _top;

		[SerializeField]
		private GameObject _right;

		[SerializeField]
		private GameObject _bottom;

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
			_lockedContainer.SetActive(Inventory.KeyPieces + Inventory.TempleKeys == 0 && !DataManager.Instance.HadFirstTempleKey);
			_top.SetActive(pieces >= 1);
			_right.SetActive(pieces >= 2);
			_bottom.SetActive(pieces >= 3);
			_left.SetActive(pieces >= 4);
		}
	}
}
