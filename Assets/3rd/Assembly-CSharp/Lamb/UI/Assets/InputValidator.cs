using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Lamb.UI.Assets
{
	[Serializable]
	[CreateAssetMenu(fileName = "InputValidator", menuName = "Massive Monster/InputValidator", order = 100)]
	public class InputValidator : TMP_InputValidator
	{
		private int _maxCharacters = 14;

		public void OverrideMaxCharacters(int maxCharacters)
		{
			_maxCharacters = maxCharacters;
		}

		public override char Validate(ref string text, ref int pos, char ch)
		{
			if (text.Length >= _maxCharacters)
			{
				return ch;
			}
			string pattern = "[^\\p{Cc}\\p{Cn}\\p{Cs}]";
			if (Regex.Match(ch.ToString(), pattern, RegexOptions.IgnoreCase).Success)
			{
				pos++;
				text += ch;
				return ch;
			}
			return '\0';
		}
	}
}
