using System.Collections.Generic;

namespace Unify
{
	public class Activities
	{
		private static Activities singleton;

		public static Activities Instance => singleton;

		public static void Init()
		{
			Logger.Log("ACTIVITIES: Init");
			singleton = new Activities();
		}

		public Activities()
		{
			Logger.Log("ACTIVITIES: Platform does not support 'Activities': No Operations");
		}

		public virtual void UpdateActivityAvailablility(List<string> available, List<string> unavailable)
		{
			Logger.Log("ACTIVITIES: UpdateActivityAvailablility: ");
			Logger.Log("ACTIVITIES: Now Available: " + string.Join(", ", available.ToArray()));
			Logger.Log("ACTIVITIES: Now Unavailable: " + string.Join(", ", unavailable.ToArray()));
		}

		public virtual void ActivityStart(string id)
		{
			Logger.Log("ACTIVITIES: ActivityStart: " + id);
		}

		public virtual void ActivityComplete(string id)
		{
			Logger.Log("ACTIVITIES: ActivityComplete: " + id);
		}

		public virtual void ActivityAbandon(string id)
		{
			Logger.Log("ACTIVITIES: ActivityAbandon: " + id);
		}

		public virtual void ActivityFailed(string id)
		{
			Logger.Log("ACTIVITIES: ActivityFailed: " + id);
		}

		public virtual void ActivityTerminate()
		{
			Logger.Log("ACTIVITIES: ActivityTerminate");
		}

		public virtual void ActivityResume(string id)
		{
			Logger.Log("ACTIVITIES: ActivityResume: " + id);
		}
	}
}
