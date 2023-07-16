using I2.Loc;
using TMPro;
using UnityEngine;

public class NotificationGeneric : NotificationBase
{
	[SerializeField]
	private TextMeshProUGUI _icon;

	private string _locKey;

	private string[] _nonLocalizedParameters;

	private string[] _localizedParameters;

	protected override float _onScreenDuration
	{
		get
		{
			return 3f;
		}
	}

	protected override float _showHideDuration
	{
		get
		{
			return 0.4f;
		}
	}

	public void ConfigureLocalizedParams(string locKey, params string[] localizedParameters)
	{
		_localizedParameters = localizedParameters;
		Configure(locKey);
	}

	public void ConfigureNonLocalizedParams(string locKey, params string[] nonLocalizedParameters)
	{
		_nonLocalizedParameters = nonLocalizedParameters;
		Configure(locKey);
	}

	public void Configure(NotificationCentre.NotificationType type, Flair flair = Flair.None)
	{
		_icon.text = GetNotificationIcon(type);
		Configure(NotificationCentre.GetLocKey(type), flair);
	}

	public void Configure(string locKey, Flair flair = Flair.None)
	{
		_locKey = locKey;
		Configure(flair);
	}

	protected override void Localize()
	{
		if (_localizedParameters != null && _localizedParameters.Length != 0)
		{
			string[] array = new string[_localizedParameters.Length];
			for (int i = 0; i < _localizedParameters.Length; i++)
			{
				array[i] = LocalizationManager.GetTranslation(_localizedParameters[i]);
			}
			TextMeshProUGUI description = _description;
			string translation = LocalizationManager.GetTranslation(_locKey);
			object[] args = array;
			description.text = string.Format(translation, args);
		}
		else if (_nonLocalizedParameters != null && _nonLocalizedParameters.Length != 0)
		{
			TextMeshProUGUI description2 = _description;
			string translation2 = LocalizationManager.GetTranslation(_locKey);
			object[] args = _nonLocalizedParameters;
			description2.text = string.Format(translation2, args);
		}
		else
		{
			_description.text = LocalizationManager.GetTranslation(_locKey);
		}
	}

	private string GetNotificationIcon(NotificationCentre.NotificationType Notification)
	{
		switch (Notification)
		{
		case NotificationCentre.NotificationType.BuildComplete:
			return "\uf6e3";
		case NotificationCentre.NotificationType.FaithUp:
			return "UP";
		case NotificationCentre.NotificationType.FaithUpDoubleArrow:
			return "UP DOUBLE";
		case NotificationCentre.NotificationType.FaithDown:
			return "DOWN";
		case NotificationCentre.NotificationType.FaithDownDoubleArrow:
			return "DOWN DOUBLE";
		default:
			return "";
		}
	}
}
