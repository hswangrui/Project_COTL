using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectInfo : BaseMonoBehaviour
{
	public static CharacterSelectInfo Instance;

	public RectTransform Info;

	public Canvas canvas;

	public float TargetX = 600f;

	public TextMeshProUGUI text;

	public TextMeshProUGUI SubText;

	public Image HeadGraphic;

	private float Timer;

	private void Start()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		TargetX = 600f;
		Info.localPosition = new Vector3(TargetX, 0f);
	}

	public void Show(bool show)
	{
		TargetX = ((!show) ? 600 : 0);
		Timer = 0.5f;
	}

	public void SetInfo(string Name, string Subtitle, bool ShowGraphic)
	{
		if (text.text != Name.ToUpper())
		{
			text.text = Name.ToUpper();
			SubText.text = Subtitle;
			Info.localPosition = new Vector3(600f, 0f);
		}
		if (HeadGraphic.gameObject.activeSelf != ShowGraphic)
		{
			HeadGraphic.gameObject.SetActive(ShowGraphic);
		}
	}

	private void Update()
	{
		Info.localPosition = Vector3.Lerp(Info.localPosition, new Vector3(TargetX, 0f), 20f * Time.deltaTime);
		if (TargetX == 0f && (Timer -= Time.deltaTime) < 0f)
		{
			TargetX = 600f;
		}
	}
}
