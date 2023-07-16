using System;
using System.Collections;
using Unify.Input;
using UnityEngine;

public class CheatChainRunner : MonoBehaviour
{
	private CheatConsole cheatConsole;

	private int i;

	public void RunChain(string[] CheatChain, float[] timer)
	{
		cheatConsole = UnityEngine.Object.FindObjectOfType<CheatConsole>();
		StartCoroutine(ChainRun(CheatChain, timer));
	}

	public void RunChainForEver(string[] CheatChain, float timer)
	{
		cheatConsole = UnityEngine.Object.FindObjectOfType<CheatConsole>();
		StartCoroutine(ChainRun(CheatChain, timer));
	}

	private IEnumerator ChainRun(string[] CheatChain, float[] timer)
	{
		for (i = 0; i < CheatChain.Length; i++)
		{
			while (cheatConsole == null)
			{
				cheatConsole = UnityEngine.Object.FindObjectOfType<CheatConsole>();
				yield return new WaitForSecondsRealtime(1f);
			}
			Action value;
			if (cheatConsole.Cheats.TryGetValue(CheatChain[i], out value))
			{
				Debug.Log("running Cheat " + CheatChain[i]);
				value();
			}
			yield return new WaitForSecondsRealtime(timer[i]);
		}
		UnityEngine.Object.Destroy(this);
	}

	private IEnumerator ChainRun(string[] CheatChain, float timer)
	{
		for (i = 0; i <= CheatChain.Length; i++)
		{
			if (i == CheatChain.Length)
			{
				i = 0;
			}
			while (cheatConsole == null)
			{
				cheatConsole = UnityEngine.Object.FindObjectOfType<CheatConsole>();
				yield return new WaitForSecondsRealtime(1f);
			}
			Action value;
			if (cheatConsole.Cheats.TryGetValue(CheatChain[i], out value))
			{
				Debug.Log("running Cheat " + CheatChain[i]);
				value();
			}
			yield return new WaitForSecondsRealtime(timer);
		}
	}

	public void Update()
	{
		if (RewiredInputManager.MainPlayer.GetButtonDown("Bleat"))
		{
			UnityEngine.Object.Destroy(this);
		}
	}
}
