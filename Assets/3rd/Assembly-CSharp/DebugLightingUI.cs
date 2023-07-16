using UnityEngine;

[ExecuteInEditMode]
public class DebugLightingUI : BaseMonoBehaviour
{
	private void Start()
	{
	}

	private void OnGUI()
	{
		LightingManager instance = LightingManager.Instance;
		if (!(instance == null))
		{
			GUIStyle gUIStyle = new GUIStyle();
			gUIStyle.fontSize = 30;
			gUIStyle.normal.textColor = Color.white;
			GUI.Label(new Rect(10f, 10f, 0f, 0f), "isGlobalOverride: " + instance.inGlobalOverride, gUIStyle);
			GUI.Label(new Rect(10f, 50f, 0f, 0f), "inOverride: " + instance.inOverride, gUIStyle);
			GUI.Label(new Rect(10f, 90f, 0f, 0f), "overrideNaturalLight: " + instance.overrideNaturalLightRot, gUIStyle);
		}
	}
}
