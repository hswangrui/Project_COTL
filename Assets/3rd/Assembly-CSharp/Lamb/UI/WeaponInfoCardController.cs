using UnityEngine.UI;

namespace Lamb.UI
{
	public class WeaponInfoCardController : UIInfoCardController<WeaponInfoCard, EquipmentType>
	{
		protected override bool IsSelectionValid(Selectable selectable, out EquipmentType showParam)
		{
			showParam = EquipmentType.Invalid;
			WeaponItem component;
			if (selectable.TryGetComponent<WeaponItem>(out component))
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
