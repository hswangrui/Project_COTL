using System;

namespace src.Data
{
	[Serializable]
	public class TwitchSettings
	{
		public bool HelpHinderEnabled = true;

		public float HelpHinderFrequency = 20f;

		public bool TotemEnabled = true;

		public bool FollowerNamesEnabled = true;

		public bool TwitchMessagesEnabled = true;

		public TwitchSettings()
		{
		}

		public TwitchSettings(TwitchSettings twitchSettings)
		{
			HelpHinderEnabled = twitchSettings.HelpHinderEnabled;
			HelpHinderFrequency = twitchSettings.HelpHinderFrequency;
			TotemEnabled = twitchSettings.TotemEnabled;
			FollowerNamesEnabled = twitchSettings.FollowerNamesEnabled;
			TwitchMessagesEnabled = twitchSettings.TwitchMessagesEnabled;
		}
	}
}
