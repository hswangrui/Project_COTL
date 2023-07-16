using UnityEngine;

public class UITwitchExtensionButton : MMButton
{
	protected override void Awake()
	{
		base.Awake();
		base.onClick.AddListener(OnClick);
	}

	private void OnClick()
	{
		if (base.interactable)
		{
			Application.OpenURL("https://dashboard.twitch.tv/extensions/wph0p912gucvcee0114kfoukn319db");
		}
	}
}
