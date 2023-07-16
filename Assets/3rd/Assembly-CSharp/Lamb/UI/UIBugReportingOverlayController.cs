using System;
using System.Collections;
using System.IO;
using System.Text;
using I2.Loc;
using src.UINavigator;
using src.Utilities;
using TMPro;
using Unity.Cloud.UserReporting;
using Unity.Cloud.UserReporting.Client;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;

namespace Lamb.UI
{
	public class UIBugReportingOverlayController : UIMenuBase
	{
		private enum State
		{
			Undefined,
			Loading,
			Form,
			Sending,
			Completed,
			Error
		}

		private enum ValidationResult
		{
			Valid,
			Invalid,
			Spam
		}

		private const int kMaxSpamCount = 2;

		[Header("Bug Reporting")]
		[SerializeField]
		private GameObject _buttonHighlight;

		[SerializeField]
		private UserReportingPlatformType UserReportingPlatform;

		[Header("Loading")]
		[SerializeField]
		private GameObject _loadingContent;

		[Header("Form")]
		[SerializeField]
		private GameObject _formContent;

		[SerializeField]
		private MMInputField _summaryField;

		[SerializeField]
		private BugInputField _summarySelector;

		[SerializeField]
		private MMInputField _contactField;

		[SerializeField]
		private BugInputField _contactSelector;

		[SerializeField]
		private MMInputField _descriptionField;

		[SerializeField]
		private BugInputField _descriptionSelector;

		[SerializeField]
		private MMButton _cancelButton;

		[SerializeField]
		private MMButton _acceptButton;

		[Header("Completed")]
		[SerializeField]
		private GameObject _sendingContent;

		[SerializeField]
		private TextMeshProUGUI _sendingStatus;

		[Header("Completed")]
		[SerializeField]
		private GameObject _completedContent;

		[SerializeField]
		private MMButton _completedConfirmButton;

		[Header("Eror")]
		[SerializeField]
		private TextMeshProUGUI _errorHeader;

		[SerializeField]
		private TextMeshProUGUI _erroDescription;

		[SerializeField]
		private GameObject _errorContent;

		[SerializeField]
		private MMButton _errorConfirmButton;

		private UserReport _currentUserReport;

		private State _currentState;

		private UnityUserReportingUpdater _unityUserReportingUpdater;

		private string _summary;

		private string _contact;

		private string _description;

		private static int _spamCount;

		public UIBugReportingOverlayController()
		{
			_unityUserReportingUpdater = new UnityUserReportingUpdater();
		}

		protected override void OnShowStarted()
		{
			_acceptButton.onClick.AddListener(SubmitUserReport);
			_cancelButton.onClick.AddListener(OnCancelButtonInput);
			_completedConfirmButton.onClick.AddListener(OnCancelButtonInput);
			_errorConfirmButton.onClick.AddListener(OnCancelButtonInput);
			MMInputField summaryField = _summaryField;
			summaryField.OnDeselected = (Action)Delegate.Combine(summaryField.OnDeselected, new Action(_summarySelector.ShowNormal));
			MMInputField summaryField2 = _summaryField;
			summaryField2.OnSelected = (Action)Delegate.Combine(summaryField2.OnSelected, new Action(_summarySelector.ShowSelected));
			MMInputField descriptionField = _descriptionField;
			descriptionField.OnDeselected = (Action)Delegate.Combine(descriptionField.OnDeselected, new Action(_descriptionSelector.ShowNormal));
			MMInputField descriptionField2 = _descriptionField;
			descriptionField2.OnSelected = (Action)Delegate.Combine(descriptionField2.OnSelected, new Action(_descriptionSelector.ShowSelected));
			MMInputField contactField = _contactField;
			contactField.OnDeselected = (Action)Delegate.Combine(contactField.OnDeselected, new Action(_contactSelector.ShowNormal));
			MMInputField contactField2 = _contactField;
			contactField2.OnSelected = (Action)Delegate.Combine(contactField2.OnSelected, new Action(_contactSelector.ShowSelected));
			_summary = _summaryField.text;
			_contact = _contactField.text;
			_description = _descriptionField.text;
			if (_spamCount > 2)
			{
				ChangeState(State.Error, ScriptLocalization.UI_BugReporting.TooManyReports, ScriptLocalization.UI_BugReporting_TooManyReports.Description);
			}
			else
			{
				ChangeState(State.Loading);
			}
		}

