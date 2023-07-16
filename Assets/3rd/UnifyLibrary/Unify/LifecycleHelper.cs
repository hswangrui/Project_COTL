namespace Unify
{
	public class LifecycleHelper
	{
		private static LifecycleHelper instance;

		public static LifecycleHelper Instance => instance;

		public static void Init()
		{
			Logger.Log("LIFECYCLE: Init");
			instance = new LifecycleHelper();
		}

		public virtual void Destroy()
		{
			instance = null;
		}
	}
}
