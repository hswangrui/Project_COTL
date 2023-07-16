using UnityEngine;
using UnityEngine.UI;

public class LoadingIcon : MonoBehaviour
{
	private float Scale;

	private float ScaleSpeed;

	private float Rotation;

	private float Rotation2;

	public Image Icon;

	public Image Icon2;

	public RectTransform rectTransform;

	public bool ForceFifty;

	private float Timer;

	private float Duration = 0.3f;

	public void UpdateProgress(float Progress)
	{
		Icon.fillAmount = Progress;
		Icon2.fillAmount = Progress;
		if (ForceFifty)
		{
			Icon.fillAmount = 0.5f;
			Icon2.fillAmount = 0.5f;
		}
	}

	private void OnEnable()
	{
		Timer = 0f;
		Image icon = Icon;
		float fillAmount = (Icon2.fillAmount = 0f);
		icon.fillAmount = fillAmount;
	}

	private void Update()
	{
		Timer += Time.unscaledDeltaTime;
		Rotation -= 2f;
		Icon.rectTransform.eulerAngles = new Vector3(0f, 0f, Rotation);
		Rotation2 += 4f;
		Icon2.rectTransform.eulerAngles = new Vector3(0f, 0f, Rotation2);
	}
}