		private void ChangeState(State newState, string header = "", string description = "")
		{
			if (newState == _currentState)
			{
				return;
			}
			_currentState = newState;
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			_loadingContent.SetActive(_currentState == State.Loading);
			_formContent.SetActive(_currentState == State.Form);
			_sendingContent.SetActive(_currentState == State.Sending);
			_completedContent.SetActive(_currentState == State.Completed);
			_errorContent.SetActive(_currentState == State.Error);
			_buttonHighlight.SetActive(_currentState != State.Loading && _currentState != State.Sending);
			if (_currentState == State.Loading)
			{
				EnterLoadingState();
			}
			else if (_currentState == State.Form)
			{
				EnterFormState();
			}
			else if (_currentState == State.Sending)
			{
				EnterSendingState();
			}
			else if (_currentState == State.Completed)
			{
				EnterCompletedState();
			}
			else if (_currentState == State.Error)
			{
				EnterErrorState();
				if (!string.IsNullOrEmpty(header))
				{
					_errorHeader.text = header;
				}
				if (!string.IsNullOrEmpty(description))
				{
					_erroDescription.text = description;
				}
			}
		}

		public override void OnCancelButtonInput()
		{
			if (!_summaryField.isFocused && !_contactField.isFocused && !_descriptionField.isFocused && _canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void Update()
		{
			_unityUserReportingUpdater.Reset();
			StartCoroutine(_unityUserReportingUpdater);
		}

		private void EnterLoadingState()
		{
			UnityUserReporting.CurrentClient.IsSelfReporting = false;
			UnityUserReporting.CurrentClient.SendEventsToAnalytics = false;
			UnityUserReporting.Configure(GetConfiguration());
			string endpoint = string.Format("https://userreporting.cloud.unity3d.com/api/userreporting/projects/{0}/ping", UnityUserReporting.CurrentClient.ProjectIdentifier);
			UnityUserReporting.CurrentClient.Platform.Post(endpoint, "application/json", Encoding.UTF8.GetBytes("\"Ping\""), delegate
			{
			}, delegate
			{
			});
			StartCoroutine(WaitForReport());
			CreateReport();
		}

		private IEnumerator WaitForReport()
		{
			float timeout = 0f;
			while (_currentUserReport == null)
			{
				timeout += Time.unscaledDeltaTime;
				if (timeout >= 10f)
				{
					ChangeState(State.Error);
					yield break;
				}
				yield return null;
			}
			ChangeState(State.Form);
		}

		private void EnterFormState()
		{
			OverrideDefault(_cancelButton);
			ActivateNavigation();
		}

		private void SubmitUserReport()
		{
			ValidationResult result;
			if (!IsReportValid(out result))
			{
				switch (result)
				{
				case ValidationResult.Spam:
					_spamCount++;
					ChangeState(State.Error, "Invalid Report!", "If you do not enter a valid summary and description of your issue you will be locked from sending bug reports");
					return;
				case ValidationResult.Invalid:
					ChangeState(State.Error, "Invalid Report!", "Please make sure you're entering a correct summary and description of the issue");
					return;
				}
			}
			_currentUserReport.Summary = "[prc]" + _summaryField.text;
			_currentUserReport.Fields.Add(new UserReportNamedValue("Description", _descriptionField.text));
			if (_contact != _contactField.text)
			{
				_currentUserReport.Fields.Add(new UserReportNamedValue("Contact", _contactField.text));
			}
			_currentUserReport.Fields.Add(new UserReportNamedValue("Category", "Bug"));
			ChangeState(State.Sending);
		}

		private bool IsReportValid(out ValidationResult result)
		{
			string text = _summary.StripWhitespace();
			string text2 = _description.StripWhitespace();
			string text3 = _summaryField.text.StripWhitespace();
			string text4 = _descriptionField.text.StripWhitespace();
			if (string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(text4))
			{
				result = ValidationResult.Invalid;
				return false;
			}
			if (text == text3 && text2 == text4)
			{
				result = ValidationResult.Spam;
				return false;
			}
			result = ValidationResult.Valid;
			return true;
		}

		private void EnterSendingState()
		{
			UnityUserReporting.CurrentClient.SendUserReport(_currentUserReport, delegate(float uploadProgress, float downloadProgress)
			{
				string text = string.Format("SENDING {0:P}", uploadProgress);
				_sendingStatus.text = text;
			}, delegate(bool success, UserReport br2)
			{
				if (!success)
				{
					ChangeState(State.Error);
				}
				else
				{
					ChangeState(State.Completed);
				}
			});
		}

		private void EnterCompletedState()
		{
			OverrideDefault(_completedConfirmButton);
			ActivateNavigation();
		}

		private void EnterErrorState()
		{
			OverrideDefault(_errorConfirmButton);
			ActivateNavigation();
		}

		private void CreateReport()
		{
			UnityUserReporting.CurrentClient.TakeScreenshotFromSource(Screen.width, Screen.height, Camera.main, delegate
			{
			});
			UnityUserReporting.CurrentClient.TakeScreenshotFromSource(Screen.width, Screen.height, Camera.main, delegate
			{
			});
			UnityUserReporting.CurrentClient.CreateUserReport(delegate(UserReport userReport)
			{
				Debug.Log("Created user report".Colour(Color.yellow));
				if (string.IsNullOrEmpty(userReport.ProjectIdentifier))
				{
					Debug.LogWarning("The user report's project identifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityUserReporting.Configure().");
				}
				string path = Path.Combine(Application.persistentDataPath, "saves");
				COTLDataReadWriter<DataManager> cOTLDataReadWriter = new COTLDataReadWriter<DataManager>();
				string text = SaveAndLoad.MakeSaveSlot(SaveAndLoad.SAVE_SLOT);
				if (cOTLDataReadWriter.FileExists(text))
				{
					byte[] data = File.ReadAllBytes(Path.Combine(path, text));
					data = Compression.Compress(data);
					userReport.Attachments.Add(new UserReportAttachment("SaveFile", "SaveFile.zip", "application/zip", data));
				}
				COTLDataReadWriter<SettingsData> cOTLDataReadWriter2 = new COTLDataReadWriter<SettingsData>();
				string text2 = "settings.json";
				if (cOTLDataReadWriter2.FileExists(text2))
				{
					byte[] data2 = File.ReadAllBytes(Path.Combine(path, text2));
					data2 = Compression.Compress(data2);
					userReport.Attachments.Add(new UserReportAttachment("SettingsFile", "SettingsFile.zip", "application/zip", data2));
				}
				MonoSingleton<MMLogger>.Instance.Enabled = false;
				if (File.Exists(MMLogger.GetFileDirectory()))
				{
					byte[] data3 = File.ReadAllBytes(MMLogger.GetFileDirectory());
					data3 = Compression.Compress(data3);
					userReport.Attachments.Add(new UserReportAttachment("MMLog", "MMLog.zip", "application/log", data3));
				}
				MonoSingleton<MMLogger>.Instance.Enabled = true;
				string arg = "Unknown";
				string arg2 = "0.0";
				foreach (UserReportNamedValue deviceMetadatum in userReport.DeviceMetadata)
				{
					if (deviceMetadatum.Name == "Platform")
					{
						arg = deviceMetadatum.Value;
					}
					if (deviceMetadatum.Name == "Version")
					{
						arg2 = deviceMetadatum.Value;
					}
				}
				userReport.Dimensions.Add(new UserReportNamedValue("Platform.Version", string.Format("{0}.{1}", arg, arg2)));
				_currentUserReport = userReport;
			});
		}

		private UserReportingClientConfiguration GetConfiguration()
		{
			return new UserReportingClientConfiguration();
		}
	}
}
