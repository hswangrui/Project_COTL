using UnityEngine;
using UnityEngine.UI;

public class VersionNumber : BaseMonoBehaviour
{
	public Text Text;

	private void OnEnable()
	{
		Text.text = Application.version;
	}
}
