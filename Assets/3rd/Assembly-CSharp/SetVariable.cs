using System;
using System.Collections.Generic;
using UnityEngine;

public class SetVariable : MonoBehaviour
{
	[Serializable]
	public class VariableAndCondition
	{
		public DataManager.Variables Variable;

		public bool Condition = true;
	}

	public List<VariableAndCondition> SetConditions = new List<VariableAndCondition>();

	public void Set()
	{
		foreach (VariableAndCondition setCondition in SetConditions)
		{
			if (DataManager.Instance.GetVariable(setCondition.Variable) != setCondition.Condition)
			{
				DataManager.Instance.SetVariable(setCondition.Variable, setCondition.Condition);
				break;
			}
		}
	}
}
