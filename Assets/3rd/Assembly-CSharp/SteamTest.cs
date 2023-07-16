using I2.Loc;
using Steamworks;
using TMPro;
using UnityEngine;

public class SteamTest : BaseMonoBehaviour
{
	public TextMeshProUGUI Text;

	private void Start()
	{
		if (SteamManager.Initialized)
		{
			string personaName = SteamFriends.GetPersonaName();
			Debug.Log(">>>>>>>>> " + personaName);
			Text.text = ScriptLocalization.UI_MainMenu.Welcome + " " + personaName;
		}
		else
		{
			Text.text = "";
		}
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += UpdateLocalisation;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= UpdateLocalisation;
	}

	private void UpdateLocalisation()
	{
		Text.text = ScriptLocalization.UI_MainMenu.Welcome + " " + base.name;
	}
}
