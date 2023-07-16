using System;
using System.Collections.Generic;
using UnityEngine;

public class DeleteOnVariable : BaseMonoBehaviour
{
	[Serializable]
	public class VariableAndCondition
	{
		public DataManager.Variables Variable;

		public bool Condition = true;
	}

	public List<VariableAndCondition> DeleteConditions = new List<VariableAndCondition>();

	public bool justDeactive;

	private void OnEnable()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Combine(SaveAndLoad.OnLoadComplete, new Action(OnLoadComplete));
		Play();
	}

	private void OnDisable()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(OnLoadComplete));
	}

	private void OnLoadComplete()
	{
		SaveAndLoad.OnLoadComplete = (Action)Delegate.Remove(SaveAndLoad.OnLoadComplete, new Action(OnLoadComplete));
		Play();
	}

	private void Play()
	{
		bool flag = true;
		foreach (VariableAndCondition deleteCondition in DeleteConditions)
		{
			if (DataManager.Instance.GetVariable(deleteCondition.Variable) != deleteCondition.Condition)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (!justDeactive)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				base.gameObject.SetActive(false);
			}
		}
	}
}
