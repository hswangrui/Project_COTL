using System;
using UnityEngine;

public class AutoCheat : MonoBehaviour
{
	[SerializeField]
	private string[] _cheats;

	public void Start()
	{
		CheatConsole cheatConsole = UnityEngine.Object.FindObjectOfType<CheatConsole>();
		string[] cheats = _cheats;
		foreach (string key in cheats)
		{
			Action value;
			if (cheatConsole.Cheats.TryGetValue(key, out value))
			{
				value();
			}
		}
	}
}
