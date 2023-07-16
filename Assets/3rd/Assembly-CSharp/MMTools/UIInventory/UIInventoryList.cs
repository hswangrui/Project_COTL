using System.Collections.Generic;
using UnityEngine;

namespace MMTools.UIInventory
{
	public class UIInventoryList : BaseMonoBehaviour
	{
		public List<UIInventoryItem> Items = new List<UIInventoryItem>();

		public Vector2Int GridSize = new Vector2Int(2, 2);

		public Vector2 Padding = new Vector2(100f, 50f);

		public UIInventoryItem ListObject;

		public Transform Container;

		public void PopulateList(List<InventoryItem> ItemsToPopulate)
		{
			int num = -1;
			while (++num < Items.Count)
			{
				if (num < ItemsToPopulate.Count)
				{
					Items[num].Init(ItemsToPopulate[num]);
				}
				else
				{
					Items[num].InitEmpty();
				}
			}
		}

		private void GenerateList()
		{
			ClearList();
			for (int i = 0; i < GridSize.y; i++)
			{
				for (int j = 0; j < GridSize.x; j++)
				{
				}
			}
		}

		private void ClearList()
		{
			foreach (UIInventoryItem item in Items)
			{
				if (item != null)
				{
					Object.DestroyImmediate(item.gameObject);
				}
			}
			Items = new List<UIInventoryItem>();
		}
	}
}
