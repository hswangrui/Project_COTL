using Lamb.UI.FollowerSelect;

namespace Lamb.UI.Mission
{
	public class UIMissionMenuController : UIFollowerSelectBase<MissionaryFollowerItem>
	{
		public override bool AllowsVoting
		{
			get
			{
				return false;
			}
		}

		protected override MissionaryFollowerItem PrefabTemplate()
		{
			return MonoSingleton<UIManager>.Instance.MissionaryFollowerItemTemplate;
		}
	}
}
