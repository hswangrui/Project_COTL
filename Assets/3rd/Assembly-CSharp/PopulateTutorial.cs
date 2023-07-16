using I2.Loc;
using TMPro;
using UnityEngine;

public class PopulateTutorial : MonoBehaviour
{
	public TutorialTopic Topic;

	public TextMeshProUGUI Title;

	public TextMeshProUGUI Description;

	public TextMeshProUGUI Info1;

	public TextMeshProUGUI Info2;

	public TextMeshProUGUI Info3;

	private void OnEnable()
	{
		Populate();
	}

	private void Populate()
	{
		string text = "Tutorial UI/" + Topic;
		Title.text = LocalizationManager.GetTranslation(text);
		Description.text = LocalizationManager.GetTranslation(text + "/Description");
		Info1.text = LocalizationManager.GetTranslation(text + "/Info1");
		Info2.text = LocalizationManager.GetTranslation(text + "/Info2");
		Info3.text = LocalizationManager.GetTranslation(text + "/Info3");
	}
}
