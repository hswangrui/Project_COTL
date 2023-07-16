using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDoctrineXPBarSmall : BaseMonoBehaviour
{
	public TextMeshProUGUI Text;

	public Image ProgressBar;

	public void Play(float Progress, string SermonNameAndLevel)
	{
		Text.text = SermonNameAndLevel;
		ProgressBar.transform.localScale = new Vector3(Progress, 1f);
	}
}
