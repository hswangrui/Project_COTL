using System.Collections.Generic;
using UnityEngine;

public class SimpleDeleteOnCondiiton : BaseMonoBehaviour
{
	public List<Interaction_SimpleConversation.VariableAndCondition> DeleteConditions = new List<Interaction_SimpleConversation.VariableAndCondition>();

	public List<GameObject> ObjectsToDelete = new List<GameObject>();

	public List<Interaction_SimpleConversation.VariableAndCondition> SetConditions = new List<Interaction_SimpleConversation.VariableAndCondition>();

	public bool DoInAwake;

	private void Awake()
	{
		if (DoInAwake)
		{
			Start();
		}
	}

	private void Start()
	{
		if (DeleteConditions.Count <= 0)
		{
			return;
		}
		bool flag = true;
		foreach (Interaction_SimpleConversation.VariableAndCondition deleteCondition in DeleteConditions)
		{
			if (DataManager.Instance.GetVariable(deleteCondition.Variable) != deleteCondition.Condition)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (GameObject item in ObjectsToDelete)
		{
			Object.Destroy(item);
		}
	}

	public void Play()
	{
		foreach (Interaction_SimpleConversation.VariableAndCondition setCondition in SetConditions)
		{
			DataManager.Instance.SetVariable(setCondition.Variable, setCondition.Condition);
		}
	}
}
