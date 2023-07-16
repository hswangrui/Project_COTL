using System.Collections.Generic;
using I2.Loc;

namespace Lamb.UI.FollowerInteractionWheel
{
	public class CommandItem
	{
		public FollowerCommands Command;

		public List<CommandItem> SubCommands;

		public virtual string GetTitle(Follower follower)
		{
			return LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}", Command));
		}

		public virtual string GetDescription(Follower follower)
		{
			return LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}/Description", Command));
		}

		public virtual string GetLockedDescription(Follower follower)
		{
			return LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}/NotAvailable", Command));
		}

		public virtual bool IsAvailable(Follower follower)
		{
			return true;
		}
	}
}
