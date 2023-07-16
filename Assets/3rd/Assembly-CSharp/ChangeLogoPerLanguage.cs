using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class ChangeLogoPerLanguage : MonoBehaviour
{
	[SerializeField]
	private Image logo;

	[SerializeField]
	private Sprite chineseLogo;

	[SerializeField]
	private Sprite englishLogo;

	public void UpdateLogo()
	{
		string currentLanguage = LocalizationManager.CurrentLanguage;
		if (currentLanguage == "Chinese (Simplified)")
		{
			logo.sprite = chineseLogo;
		}
		else
		{
			logo.sprite = englishLogo;
		}
	}

	private void OnEnable()
	{
		UpdateLogo();
		LocalizationManager.OnLocalizeEvent += UpdateLogo;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= UpdateLogo;
	}
}
