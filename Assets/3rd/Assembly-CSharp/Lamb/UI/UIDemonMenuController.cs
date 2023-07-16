using Lamb.UI.FollowerSelect;

namespace Lamb.UI
{
	public class UIDemonMenuController : UIFollowerSelectBase<DemonFollowerItem>
	{
		public override bool AllowsVoting
		{
			get
			{
				return false;
			}
		}

		protected override DemonFollowerItem PrefabTemplate()
		{
			return MonoSingleton<UIManager>.Instance.DemonFollowerItemTemplate;
		}
	}
}
