using TMPro;
using UnityEngine;

public class DemoWatermark : MonoBehaviour
{
	public static GameObject Instance;

	public TextMeshProUGUI Text;

	public static void Play()
	{
		if (Instance == null)
		{
			Instance = Object.Instantiate(Resources.Load("DemoWatermark/DemoWatermark")) as GameObject;
		}
	}

	private void Update()
	{
		if (!CheatConsole.IN_DEMO)
		{
			Object.Destroy(Instance);
		}
		else if (CheatConsole.DemoBeginTime == 0f || CheatConsole.DemoBeginTime >= 1200f)
		{
			Text.text = "Work In Progress Gameplay";
		}
		else
		{
			Text.text = "Work In Progress Gameplay | " + GetTime();
		}
	}

	private string GetTime()
	{
		float num = 1200f - CheatConsole.DemoBeginTime;
		int num2 = Mathf.FloorToInt(num / 60f);
		int num3 = 0;
		while (num2 > 60)
		{
			num3++;
			num2 -= 60;
		}
		int num4 = Mathf.FloorToInt(num % 60f);
		if (num3 > 0 && num3 < 10)
		{
			string text = "0" + num3;
		}
		if (num3 >= 10)
		{
			num3.ToString();
		}
		return ((num2 < 10) ? "0" : "") + num2 + ":" + ((num4 < 10) ? "0" : "") + num4;
	}
}
