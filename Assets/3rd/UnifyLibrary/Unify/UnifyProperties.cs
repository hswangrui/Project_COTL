using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unify
{
	[Serializable]
	public class UnifyProperties
	{
		[Serializable]
		public class CustomProperty
		{
			public string name;

			public string value;

			public string description;
		}

		public List<CustomProperty> properties;

		public Dictionary<string, string> dictionary;

		private static UnifyProperties _staticProperties;

		public static UnifyProperties LoadFromJson()
		{
			if (_staticProperties == null)
			{
				try
				{
					_staticProperties = JsonUtility.FromJson<UnifyProperties>(Resources.Load<TextAsset>("properties").text);
					_staticProperties.dictionary = new Dictionary<string, string>();
					foreach (CustomProperty property in _staticProperties.properties)
					{
						_staticProperties.dictionary.Add(property.name, property.value);
					}
					Logger.Log("UNIFYPROPERTIES: Loaded " + _staticProperties.properties.Count + " property entries");
				}
				catch (Exception ex)
				{
					Logger.LogError("Exception loading properties.json file: " + ex);
				}
			}
			return _staticProperties;
		}
	}
}
