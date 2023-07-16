using Unify;
using UnityEngine;

namespace Lamb.UI
{
	public class SettingsTabNavigatorBase : MMTabNavigatorBase<SettingsTab>
	{
		[SerializeField]
		private SettingsTab _graphicsTab;

		private void Awake()
		{
			UnifyManager.Platform platform = UnifyManager.platform;
			if (platform != UnifyManager.Platform.Standalone && platform != 0)
			{
				_graphicsTab.gameObject.SetActive(false);
			}
		}
	}
}
