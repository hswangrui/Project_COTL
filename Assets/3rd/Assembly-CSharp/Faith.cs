using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Faith : BaseMonoBehaviour
{
	public Image image;

	public Worshipper worshipper;

	public Gradient gradient;

	public TextMeshProUGUI NameText;

	private void Update()
	{
		image.fillAmount = worshipper.wim.v_i.Faith / 100f;
		image.color = gradient.Evaluate(image.fillAmount);
		NameText.text = worshipper.wim.v_i.Name;
	}
}
