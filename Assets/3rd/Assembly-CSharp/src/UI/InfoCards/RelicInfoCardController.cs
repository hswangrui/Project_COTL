using Lamb.UI;
using src.UI.Items;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class RelicInfoCardController : UIInfoCardController<RelicInfoCard, RelicData>
	{
		protected override bool IsSelectionValid(Selectable selectable, out RelicData showParam)
		{
			RelicItem component;
			if (selectable.TryGetComponent<RelicItem>(out component) && !component.Locked)
			{
				showParam = component.Data;
				return true;
			}
			RelicPlayerMenuItem component2;
			if (selectable.TryGetComponent<RelicPlayerMenuItem>(out component2))
			{
				showParam = component2.Data;
				return true;
			}
			ActiveRelicItem component3;
			if (selectable.TryGetComponent<ActiveRelicItem>(out component3))
			{
				showParam = component3.RelicData;
				return true;
			}
			showParam = null;
			return false;
		}

		protected override RelicData DefaultShowParam()
		{
			return ScriptableObject.CreateInstance<RelicData>();
		}
	}
}
