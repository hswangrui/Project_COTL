using System.Collections.Generic;

namespace Unify.PS5
{
	public class Activities : Unify.Activities
	{
		private HashSet<string> availableSaved;

		private HashSet<string> unavailableSaved;

		public override void UpdateActivityAvailablility(List<string> available, List<string> unavailable)
		{
			bool flag = true;
			if (availableSaved != null && unavailableSaved != null && availableSaved.SetEquals(available) && unavailableSaved.SetEquals(unavailable))
			{
				flag = false;
			}
		}

		public override void ActivityStart(string id)
		{
		}

		public override void ActivityResume(string id)
		{
		}

		public override void ActivityComplete(string id)
		{
		}

		public override void ActivityAbandon(string id)
		{
		}

		public override void ActivityFailed(string id)
		{
		}

		public override void ActivityTerminate()
		{
		}
	}
}
