using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MMPause_ToggleCamEffects : BaseMonoBehaviour
{
	public List<GameObject> Disable;

	public TextMeshProUGUI Text;

	private void OnEnable()
	{
		Disable = new List<GameObject>();
		Disable.Add(GameObject.Find("Shadow_Camera_RenderText"));
		Disable.Add(GameObject.Find("Lighting_Camera_RenderText"));
		Disable.Add(GameObject.Find("Scenery_Camera_RenderText"));
		Disable.Add(GameObject.Find("RoomEffects_Julian"));
		Text.text = "Cam Effects: " + Disable[0].activeSelf;
	}

	public void ToggleCamEffects()
	{
		bool activeSelf = Disable[0].activeSelf;
		foreach (GameObject item in Disable)
		{
			item.SetActive(!activeSelf);
		}
		Text.text = "Cam Effects: " + Disable[0].activeSelf;
	}
}
