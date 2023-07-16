using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class BaseMonoBehaviour : MonoBehaviour
{
	private void OnDestroy()
	{
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			Type fieldType = fieldInfo.FieldType;
			if (typeof(IList).IsAssignableFrom(fieldType))
			{
				IList list = fieldInfo.GetValue(this) as IList;
				if (list != null)
				{
					list.Clear();
				}
			}
			if (typeof(IDictionary).IsAssignableFrom(fieldType))
			{
				IDictionary dictionary = fieldInfo.GetValue(this) as IDictionary;
				if (dictionary != null)
				{
					dictionary.Clear();
				}
			}
			if (!fieldType.IsPrimitive)
			{
				fieldInfo.SetValue(this, null);
			}
		}
	}
}
