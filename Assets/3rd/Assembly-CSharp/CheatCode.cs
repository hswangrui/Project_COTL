using Rewired;
using Unify.Input;
using UnityEngine;

public class CheatCode : MonoBehaviour
{
	public string[] cheatCode = new string[8] { "UI_Up", "UI_Up", "UI_Down", "UI_Down", "UI_Left", "UI_Right", "UI_Left", "UI_Right" };

	public bool FindCheatUI;

	public string CheatUIToDisplay = "Prefabs/PerformanceTestUI";

	public new string SendMessage = "";

	private int index;

	private float downTime;

	private float upTime;

	private float pressTime;

	private float countDown = 10f;

	private GameObject CheatPanel;

	public static bool UpdateSkeletonAnimation = true;

	private Player player
	{
		get
		{
			try
			{
				return RewiredInputManager.MainPlayer;
			}
			catch
			{
				return null;
			}
		}
	}
}
