using src.UINavigator;
using UnityEngine;

namespace Lamb.UI
{
	public class UIRoadmapOverlayController : UIMenuBase
	{
		[SerializeField]
		private MMScrollRect _scrollRect;

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			UIManager.PlayAudio("event:/ui/open_menu");
			_scrollRect.normalizedPosition = Vector3.one;
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
			_scrollRect.enabled = false;
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
