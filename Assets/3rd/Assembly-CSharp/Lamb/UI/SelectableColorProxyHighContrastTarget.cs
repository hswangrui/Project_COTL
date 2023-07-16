using Lamb.UI.Assets;

namespace Lamb.UI
{
	public class SelectableColorProxyHighContrastTarget : HighContrastTarget
	{
		private SelectableColourProxy _colourProxy;

		private HighContrastConfiguration.HighContrastColorSet _cachedColorSet;

		public SelectableColorProxyHighContrastTarget(SelectableColourProxy colourProxy, HighContrastConfiguration configuration)
			: base(configuration)
		{
			_colourProxy = colourProxy;
		}

		public override void Apply(bool state)
		{
			if (state)
			{
				_configuration.ColorTransitionSet.Apply(_colourProxy);
			}
			else
			{
				_cachedColorSet.Apply(_colourProxy);
			}
		}

		public override void Init()
		{
			_cachedColorSet = new HighContrastConfiguration.HighContrastColorSet(_colourProxy);
		}
	}
}
