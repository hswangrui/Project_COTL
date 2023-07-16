using System;
using UnityEngine;

public class DecorationPathObject : BaseMonoBehaviour
{
	[SerializeField]
	private Structure structure;

	private void Awake()
	{
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		PathTileManager.Instance.SetTile(structure.Type, base.transform.position);
		structure.OnProgressCompleted.RemoveListener(OnBrainAssigned);
		Invoke("RemoveStruct", 0.01f);
	}

	private void RemoveStruct()
	{
		structure.RemoveStructure();
	}
}
