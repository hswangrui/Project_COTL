using Lamb.UI;
using Lamb.UI.FollowerSelect;
using TMPro;
using UnityEngine;

namespace src.UI.Menus.CryptMenu
{
	public class UICryptMenuController : UIFollowerSelectBase<DeadFollowerInformationBox>
	{
		[SerializeField]
		private TMP_Text _descriptionText;

		[SerializeField]
		private TMP_Text _occupiedText;

		private int _slotLimit;

		public override bool AllowsVoting
		{
			get
			{
				return false;
			}
		}

		public void Configure(Interaction_Crypt crypt)
		{
			_slotLimit = crypt.structureBrain.DEAD_BODY_SLOTS;
		}

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			_descriptionText.text = string.Format(_descriptionText.text, _slotLimit);
			_occupiedText.text = string.Format(_occupiedText.text, _followerInfoBoxes.Count, _slotLimit);
			if (_followerInfoBoxes.Count == 0)
			{
				_controlPrompts.HideAcceptButton();
			}
		}

		protected override DeadFollowerInformationBox PrefabTemplate()
		{
			return MonoSingleton<UIManager>.Instance.DeadFollowerInformationBox;
		}
	}
}
