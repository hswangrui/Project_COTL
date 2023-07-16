using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RedBlueGames.Tools.TextTyper
{
	public sealed class TextTagParser
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct CustomTags
		{
			public const string Delay = "delay";

			public const string Anim = "anim";

			public const string Animation = "animation";
		}

		public class TextSymbol
		{
			public char Character { get; private set; }

			public RichTextTag Tag { get; private set; }

			public int Length => Text.Length;

			public string Text
			{
				get
				{
					if (IsTag)
					{
						return Tag.TagText;
					}
					return Character.ToString();
				}
			}

			public bool IsTag => Tag != null;

			public TextSymbol(string character)
			{
				Character = character[0];
			}

			public TextSymbol(RichTextTag tag)
			{
				Tag = tag;
			}

			public float GetFloatParameter(float defaultValue = 0f)
			{
				if (!IsTag)
				{
					Debug.LogWarning("Attempted to retrieve parameter from symbol that is not a tag.");
					return defaultValue;
				}
				if (!float.TryParse(Tag.Parameter, out var result))
				{
					Debug.LogWarning($"Found Invalid parameter format in tag [{Tag}]. Parameter [{Tag.Parameter}] does not parse to a float.");
					return defaultValue;
				}
				return result;
			}
		}

		private static readonly string[] UnityTagTypes = new string[5] { "b", "i", "size", "color", "style" };

		private static readonly string[] CustomTagTypes = new string[3] { "delay", "anim", "animation" };

		public static List<TextSymbol> CreateSymbolListFromText(string text)
		{
			List<TextSymbol> list = new List<TextSymbol>();
			int num = 0;
			while (num < text.Length)
			{
				TextSymbol textSymbol = null;
				string text2 = text.Substring(num, text.Length - num);
				textSymbol = ((!RichTextTag.StringStartsWithTag(text2)) ? new TextSymbol(text2.Substring(0, 1)) : new TextSymbol(RichTextTag.ParseNext(text2)));
				num += textSymbol.Length;
				list.Add(textSymbol);
			}
			return list;
		}

		public static string RemoveAllTags(string textWithTags)
		{
			return RemoveCustomTags(RemoveUnityTags(textWithTags));
		}

		public static string RemoveCustomTags(string textWithTags)
		{
			return RemoveTags(textWithTags, CustomTagTypes);
		}

		public static string RemoveUnityTags(string textWithTags)
		{
			return RemoveTags(textWithTags, UnityTagTypes);
		}

		private static string RemoveTags(string textWithTags, params string[] tags)
		{
			string text = textWithTags;
			foreach (string tagType in tags)
			{
				text = RichTextTag.RemoveTagsFromString(text, tagType);
			}
			return text;
		}
	}
}
