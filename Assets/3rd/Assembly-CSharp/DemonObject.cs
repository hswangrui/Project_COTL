using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DemonObject
{
	private List<string> Paths = new List<string> { "Prefabs/Units/Demons/Demon_Shooty", "Prefabs/Units/Demons/Demon_Arrows", "Prefabs/Units/Demons/Demon_Chomp" };

	public string FilePath;

	private int RandomDemon;

	public void SetRandomDemon()
	{
		RandomDemon = UnityEngine.Random.Range(0, Paths.Count);
		FilePath = Paths[RandomDemon];
	}
}
