using TMPro;
using UnityEngine;

public class NotificationDynamicGeneric : NotificationDynamicBase
{
	[SerializeField]
	private TextMeshProUGUI _textIcon;

	[SerializeField]
	private TextMeshProUGUI _text;

	public override Color FullColour
	{
		get
		{
			return StaticColors.RedColor;
		}
	}

	public override Color EmptyColour
	{
		get
		{
			return StaticColors.RedColor;
		}
	}

	protected override void UpdateIcon()
	{
		string text;
		switch (base.Data.Type)
		{
		case NotificationCentre.NotificationType.Dynamic_Homeless:
			text = "\uf236";
			break;
		case NotificationCentre.NotificationType.Dynamic_Starving:
			text = "\uf623";
			break;
		case NotificationCentre.NotificationType.Dynamic_Sick:
			text = "<sprite name=\"icon_Sickness\">";
			break;
		case NotificationCentre.NotificationType.Exhausted:
			text = "<sprite name=\"icon_Sleep\">";
			break;
		case NotificationCentre.NotificationType.Dynamic_Dissenter:
			text = "<sprite name=\"icon_Faith\">";
			break;
		default:
			text = "";
			break;
		}
		_textIcon.text = text;
		_text.text = base.Data.TotalCount.ToString();
	}
}
