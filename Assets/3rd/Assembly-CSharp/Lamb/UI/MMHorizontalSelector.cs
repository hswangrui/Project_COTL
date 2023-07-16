using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class MMHorizontalSelector : MonoBehaviour
	{
		private const string kNextAnimationState = "Forward";

		private const string kPreviousAnimationState = "Previous";

		public Action<int> OnSelectionChanged;

		private bool _interactable = true;

		[Header("Text")]
		[SerializeField]
		private TextMeshProUGUI _text;

		[SerializeField]
		private TextMeshProUGUI _textHelper;

		[Header("Content")]
		[SerializeField]
		private bool _invertAnimations;

		[SerializeField]
		private bool _loopContent;

		[SerializeField]
		private bool _localizeContent;

		[SerializeField]
		private bool _prefillContent;

		[SerializeField]
		[TermsPopup("")]
		private string[] _prefilledContent;

		[Header("Buttons")]
		[SerializeField]
		private MMButton _leftButton;

		[SerializeField]
		private MMButton _rightButton;

		[Header("Misc")]
		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private Animator _animator;

		private string[] _content;

		private int _contentIndex;

		public MMButton LeftButton
		{
			get
			{
				return _leftButton;
			}
		}

		public MMButton RightButton
		{
			get
			{
				return _rightButton;
			}
		}

		public bool LocalizeContent
		{
			get
			{
				return _localizeContent;
			}
			set
			{
				_localizeContent = value;
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
				_leftButton.interactable = _interactable;
				_rightButton.interactable = _interactable;
				_canvasGroup.alpha = (_interactable ? 1f : 0.5f);
			}
		}

		public Color Color
		{
			get
			{
				return _text.color;
			}
			set
			{
				TextMeshProUGUI text = _text;
				Color color2 = (_textHelper.color = value);
				text.color = color2;
			}
		}

		public int CurrentSelection
		{
			get
			{
				return _contentIndex;
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
					_textHelper.text = _content[_contentIndex];
					_leftButton.gameObject.SetActive(_contentIndex > 0 && !_loopContent);
					_rightButton.gameObject.SetActive(_contentIndex < _content.Length - 1 && !_loopContent);
				}
			}
		}

		public string[] Content
		{
			get
			{
				return _content;
			}
		}

		private void Awake()
		{
			if (_prefillContent)
			{
				UpdateContent(_prefilledContent);
			}
			_leftButton.onClick.AddListener(OnLeftButtonClicked);
			_rightButton.onClick.AddListener(OnRightButtonClicked);
			LocalizationManager.OnLocalizeEvent += ReLocalize;
		}

		private void OnDestroy()
		{
			LocalizationManager.OnLocalizeEvent -= ReLocalize;
		}

		private void OnLeftButtonClicked()
		{
			int contentIndex = _contentIndex;
			if (_contentIndex == 0 && _loopContent)
			{
				_contentIndex = _content.Length - 1;
			}
			else
			{
				if (_contentIndex <= 0)
				{
					_text.gameObject.transform.DOKill();
					_text.gameObject.transform.localPosition = Vector3.zero;
					_text.gameObject.transform.DOPunchPosition(new Vector3(-5f, 0f, 0f), 0.5f).SetUpdate(true);
					return;
				}
				_contentIndex--;
			}
			_leftButton.gameObject.SetActive(_contentIndex > 0 && !_loopContent);
			_rightButton.gameObject.SetActive(true);
			Action<int> onSelectionChanged = OnSelectionChanged;
			if (onSelectionChanged != null)
			{
				onSelectionChanged(_contentIndex);
			}
			_text.text = _content[_contentIndex];
			_textHelper.text = _content[contentIndex];
			_animator.Play(_invertAnimations ? "Forward" : "Previous", -1, 0f);
		}

		private void OnRightButtonClicked()
		{
			int contentIndex = _contentIndex;
			if (_contentIndex == _content.Length - 1 && _loopContent)
			{
				_contentIndex = 0;
			}
			else
			{
				if (_contentIndex >= _content.Length - 1)
				{
					_text.gameObject.transform.DOKill();
					_text.gameObject.transform.localPosition = Vector3.zero;
					_text.gameObject.transform.DOPunchPosition(new Vector3(5f, 0f, 0f), 0.5f).SetUpdate(true);
					return;
				}
				_contentIndex++;
			}
			_rightButton.gameObject.SetActive(_contentIndex < _content.Length - 1 && !_loopContent);
			_leftButton.gameObject.SetActive(true);
			Action<int> onSelectionChanged = OnSelectionChanged;
			if (onSelectionChanged != null)
			{
				onSelectionChanged(_contentIndex);
			}
			_text.text = _content[_contentIndex];
			_textHelper.text = _content[contentIndex];
			_animator.Play(_invertAnimations ? "Previous" : "Forward", -1, 0f);
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
			_textHelper.text = _content[_contentIndex];
			_leftButton.gameObject.SetActive(_contentIndex > 0 && !_loopContent);
			_rightButton.gameObject.SetActive(_contentIndex < _content.Length - 1 && !_loopContent);
		}

		private void ReLocalize()
		{
			if (_prefillContent && _localizeContent)
			{
				Debug.Log((base.gameObject.transform.parent.name + " Re-Localize").Colour(Color.red));
				UpdateContent(_prefilledContent);
			}
		}
	}
}
