using System;
using Lamb.UI.Alerts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lamb.UI
{
	public class IndoctrinationFormItem : IndoctrinationCharacterItem<IndoctrinationFormItem>, ISelectHandler, IEventSystemHandler
	{
		[SerializeField]
		private CharacterSkinAlert _alert;

		public override void Configure(WorshipperData.SkinAndData skinAndData)
		{
			base.Configure(skinAndData);
			_alert.Configure(skinAndData.Skin[0].Skin);
		}

		protected override void OnButtonClickedImpl()
		{
			Action<IndoctrinationFormItem> onItemSelected = OnItemSelected;
			if (onItemSelected != null)
			{
				onItemSelected(this);
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			_alert.TryRemoveAlert();
		}
	}
}
