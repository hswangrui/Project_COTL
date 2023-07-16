using DG.Tweening;
using UnityEngine;

namespace Lamb.UI.KitchenMenu
{
	public class CookingFireMenuTabNavigatorBase : MMTabNavigatorBase<CooklingFireMenuTab>
	{
		private static int _recentTab;

		public override void ShowDefault()
		{
			_defaultTabIndex = _recentTab;
			base.ShowDefault();
		}

		protected override void PerformTransitionTo(CooklingFireMenuTab from, CooklingFireMenuTab to)
		{
			_recentTab = _tabs.IndexOf(to);
			from.Menu.Hide();
			to.Menu.Show();
			int num = _tabs.IndexOf(to);
			int num2 = _tabs.IndexOf(from);
			RectTransform component = to.Menu.GetComponent<RectTransform>();
			RectTransform component2 = from.Menu.GetComponent<RectTransform>();
			if (num > num2)
			{
				component.DOKill();
				component.anchoredPosition = new Vector2(50f, 0f);
				component.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.InSine).SetUpdate(true);
				component2.DOKill();
				component2.anchoredPosition = Vector2.zero;
				component2.DOAnchorPos(new Vector2(-50f, 0f), 0.1f).SetEase(Ease.OutSine).SetUpdate(true);
			}
			else if (num2 > num)
			{
				component.DOKill();
				component.anchoredPosition = new Vector2(-50f, 0f);
				component.DOAnchorPos(Vector2.zero, 0.1f).SetEase(Ease.OutSine).SetUpdate(true);
				component2.DOKill();
				component2.anchoredPosition = Vector2.zero;
				component2.DOAnchorPos(new Vector2(50f, 0f), 0.1f).SetEase(Ease.InSine).SetUpdate(true);
			}
		}
	}
}
