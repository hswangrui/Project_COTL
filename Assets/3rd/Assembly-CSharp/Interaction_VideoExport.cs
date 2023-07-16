using System;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.VideoMenu;
using src.Extensions;
using UnityEngine;

public class Interaction_VideoExport : Interaction
{
	private UIVideoExportMenuController videoMenu;

	public override void GetLabel()
	{
		base.Label = ScriptLocalization.Interactions.Look;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		GameManager.GetInstance().OnConversationNew();
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		videoMenu = MonoSingleton<UIManager>.Instance.VideoExportTemplate.Instantiate();
		videoMenu.Show(TimeManager.CurrentDay);
		UIVideoExportMenuController uIVideoExportMenuController = videoMenu;
		uIVideoExportMenuController.OnHidden = (Action)Delegate.Combine(uIVideoExportMenuController.OnHidden, (Action)delegate
		{
			Time.timeScale = 1f;
			Interactable = true;
			GameManager.GetInstance().OnConversationEnd();
		});
		UIVideoExportMenuController uIVideoExportMenuController2 = videoMenu;
		uIVideoExportMenuController2.OnCancel = (Action)Delegate.Combine(uIVideoExportMenuController2.OnCancel, (Action)delegate
		{
			Time.timeScale = 1f;
			Interactable = true;
			GameManager.GetInstance().OnConversationEnd();
		});
	}
}
