using I2.Loc;

namespace Lamb.UI.MainMenu
{
	public class SaveSlotButton_Load : SaveSlotButtonBase
	{
		protected override void LocalizeOccupied()
		{
			if (_metaData.HasValue)
			{
				_text.text = _metaData.Value.ToString();
			}
		}

		protected override void LocalizeEmpty()
		{
			_text.text = ScriptLocalization.UI_MainMenu.NewSave;
		}

		protected override void SetupOccupiedSlot()
		{
			if (_metaData.HasValue)
			{
				_completionBadge.SetActive(_metaData.Value.GameBeaten && !_metaData.Value.SandboxBeaten);
				_sandboxCompletionBadge.SetActive(_metaData.Value.SandboxBeaten);
				_button.interactable = true;
			}
		}

		protected override void SetupEmptySlot()
		{
			_button.interactable = true;
			_completionBadge.SetActive(false);
			_sandboxCompletionBadge.SetActive(false);
		}
	}
}
