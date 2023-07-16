using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class FollowerIndoctrinationTabNavigatorBase : MMTabNavigatorBase<FollowerIndoctrinationTab>
	{
		[SerializeField]
		private RectTransform _indicatorTransform;

		[SerializeField]
		private Image _indicatorImage;

		protected override void PerformTransitionTo(FollowerIndoctrinationTab from, FollowerIndoctrinationTab to)
		{
			base.PerformTransitionTo(from, to);
			_indicatorImage.sprite = to.CategorySprite;
			_indicatorTransform.DOKill();
			_indicatorTransform.DOAnchorPosX(to.RectTransform.anchoredPosition.x, 0.2f).SetUpdate(true);
		}
	}
}
