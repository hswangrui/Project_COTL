using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI.BuildMenu
{
	public class FaithCategory : BuildMenuCategory
	{
		[Header("Content")]
		[SerializeField]
		private RectTransform _content;

		[Header("Counts")]
		[SerializeField]
		private TextMeshProUGUI _unlocked;

		protected override void Populate()
		{
			Populate(AllStructures(), _content);
			SetUnlockedText(_unlocked);
		}

		private void SetUnlockedText(TextMeshProUGUI target)
		{
			int num = 0;
			List<StructureBrain.TYPES> list = AllStructures();
			foreach (StructureBrain.TYPES item in list)
			{
				if (StructuresData.GetUnlocked(item))
				{
					num++;
				}
			}
			target.text = string.Format(ScriptLocalization.UI.Collected, string.Format("{0}/{1}", num, list.Count));
		}

		public static List<StructureBrain.TYPES> AllStructures()
		{
			return new List<StructureBrain.TYPES>
			{
				StructureBrain.TYPES.SHRINE,
				StructureBrain.TYPES.TEMPLE,
				StructureBrain.TYPES.CONFESSION_BOOTH,
				StructureBrain.TYPES.MISSIONARY,
				StructureBrain.TYPES.MISSIONARY_II,
				StructureBrain.TYPES.MISSIONARY_III,
				StructureBrain.TYPES.SHRINE_PASSIVE,
				StructureBrain.TYPES.SHRINE_PASSIVE_II,
				StructureBrain.TYPES.SHRINE_PASSIVE_III,
				StructureBrain.TYPES.OFFERING_STATUE
			};
		}
	}
}
