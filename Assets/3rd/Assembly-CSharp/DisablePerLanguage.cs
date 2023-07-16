using I2.Loc;
using UnityEngine;

public class DisablePerLanguage : MonoBehaviour
{
	private MMButton _button;

	private CanvasGroup _canvasGroup;

	public bool disabled;

	private void Start()
	{
		_button = GetComponent<MMButton>();
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += CheckLanguage;
		CheckLanguage();
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= CheckLanguage;
	}

	private void CheckLanguage()
	{
		if (_button != null && _canvasGroup != null)
		{
			if (LocalizationManager.CurrentLanguage == "English")
			{
				_button.Interactable = true;
				_canvasGroup.alpha = 1f;
				_canvasGroup.interactable = true;
				disabled = false;
			}
			else
			{
				_canvasGroup.interactable = false;
				_button.Interactable = false;
				_canvasGroup.alpha = 0.5f;
				disabled = true;
			}
		}
	}
}
