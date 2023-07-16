using System.Collections.Generic;
using UnityEngine;

namespace RedBlueGames.Tools.TextTyper
{
	[CreateAssetMenu(fileName = "ShakeLibrary", menuName = "Text Typer/Shake Library", order = 1)]
	public class ShakeLibrary : ScriptableObject
	{
		public List<ShakePreset> ShakePresets;

		public ShakePreset this[string key]
		{
			get
			{
				ShakePreset shakePreset = FindPresetOrNull(key);
				if (shakePreset == null)
				{
					throw new KeyNotFoundException();
				}
				return shakePreset;
			}
		}

		public bool ContainsKey(string key)
		{
			return FindPresetOrNull(key) != null;
		}

		private ShakePreset FindPresetOrNull(string key)
		{
			foreach (ShakePreset shakePreset in ShakePresets)
			{
				if (shakePreset.Name.ToUpper() == key.ToUpper())
				{
					return shakePreset;
				}
			}
			return null;
		}
	}
}
