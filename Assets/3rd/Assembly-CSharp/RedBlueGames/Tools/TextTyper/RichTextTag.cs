namespace RedBlueGames.Tools.TextTyper
{
	public class RichTextTag
	{
		public static readonly RichTextTag ClearColorTag = new RichTextTag("<color=#00000000>");

		private const char OpeningNodeDelimeter = '<';

		private const char CloseNodeDelimeter = '>';

		private const char EndTagDelimeter = '/';

		private const string ParameterDelimeter = "=";

		public string TagText { get; private set; }

		public string ClosingTagText
		{
			get
			{
				if (!IsClosingTag)
				{
					return $"</{TagType}>";
				}
				return TagText;
			}
		}

		public string TagType
		{
			get
			{
				string text = TagText.Substring(1, TagText.Length - 2);
				text = text.TrimStart('/');
				int num = text.IndexOf("=");
				if (num > 0)
				{
					text = text.Substring(0, num);
				}
				return text;
			}
		}

		public string Parameter
		{
			get
			{
				int num = TagText.IndexOf("=");
				if (num < 0)
				{
					return string.Empty;
				}
				int length = TagText.Length - num - 2;
				string text = TagText.Substring(num + 1, length);
				if (text.Length > 0 && text[0] == '"' && text[text.Length - 1] == '"')
				{
					text = text.Substring(1, text.Length - 2);
				}
				return text;
			}
		}

		public bool IsOpeningTag => !IsClosingTag;

		public bool IsClosingTag
		{
			get
			{
				if (TagText.Length > 2)
				{
					return TagText[1] == '/';
				}
				return false;
			}
		}

		public int Length => TagText.Length;

		public RichTextTag(string tagText)
		{
			TagText = tagText;
		}

		public static bool StringStartsWithTag(string text)
		{
			return text.StartsWith('<'.ToString());
		}

		public static RichTextTag ParseNext(string text)
		{
			int num = text.IndexOf('<');
			if (num < 0)
			{
				return null;
			}
			int num2 = text.IndexOf('>');
			if (num2 < 0)
			{
				return null;
			}
			return new RichTextTag(text.Substring(num, num2 - num + 1));
		}

		public static string RemoveTagsFromString(string text, string tagType)
		{
			string text2 = text;
			for (int i = 0; i < text.Length; i++)
			{
				string text3 = text.Substring(i, text.Length - i);
				if (StringStartsWithTag(text3))
				{
					RichTextTag richTextTag = ParseNext(text3);
					if (richTextTag.TagType == tagType)
					{
						text2 = text2.Replace(richTextTag.TagText, string.Empty);
					}
					i += richTextTag.Length - 1;
				}
			}
			return text2;
		}

		public override string ToString()
		{
			return TagText;
		}
	}
}
