using System.Collections.Generic;
using UnityEngine;

public class HealingBay : BaseMonoBehaviour
{
	public static List<HealingBay> HealingBays = new List<HealingBay>();

	public Structure Structure;

	public Transform HealingBayLocation;

	public GameObject HealingBayExitLocation;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	private void OnEnable()
	{
		HealingBays.Add(this);
	}

	private void OnDisable()
	{
		HealingBays.Remove(this);
	}
}
