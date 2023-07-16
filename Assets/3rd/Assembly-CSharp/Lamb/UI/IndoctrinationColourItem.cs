using System;

namespace Lamb.UI
{
	public class IndoctrinationColourItem : IndoctrinationCharacterItem<IndoctrinationColourItem>
	{
		public void Configure(int skin, int colour, WorshipperData.SkinAndData skinAndData)
		{
			_skinAndData = skinAndData;
			Configure(skin, colour);
		}

		public void Configure(int skin, int colour)
		{
			_spine.ConfigureFollowerSkin(_skinAndData, skin, colour);
		}

		protected override void OnButtonClickedImpl()
		{
			Action<IndoctrinationColourItem> onItemSelected = OnItemSelected;
			if (onItemSelected != null)
			{
				onItemSelected(this);
			}
		}
	}
}
