using System.Collections.Generic;
using UnityEngine;

namespace RedBlueGames.Tools.TextTyper
{
	[CreateAssetMenu(fileName = "CurveLibrary", menuName = "Text Typer/Curve Library", order = 1)]
	public class CurveLibrary : ScriptableObject
	{
		public List<CurvePreset> CurvePresets;

		public CurvePreset this[string key]
		{
			get
			{
				CurvePreset curvePreset = FindPresetOrNull(key);
				if (curvePreset == null)
				{
					throw new KeyNotFoundException();
				}
				return curvePreset;
			}
		}

		public bool ContainsKey(string key)
		{
			return FindPresetOrNull(key) != null;
		}

		private CurvePreset FindPresetOrNull(string key)
		{
			foreach (CurvePreset curvePreset in CurvePresets)
			{
				if (curvePreset.Name.ToUpper() == key.ToUpper())
				{
					return curvePreset;
				}
			}
			return null;
		}
	}
}
