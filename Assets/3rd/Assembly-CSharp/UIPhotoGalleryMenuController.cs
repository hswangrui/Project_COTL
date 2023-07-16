using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Data.ReadWrite;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using src.UI;
using TMPro;
using Unify;
using UnityEngine;

public class UIPhotoGalleryMenuController : UIMenuBase
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass23_0
	{
		public UIPhotoGalleryMenuController _003C_003E4__this;

		public string[] filepaths;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass23_1
	{
		public string file;

		public _003C_003Ec__DisplayClass23_0 CS_0024_003C_003E8__locals1;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass23_2
	{
		public PhotoModeCamera.PhotoReadWriteResult result;

		public _003C_003Ec__DisplayClass23_1 CS_0024_003C_003E8__locals2;

		internal void _003CLoadPhotos_003Eg__OnReadCompleted_007C0(Texture2D texture2D)
		{
			PhotoModeManager.PhotoData photoData = new PhotoModeManager.PhotoData
			{
				PhotoName = CS_0024_003C_003E8__locals2.file,
				PhotoTexture = texture2D
			};
			PhotoInformationBox item = CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.MakePhoto(photoData, CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1.filepaths.IndexOf(CS_0024_003C_003E8__locals2.file) != 0);
			CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.photoBoxes.Add(item);
			if (CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.photoBoxes.Count == 1)
			{
				CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.OverrideDefault(CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.photoBoxes[0].Button);
				CS_0024_003C_003E8__locals2.CS_0024_003C_003E8__locals1._003C_003E4__this.ActivateNavigation();
			}
			result = PhotoModeCamera.PhotoReadWriteResult.Success;
		}

		internal void _003CLoadPhotos_003Eg__OnReadError_007C1(MMReadWriteError readWriteError)
		{
			result = PhotoModeCamera.PhotoReadWriteResult.Error;
		}
	}

	[SerializeField]
	private Transform _contentContainer;

	[SerializeField]
	private Transform _CardcontentContainer;

	[SerializeField]
	private Transform noPhotos;

	[SerializeField]
	private GameObject locatePrompt;

	[SerializeField]
	private GameObject DeletePrompt;

	[SerializeField]
	private GameObject AcceptPrompt;

	[SerializeField]
	private GameObject SharePrompt;

	[SerializeField]
	private Localize SharePromptLocalize;

	[SerializeField]
	private MMScrollRect _scrollRect;

	[SerializeField]
	private GameObject _controlPrompts;

	[Header("Photo Counter")]
	[SerializeField]
	private GameObject _counterContainer;

	[SerializeField]
	private TMP_Text _photosTaken;

	private List<PhotoInformationBox> photoBoxes = new List<PhotoInformationBox>();

	private PhotoModeManager.PhotoData currentHoveredPhoto;

	private PhotoInformationBox _currentPhoto;

	private Coroutine _loadPhotosCoroutine;

	private bool PhotoIsSharing;

	private List<Texture2D> _texturesToDestroy = new List<Texture2D>();

	private float InputTimeOut;

	private float InputTimeOutTime = 1f;

	protected override void OnShowStarted()
	{
		base.OnShowStarted();
		locatePrompt.gameObject.SetActive(true);
		_counterContainer.SetActive(true);
		SharePrompt.gameObject.SetActive(false);
		AudioManager.Instance.PlayOneShot("event:/ui/open_menu");
		noPhotos.gameObject.SetActive(false);
		photoBoxes = new List<PhotoInformationBox>();
		_loadPhotosCoroutine = StartCoroutine(LoadPhotos());
		if (UnifyManager.platform == UnifyManager.Platform.PS4 || UnifyManager.platform == UnifyManager.Platform.PS5)
		{
			SharePromptLocalize.Term = "UI/PhotoMode/Share_PLAYSTATION";
		}
		else if (UnifyManager.platform == UnifyManager.Platform.XboxOne || UnifyManager.platform == UnifyManager.Platform.GameCore)
		{
			SharePromptLocalize.Term = "UI/PhotoMode/Share_XBOX";
		}
		else if (UnifyManager.platform == UnifyManager.Platform.Switch)
		{
			SharePromptLocalize.Term = "UI/PhotoMode/Share_SWITCH";
		}
		InputTimeOut = Time.unscaledTime;
	}

	protected override void OnPush()
	{
		base.OnPush();
		_controlPrompts.SetActive(false);
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		_controlPrompts.SetActive(true);
	}

	private IEnumerator LoadPhotos()
	{
		_003C_003Ec__DisplayClass23_0 _003C_003Ec__DisplayClass23_ = new _003C_003Ec__DisplayClass23_0();
		_003C_003Ec__DisplayClass23_._003C_003E4__this = this;
		UpdatePhotoText();
		_003C_003Ec__DisplayClass23_.filepaths = PhotoModeManager.ImageReadWriter.GetFiles();
		int errors = 0;
		DeletePrompt.gameObject.SetActive(_003C_003Ec__DisplayClass23_.filepaths.Length != 0);
		AcceptPrompt.gameObject.SetActive(_003C_003Ec__DisplayClass23_.filepaths.Length != 0);
		string[] filepaths = _003C_003Ec__DisplayClass23_.filepaths;
		for (int i = 0; i < filepaths.Length; i++)
		{
			_003C_003Ec__DisplayClass23_1 _003C_003Ec__DisplayClass23_2 = new _003C_003Ec__DisplayClass23_1();
			_003C_003Ec__DisplayClass23_2.CS_0024_003C_003E8__locals1 = _003C_003Ec__DisplayClass23_;
			_003C_003Ec__DisplayClass23_2.file = filepaths[i];
			_003C_003Ec__DisplayClass23_2 _003C_003Ec__DisplayClass23_3 = new _003C_003Ec__DisplayClass23_2();
			_003C_003Ec__DisplayClass23_3.CS_0024_003C_003E8__locals2 = _003C_003Ec__DisplayClass23_2;
			if (!string.IsNullOrEmpty(_003C_003Ec__DisplayClass23_3.CS_0024_003C_003E8__locals2.file))
			{
				_003C_003Ec__DisplayClass23_3.result = PhotoModeCamera.PhotoReadWriteResult.None;
				COTLImageReadWriter imageReadWriter = PhotoModeManager.ImageReadWriter;
				imageReadWriter.OnReadCompleted = (Action<Texture2D>)Delegate.Combine(imageReadWriter.OnReadCompleted, new Action<Texture2D>(_003C_003Ec__DisplayClass23_3._003CLoadPhotos_003Eg__OnReadCompleted_007C0));
				COTLImageReadWriter imageReadWriter2 = PhotoModeManager.ImageReadWriter;
				imageReadWriter2.OnReadError = (Action<MMReadWriteError>)Delegate.Combine(imageReadWriter2.OnReadError, new Action<MMReadWriteError>(_003C_003Ec__DisplayClass23_3._003CLoadPhotos_003Eg__OnReadError_007C1));
				PhotoModeManager.ImageReadWriter.Read(_003C_003Ec__DisplayClass23_3.CS_0024_003C_003E8__locals2.file);
				while (_003C_003Ec__DisplayClass23_3.result == PhotoModeCamera.PhotoReadWriteResult.None)
				{
					yield return new WaitForEndOfFrame();
				}
				COTLImageReadWriter imageReadWriter3 = PhotoModeManager.ImageReadWriter;
				imageReadWriter3.OnReadCompleted = (Action<Texture2D>)Delegate.Remove(imageReadWriter3.OnReadCompleted, new Action<Texture2D>(_003C_003Ec__DisplayClass23_3._003CLoadPhotos_003Eg__OnReadCompleted_007C0));
				COTLImageReadWriter imageReadWriter4 = PhotoModeManager.ImageReadWriter;
				imageReadWriter4.OnReadError = (Action<MMReadWriteError>)Delegate.Remove(imageReadWriter4.OnReadError, new Action<MMReadWriteError>(_003C_003Ec__DisplayClass23_3._003CLoadPhotos_003Eg__OnReadError_007C1));
				if (_003C_003Ec__DisplayClass23_3.result == PhotoModeCamera.PhotoReadWriteResult.Error)
				{
					errors++;
				}
				yield return new WaitForEndOfFrame();
				UpdatePhotoText();
			}
		}
		if (errors > 0)
		{
			PhotoModeCamera.Instance.IsErrorShown = true;
			UIMenuConfirmationWindow uIMenuConfirmationWindow = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_PhotoMode_Error_Read.Heading, ScriptLocalization.UI_PhotoMode_Error_Read.Description, true);
			uIMenuConfirmationWindow.OnHide = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnHide, (Action)delegate
			{
				PhotoModeCamera.Instance.IsErrorShown = false;
			});
			yield return uIMenuConfirmationWindow.YieldUntilHidden();
		}
		noPhotos.gameObject.SetActive(photoBoxes.Count == 0);
		_CardcontentContainer.gameObject.SetActive(photoBoxes.Count > 0);
		UpdatePhotoText();
		_loadPhotosCoroutine = null;
	}

	private void Update()
	{
		if (PhotoModeCamera.Instance.IsErrorShown || PhotoIsSharing)
		{
			return;
		}
		if (currentHoveredPhoto != null && InputManager.PhotoMode.GetDeletePhotoButtonDown() && _canvasGroup.interactable && photoBoxes.Count > 0 && Time.unscaledTime - InputTimeOut > InputTimeOutTime)
		{
			InputTimeOut = Time.unscaledTime;
			UIMenuConfirmationWindow uIMenuConfirmationWindow = Push(MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate);
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_PhotoMode.Discard, ScriptLocalization.UI_PhotoMode_Discard.Description);
			uIMenuConfirmationWindow.OnConfirm = (Action)Delegate.Combine(uIMenuConfirmationWindow.OnConfirm, (Action)delegate
			{
				int num = photoBoxes.IndexOf(_currentPhoto);
				photoBoxes[num].Recycle();
				photoBoxes.RemoveAt(num);
				PhotoModeManager.DeletePhoto(currentHoveredPhoto.PhotoName);
				num = ((photoBoxes.Count <= num) ? (num - 1) : num);
				if (photoBoxes.Count > num && num > 0)
				{
					OverrideDefault(photoBoxes[num].Button);
					ActivateNavigation();
				}
				else if (photoBoxes.Count > 0)
				{
					OverrideDefault(photoBoxes[0].Button);
					ActivateNavigation();
				}
				noPhotos.gameObject.SetActive(photoBoxes.Count == 0);
				_CardcontentContainer.gameObject.SetActive(photoBoxes.Count > 0);
				UpdatePhotoText();
				DeletePrompt.gameObject.SetActive(photoBoxes.Count > 0);
				AcceptPrompt.gameObject.SetActive(photoBoxes.Count > 0);
				if (photoBoxes.Count == 0)
				{
					DataManager.Instance.Alerts.GalleryAlerts.ClearAll();
				}
				InputTimeOut = Time.unscaledTime;
			});
		}
		if (InputManager.PhotoMode.GetGalleryFolderButtonDown() && _canvasGroup.interactable)
		{
			string directory = MMImageDataReadWriter.GetDirectory();
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			Application.OpenURL(directory);
		}
	}

	private void OnPhotoHovered(PhotoInformationBox photoInformationBox)
	{
		_currentPhoto = photoInformationBox;
		currentHoveredPhoto = photoInformationBox.PhotoData;
	}

	private void OnPhotoSelected(PhotoInformationBox photoInformationBox)
	{
		if (!(Time.unscaledTime - InputTimeOut < InputTimeOutTime))
		{
			InputTimeOut = Time.unscaledTime;
			PhotoModeManager.CurrentPhotoState = PhotoModeManager.PhotoState.EditPhoto;
			UIEditPhotoOverlayController uIEditPhotoOverlayController = Push(MonoSingleton<UIManager>.Instance.EditPhotoMenuTemplate);
			uIEditPhotoOverlayController.OnHide = (Action)Delegate.Combine(uIEditPhotoOverlayController.OnHide, (Action)delegate
			{
				InputTimeOut = Time.unscaledTime + InputTimeOutTime;
			});
			uIEditPhotoOverlayController.OnHidden = (Action)Delegate.Combine(uIEditPhotoOverlayController.OnHidden, (Action)delegate
			{
				PhotoModeManager.CurrentPhotoState = PhotoModeManager.PhotoState.Gallery;
			});
			uIEditPhotoOverlayController.Show(photoInformationBox.PhotoData);
			uIEditPhotoOverlayController.OnNewPhotoCreated = (Action<PhotoModeManager.PhotoData>)Delegate.Combine(uIEditPhotoOverlayController.OnNewPhotoCreated, (Action<PhotoModeManager.PhotoData>)delegate(PhotoModeManager.PhotoData data)
			{
				_texturesToDestroy.Add(data.PhotoTexture);
				PhotoInformationBox photoInformationBox2 = MakePhoto(data, false);
				photoInformationBox2.Button.SetInteractionState(false);
				photoInformationBox2.transform.SetAsFirstSibling();
				photoBoxes.Insert(0, photoInformationBox2);
				noPhotos.gameObject.SetActive(photoBoxes.Count == 0);
				_CardcontentContainer.gameObject.SetActive(photoBoxes.Count > 0);
				UpdatePhotoText();
			});
		}
	}

	private PhotoInformationBox MakePhoto(PhotoModeManager.PhotoData photoData, bool removeOnHover)
	{
		PhotoInformationBox photoInformationBox = MonoSingleton<UIManager>.Instance.PhotoInformationBoxTemplate.Spawn(_contentContainer);
		photoInformationBox.transform.localScale = Vector3.one;
		photoInformationBox.OnPhotoSelected = (Action<PhotoInformationBox>)Delegate.Combine(photoInformationBox.OnPhotoSelected, new Action<PhotoInformationBox>(OnPhotoSelected));
		photoInformationBox.OnPhotoHovered = (Action<PhotoInformationBox>)Delegate.Combine(photoInformationBox.OnPhotoHovered, new Action<PhotoInformationBox>(OnPhotoHovered));
		photoInformationBox.Configure(photoData, removeOnHover);
		return photoInformationBox;
	}

	public override void OnCancelButtonInput()
	{
		base.OnCancelButtonInput();
		if (_canvasGroup.interactable)
		{
			UIManager.PlayAudio("event:/ui/go_back");
			Hide();
		}
	}

	protected override void OnHideStarted()
	{
		base.OnHideStarted();
		_scrollRect.enabled = false;
		if (_loadPhotosCoroutine != null)
		{
			StopCoroutine(_loadPhotosCoroutine);
			_loadPhotosCoroutine = null;
		}
		AudioManager.Instance.PlayOneShot("event:/ui/close_menu");
	}

	protected override void OnHideCompleted()
	{
		base.OnHideCompleted();
		foreach (PhotoInformationBox photoBox in photoBoxes)
		{
			photoBox.Recycle();
		}
		photoBoxes = new List<PhotoInformationBox>();
		foreach (Texture2D item in _texturesToDestroy)
		{
			UnityEngine.Object.Destroy(item);
		}
		_texturesToDestroy.Clear();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void UpdatePhotoText()
	{
		_photosTaken.text = string.Format(ScriptLocalization.UI_PhotoMode.Photos.Replace("/{1}", ""), photoBoxes.Count);
	}
}
