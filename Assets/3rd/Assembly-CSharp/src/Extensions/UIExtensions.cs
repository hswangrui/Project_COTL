using System;
using System.Collections;
using Lamb.UI;

namespace src.Extensions
{
	public static class UIExtensions
	{
		public static IEnumerator YieldUntilHide(this UIMenuBase menu)
		{
			bool hidden = false;
			menu.OnHide = (Action)Delegate.Combine(menu.OnHide, (Action)delegate
			{
				hidden = true;
			});
			while (!hidden)
			{
				yield return null;
			}
		}

		public static IEnumerator YieldUntilHidden(this UIMenuBase menu)
		{
			bool hidden = false;
			menu.OnHidden = (Action)Delegate.Combine(menu.OnHidden, (Action)delegate
			{
				hidden = true;
			});
			while (!hidden)
			{
				yield return null;
			}
		}

		public static IEnumerator YieldUntilShow(this UIMenuBase menu)
		{
			bool hidden = false;
			menu.OnShow = (Action)Delegate.Combine(menu.OnShow, (Action)delegate
			{
				hidden = true;
			});
			while (!hidden)
			{
				yield return null;
			}
		}

		public static IEnumerator YieldUntilShown(this UIMenuBase menu)
		{
			bool hidden = false;
			menu.OnShown = (Action)Delegate.Combine(menu.OnShown, (Action)delegate
			{
				hidden = true;
			});
			while (!hidden)
			{
				yield return null;
			}
		}
	}
}
