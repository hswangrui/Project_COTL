using Lamb.UI.Assets;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class SelectableColorTransitionHighContrastTarget : HighContrastTarget
	{
		private Selectable _selectable;

		private HighContrastConfiguration.HighContrastColorSet _cachedColorSet;

		public SelectableColorTransitionHighContrastTarget(Selectable selectable, HighContrastConfiguration configuration)
			: base(configuration)
		{
			_selectable = selectable;
		}

		public override void Apply(bool state)
		{
			if (state)
			{
				_configuration.ColorTransitionSet.Apply(_selectable);
			}
			else
			{
				_cachedColorSet.Apply(_selectable);
			}
		}

		public override void Init()
		{
			_cachedColorSet = new HighContrastConfiguration.HighContrastColorSet(_selectable);
		}
	}
}
