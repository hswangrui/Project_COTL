using System;
using UnityEngine;

public class CompleteSozoQuest : MonoBehaviour
{
	public Structure Structure;

	private void OnEnable()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnDisable()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		if (!DataManager.Instance.BuiltMushroomDecoration)
		{
			DataManager.Instance.BuiltMushroomDecoration = true;
			DataManager.Instance.SozoDecorationQuestActive = false;
			ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/VisitSozo", Objectives.CustomQuestTypes.SozoReturn));
		}
	}
}
