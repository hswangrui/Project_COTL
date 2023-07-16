using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRitualScreenListObject : BaseMonoBehaviour
{
	public TextMeshProUGUI Text;

	public TextMeshProUGUI SelectedText;

	public Image Icon;

	public Image SelectedIcon;

	public RectTransform ShakeRectTransform;

	private float Shaking;

	private float ShakeSpeed;

	public void Init(SermonsAndRituals.SermonRitualType Type)
	{
		TextMeshProUGUI text = Text;
		string text3 = (SelectedText.text = SermonsAndRituals.LocalisedName(Type));
		text.text = text3;
		Image icon = Icon;
		Sprite sprite2 = (SelectedIcon.sprite = SermonsAndRituals.Sprite(Type));
		icon.sprite = sprite2;
	}

	public void Shake()
	{
		ShakeSpeed = 25 * ((Random.Range(0, 2) < 1) ? 1 : (-1));
	}

	private void Update()
	{
		ShakeSpeed += (0f - Shaking) * 0.4f;
		Shaking += (ShakeSpeed *= 0.8f);
		ShakeRectTransform.localPosition = Vector3.left * Shaking;
	}
}
