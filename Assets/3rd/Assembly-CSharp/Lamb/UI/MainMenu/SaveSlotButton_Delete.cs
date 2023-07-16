namespace Lamb.UI.MainMenu
{
	public class SaveSlotButton_Delete : SaveSlotButtonBase
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
			_text.text = "---";
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
			_button.enabled = false;
			_button.interactable = false;
			_completionBadge.SetActive(false);
			_sandboxCompletionBadge.SetActive(false);
		}
	}
}
