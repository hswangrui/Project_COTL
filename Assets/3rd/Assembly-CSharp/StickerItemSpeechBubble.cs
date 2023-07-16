using System;
using Lamb.UI;
using src.UINavigator;
using UnityEngine;

public class StickerItemSpeechBubble : StickerItem
{
	[SerializeField]
	private MMInputField inputField;

	public override void OnStickerPlaced()
	{
		base.OnStickerPlaced();
		_editOverlay.CancelSticker();
		_editOverlay.DisableAllInputs();
		MMInputField mMInputField = inputField;
		mMInputField.OnStartedEditing = (Action)Delegate.Combine(mMInputField.OnStartedEditing, new Action(OnStartedEditing));
		MMInputField mMInputField2 = inputField;
		mMInputField2.OnEndedEditing = (Action<string>)Delegate.Combine(mMInputField2.OnEndedEditing, new Action<string>(OnEndedEditing));
		inputField.gameObject.SetActive(true);
		inputField.Interactable = true;
		MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(inputField);
		inputField.TryPerformConfirmAction();
	}

	private void OnStartedEditing()
	{
	}

	private void OnEndedEditing(string s)
	{
		inputField.text = s;
		inputField.Interactable = false;
		MMInputField mMInputField = inputField;
		mMInputField.OnStartedEditing = (Action)Delegate.Remove(mMInputField.OnStartedEditing, new Action(OnStartedEditing));
		MMInputField mMInputField2 = inputField;
		mMInputField2.OnEndedEditing = (Action<string>)Delegate.Remove(mMInputField2.OnEndedEditing, new Action<string>(OnEndedEditing));
		_editOverlay.EnableAllInputs();
	}

	public override void Flip()
	{
		base.Flip();
		Vector3 localScale = inputField.transform.localScale;
		localScale.x *= -1f;
		inputField.transform.localScale = localScale;
	}
}
