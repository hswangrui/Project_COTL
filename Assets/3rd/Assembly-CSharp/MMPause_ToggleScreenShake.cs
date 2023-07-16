using TMPro;

public class MMPause_ToggleScreenShake : BaseMonoBehaviour
{
	public TextMeshProUGUI text;

	private void OnEnable()
	{
		text.text = (DataManager.Instance.Options_ScreenShake ? "Disable Screen Shake" : "Enable Screen Shake");
	}

	public void ToggleScreenShake()
	{
		DataManager.Instance.Options_ScreenShake = !DataManager.Instance.Options_ScreenShake;
		text.text = (DataManager.Instance.Options_ScreenShake ? "Disable Screen Shake" : "Enable Screen Shake");
	}
}
