using System;
using System.Collections.Generic;
using I2.Loc;
using src.Extensions;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class MMDropdown : MonoBehaviour
	{
		public Action<int> OnValueChanged;

		[Header("Components")]
		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private TMP_Text _text;

		[Header("Dropdown Properties")]
		[SerializeField]
		private bool _localizeContent;

		[SerializeField]
		private bool _prefillContent;

		[SerializeField]
		[TermsPopup("")]
		private string[] _prefilledContent;

		private int _contentIndex;

		private string[] _content;

		private bool _interactable = true;

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public string[] Content
		{
			get
			{
				return _content;
			}
		}

		public int ContentIndex
		{
			get
			{
				return _contentIndex;
			}
			set
			{
				if (_contentIndex != value && _content != null && value >= 0 && value <= _content.Length - 1)
				{
					_contentIndex = value;
					_text.text = _content[_contentIndex];
				}
			}
		}

		public bool Interactable
		{
			get
			{
				return _interactable;
			}
			set
			{
				_interactable = value;
				_button.Interactable = _interactable;
			}
		}

		private void Awake()
		{
			_button.onClick.AddListener(Open);
			LocalizationManager.OnLocalizeEvent += ReLocalize;
		}

		private void OnDestroy()
		{
			LocalizationManager.OnLocalizeEvent -= ReLocalize;
		}

		public void PrefillContent(List<string> content)
		{
			PrefillContent(content.ToArray());
		}

		public void PrefillContent(params string[] content)
		{
			_prefillContent = true;
			_prefilledContent = content;
			UpdateContent(_prefilledContent);
		}

		private void UpdateContent(string[] newContent)
		{
			_content = new string[newContent.Length];
			for (int i = 0; i < newContent.Length; i++)
			{
				if (!_localizeContent)
				{
					_content[i] = newContent[i];
				}
				else
				{
					_content[i] = LocalizationManager.GetTranslation(newContent[i]);
				}
			}
			_text.text = _content[_contentIndex];
		}

		private void ReLocalize()
		{
			if (_prefillContent && _localizeContent)
			{
				Debug.Log((base.gameObject.transform.parent.name + " Re-Localize").Colour(Color.red));
				UpdateContent(_prefilledContent);
			}
		}

		public void Open()
		{
			UIMenuBase uIMenuBase = UIMenuBase.ActiveMenus.LastElement();
			UIDropdownOverlayController uIDropdownOverlayController = MonoSingleton<UIManager>.Instance.DropdownOverlayControllerTemplate.Instantiate();
			uIDropdownOverlayController.Show(this);
			uIDropdownOverlayController.OnItemChosen = (Action<int>)Delegate.Combine(uIDropdownOverlayController.OnItemChosen, (Action<int>)delegate(int index)
			{
				Action<int> onValueChanged = OnValueChanged;
				if (onValueChanged != null)
				{
					onValueChanged(index);
				}
				ContentIndex = index;
			});
			uIMenuBase.PushInstance(uIDropdownOverlayController);
		}
	}
}
