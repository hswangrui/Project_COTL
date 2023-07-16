namespace Lamb.UI.BuildMenu
{
	public class BuildMenuTabNavigatorBase : MMTabNavigatorBase<BuildMenuTab>
	{
		private static int _persistentTabIndex = -1;

		public override void ShowDefault()
		{
			if (ObjectiveManager.GroupExists("Objectives/GroupTitles/RepairTheShrine") || ObjectiveManager.GroupExists("Objectives/GroupTitles/Temple"))
			{
				SetDefaultTab(_tabs[1]);
				return;
			}
			if (_persistentTabIndex != -1)
			{
				_defaultTabIndex = _persistentTabIndex;
			}
			base.ShowDefault();
		}

		public void RemoveAllAlerts()
		{
			BuildMenuTab[] tabs = _tabs;
			for (int i = 0; i < tabs.Length; i++)
			{
				tabs[i].Alert.gameObject.SetActive(false);
			}
		}

		protected override void OnMenuHide()
		{
			base.OnMenuHide();
			_persistentTabIndex = base.CurrentMenuIndex;
		}

		public void ClearPersistentTab()
		{
			_persistentTabIndex = -1;
		}
	}
}
