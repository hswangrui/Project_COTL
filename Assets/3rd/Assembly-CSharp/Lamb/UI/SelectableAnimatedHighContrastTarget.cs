using Lamb.UI.Assets;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class SelectableAnimatedHighContrastTarget : HighContrastTarget
	{
		private Selectable _selectable;

		public SelectableAnimatedHighContrastTarget(Selectable selectable, HighContrastConfiguration configuration)
			: base(configuration)
		{
			_selectable = selectable;
		}

		public override void Apply(bool state)
		{
		}

		public override void Init()
		{
		}
	}
}
