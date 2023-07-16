using Lamb.UI;
using TMPro;
using UnityEngine;

public class UILoadingOverlayController : UIMenuBase
{
	[SerializeField]
	private TextMeshProUGUI _message;

	public string Message
	{
		get
		{
			return _message.text;
		}
		set
		{
			_message.text = value;
		}
	}
}
