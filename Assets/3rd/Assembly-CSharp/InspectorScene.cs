using System;
using UnityEngine;

[Serializable]
public class InspectorScene : IEquatable<InspectorScene>
{
	[SerializeField]
	public string SceneName;

	public bool Equals(InspectorScene other)
	{
		return SceneName.Equals(other.SceneName);
	}
}
