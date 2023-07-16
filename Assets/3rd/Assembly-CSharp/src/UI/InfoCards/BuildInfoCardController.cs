using Lamb.UI;
using Lamb.UI.BuildMenu;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class BuildInfoCardController : UIInfoCardController<BuildInfoCard, StructureBrain.TYPES>
	{
		protected override bool IsSelectionValid(Selectable selectable, out StructureBrain.TYPES showParam)
		{
			showParam = StructureBrain.TYPES.NONE;
			BuildMenuItem component;
			if (selectable.TryGetComponent<BuildMenuItem>(out component) && !component.Locked)
			{
				showParam = component.Structure;
				return true;
			}
			return false;
		}

		protected override StructureBrain.TYPES DefaultShowParam()
		{
			return StructureBrain.TYPES.NONE;
		}
	}
}
