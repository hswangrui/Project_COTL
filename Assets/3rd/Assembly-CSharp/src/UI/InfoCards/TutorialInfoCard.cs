using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI.InfoCards
{
	public class TutorialInfoCard : UIInfoCardBase<TutorialTopic>
	{
		public Action OnLeftArrowClicked;

		public Action OnRightArrowClicked;

		[Header("Tutorial Info Card")]
		[SerializeField]
		private TutorialConfiguration _tutorialConfiguration;

		[Header("Content")]
		[SerializeField]
		private GameObject _topContainer;

		[SerializeField]
		private TextMeshProUGUI _header;

		[SerializeField]
		private TextMeshProUGUI _description;

		[SerializeField]
		private Image _image;

		[SerializeField]
		private TextMeshProUGUI _tutorialBody;

		[Header("Pages")]
		[SerializeField]
		private TextMeshProUGUI _pageText;

		[SerializeField]
		private MMButton _leftArrow;

		[SerializeField]
		private MMButton _rightArrow;

		private int _page;

		private TutorialCategory _category;

		public int Page
		{
			get
			{
				return _page;
			}
		}

		public int NumPages
		{
			get
			{
				return _category.Entries.Length;
			}
		}

		public override void Awake()
		{
			base.Awake();
			_leftArrow.onClick.AddListener(delegate
			{
				OnLeftArrowClicked();
			});
			_rightArrow.onClick.AddListener(delegate
			{
				OnRightArrowClicked();
			});
		}

		public override void Configure(TutorialTopic config)
		{
			_category = _tutorialConfiguration.GetCategory(config);
			_header.text = _category.GetTitle();
			_description.text = _category.GetDescription();
			_topContainer.SetActive(_category.TopicImage != null);
			ConfigurePage(0);
		}

		private void ConfigurePage(int page)
		{
			_page = page;
			if (page > 0 && page <= _category.Entries.Length)
			{
				_image.sprite = _category.Entries[page - 1].Image;
				_tutorialBody.text = _category.Entries[page - 1].Description;
			}
			else
			{
				_image.sprite = _category.TopicImage;
			}
			_pageText.text = string.Format("{0}/{1}", page + 1, _category.Entries.Length + 1);
			_leftArrow.gameObject.SetActive(page != 0);
			_rightArrow.gameObject.SetActive(page != _category.Entries.Length);
			_topContainer.SetActive(_page == 0);
			_tutorialBody.gameObject.SetActive(_page > 0);
		}

		public IEnumerator NextPage()
		{
			if (_page < _category.Entries.Length)
			{
				UIManager.PlayAudio("event:/ui/arrow_change_selection");
				base.RectTransform.DOKill(true);
				base.CanvasGroup.DOKill(true);
				base.RectTransform.DOAnchorPos(new Vector2(-100f, 0f), 0.15f).SetEase(Ease.InSine).SetUpdate(true);
				base.CanvasGroup.DOFade(0f, 0.15f).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.15f);
				ConfigurePage(_page + 1);
				base.RectTransform.DOKill();
				base.RectTransform.anchoredPosition = new Vector2(100f, 0f);
				base.RectTransform.DOAnchorPos(new Vector2(0f, 0f), 0.15f).SetEase(Ease.OutSine).SetUpdate(true);
				base.CanvasGroup.DOFade(1f, 0.15f).SetUpdate(true);
			}
		}

		public IEnumerator PreviousPage()
		{
			if (_page > 0)
			{
				UIManager.PlayAudio("event:/ui/arrow_change_selection");
				base.RectTransform.DOKill(true);
				base.CanvasGroup.DOKill(true);
				base.RectTransform.DOAnchorPos(new Vector2(100f, 0f), 0.15f).SetEase(Ease.InSine).SetUpdate(true);
				base.CanvasGroup.DOFade(0f, 0.15f).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.15f);
				ConfigurePage(_page - 1);
				_topContainer.SetActive(_page == 0);
				_tutorialBody.gameObject.SetActive(_page > 0);
				base.RectTransform.DOKill();
				base.RectTransform.anchoredPosition = new Vector2(-100f, 0f);
				base.RectTransform.DOAnchorPos(new Vector2(0f, 0f), 0.15f).SetEase(Ease.OutSine).SetUpdate(true);
				base.CanvasGroup.DOFade(1f, 0.15f).SetUpdate(true);
			}
		}
	}
}
