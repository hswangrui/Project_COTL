using TMPro;
using UnityEngine;

[ExecuteAlways]
public class UpdateSizeTMPro : BaseMonoBehaviour
{
	private TextMeshProUGUI Text;

	public bool UpdateHeight = true;

	public bool UpdateWidth = true;

	private string _text;

	private string text
	{
		set
		{
			if (_text != value)
			{
				Text.rectTransform.sizeDelta = new Vector2(UpdateWidth ? Text.preferredWidth : Text.rectTransform.sizeDelta.x, UpdateHeight ? Text.preferredHeight : Text.rectTransform.sizeDelta.y);
			}
			_text = value;
		}
	}

	private void Start()
	{
		Text = GetComponent<TextMeshProUGUI>();
	}

	private void LateUpdate()
	{
		text = Text.text;
	}
}
