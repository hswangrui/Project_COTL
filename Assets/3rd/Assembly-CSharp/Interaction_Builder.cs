using System.Collections.Generic;
using UnityEngine;

public class Interaction_Builder : Interaction
{
	private List<StructuresData> StructuresList = new List<StructuresData>();

	private Structure structure;

	private void Start()
	{
		structure = GetComponent<Structure>();
	}

	public override void OnInteract(StateMachine state)
	{
		GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasMenuList>();
	}
}
