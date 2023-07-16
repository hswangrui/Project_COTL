using Lamb.UI.Assets;

namespace Lamb.UI
{
	public abstract class HighContrastTarget
	{
		protected HighContrastConfiguration _configuration;

		public HighContrastTarget(HighContrastConfiguration configuration)
		{
			_configuration = configuration;
		}

		public abstract void Apply(bool state);

		public abstract void Init();
	}
}
