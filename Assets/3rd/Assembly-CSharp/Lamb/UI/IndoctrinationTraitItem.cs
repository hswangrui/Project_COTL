using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class IndoctrinationTraitItem : MonoBehaviour
	{
		[SerializeField]
		private MMSelectable _selectable;

		[SerializeField]
		private Image _icon;

		[SerializeField]
		private Image _arrow;

		[Header("Arrows")]
		[SerializeField]
		private Sprite _positiveArrow;

		[SerializeField]
		private Sprite _negativeArrow;

		private FollowerTrait.TraitType _traitType;

		public MMSelectable Selectable
		{
			get
			{
				return _selectable;
			}
		}

		public FollowerTrait.TraitType TraitType
		{
			get
			{
				return _traitType;
			}
		}

		public void Configure(FollowerTrait.TraitType traitType)
		{
			_traitType = traitType;
			_icon.sprite = FollowerTrait.GetIcon(_traitType);
			_arrow.sprite = (FollowerTrait.IsPositiveTrait(_traitType) ? _positiveArrow : _negativeArrow);
		}
	}
}
