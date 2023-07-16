using UnityEngine.UI;

namespace Lamb.UI
{
	public class CurseInfoCardController : UIInfoCardController<CurseInfoCard, EquipmentType>
	{
		protected override bool IsSelectionValid(Selectable selectable, out EquipmentType showParam)
		{
			showParam = EquipmentType.Invalid;
			CurseItem component;
			if (selectable.TryGetComponent<CurseItem>(out component))
			{
				showParam = component.Type;
				return true;
			}
			return false;
		}

		protected override EquipmentType DefaultShowParam()
		{
			return EquipmentType.Invalid;
		}
	}
}
