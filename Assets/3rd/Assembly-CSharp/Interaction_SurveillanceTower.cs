using System;
using I2.Loc;
using UnityEngine;

public class Interaction_SurveillanceTower : Interaction
{
	public GameObject SurveillanceScreenPrefab;

	private Structure Structure;

	private string sSurveillance;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		ActivateDistance = 2f;
		Structure = GetComponent<Structure>();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
	}

	private void OnStructureRemoved(StructuresData structure)
	{
		if (structure.Type == StructureBrain.TYPES.SURVEILLANCE)
		{
			DataManager.Instance.HasBuiltSurveillance = false;
		}
	}

	private void OnBrainAssigned()
	{
		DataManager.Instance.HasBuiltSurveillance = true;
	}

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sSurveillance = ScriptLocalization.Interactions.Surveillance;
	}

	public override void GetLabel()
	{
		base.Label = sSurveillance;
	}
}
