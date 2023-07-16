using I2.Loc;
using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuControlPrompts : BaseMonoBehaviour
{
	[Header("Menu")]
	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private UIMenuBase _attachedMenu;

	[Header("Prompt Containers")]
	[SerializeField]
	private GameObject _acceptPromptContainer;

	[SerializeField]
	private GameObject _cancelPromptContainer;

	private void Start()
	{
		ForceRebuild();
	}

	private void OnCancelButtonClicked()
	{
		_attachedMenu.OnCancelButtonInput();
	}

	public void ShowAcceptButton()
	{
		if (_acceptPromptContainer != null)
		{
			_acceptPromptContainer.gameObject.SetActive(true);
		}
	}

	public void HideAcceptButton()
	{
		if (_acceptPromptContainer != null)
		{
			_acceptPromptContainer.gameObject.SetActive(false);
		}
	}

	public void ShowCancelButton()
	{
		if (_cancelPromptContainer != null)
		{
			_cancelPromptContainer.gameObject.SetActive(true);
		}
	}

	public void HideCancelButton()
	{
		if (_cancelPromptContainer != null)
		{
			_cancelPromptContainer.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += ForceRebuild;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= ForceRebuild;
	}

	private void ForceRebuild()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
	}
}
