using UnityEngine;

namespace Lamb.UI.Menus.PlayerMenu
{
	public class UIPauseDetailsMenuController : UIMenuBase
	{
		public void Update()
		{
			if (_canvasGroup.interactable && InputManager.Gameplay.GetMenuButtonDown())
			{
				OnCancelButtonInput();
			}
			Time.timeScale = 0f;
		}

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			AudioManager.Instance.PauseActiveLoops();
			UIManager.PlayAudio("event:/ui/pause");
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			AudioManager.Instance.ResumeActiveLoops();
			UIManager.PlayAudio("event:/ui/unpause");
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
