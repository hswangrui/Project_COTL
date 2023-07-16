using System;
using UnityEngine;

public class UnifyVersion : MonoBehaviour
{
	private static Version version;

	public static Version Version
	{
		get
		{
			return version;
		}
	}

	public static string GetVersionString()
	{
		return version.ToString();
	}

	static UnifyVersion()
	{
		TextAsset textAsset = (TextAsset)Resources.Load("version");
		if (textAsset != null)
		{
			version = new Version(textAsset.text);
		}
	}
}
