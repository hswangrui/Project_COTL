using I2.Loc;
using TMPro;
using UnityEngine.EventSystems;

public class UITwitchButton : MMButton
{
	private TMP_Text buttonText;

	protected override void Awake()
	{
		base.Awake();
		buttonText = GetComponentInChildren<TMP_Text>();
		base.onClick.AddListener(OnClick);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		buttonText.text = LocalizationManager.GetTranslation("UI/Twitch/Connect");
		//if (!string.IsNullOrEmpty(TwitchManager.ChannelName) && TwitchAuthentication.IsAuthenticated)
		//{
		//	buttonText.text = TwitchManager.ChannelName + " - " + LocalizationManager.GetTranslation("UI/Twitch/SignOut");
		//}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
	}

	private void OnClick()
	{
		if (!base.interactable)
		{
			return;
		}
		//if (!TwitchAuthentication.IsAuthenticated)
		//{
		//	TwitchAuthentication.RequestLogIn(delegate
		//	{
		//		if (!string.IsNullOrEmpty(TwitchManager.ChannelName))
		//		{
		//			buttonText.text = TwitchManager.ChannelName + " - " + LocalizationManager.GetTranslation("UI/Twitch/SignOut");
		//		}
		//	});
		//}
		//else
		//{
		//	TwitchAuthentication.SignOut();
		//	buttonText.text = LocalizationManager.GetTranslation("UI/Twitch/Connect");
		//}
	}
}
