namespace Unify
{
	public class LaunchIntent
	{
		public User user { get; set; }

		public string action { get; set; }

		public LaunchIntent(User _user, string _action)
		{
			user = _user;
			action = _action;
		}
	}
}
