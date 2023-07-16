using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuAdController : MonoBehaviour
{
	[Serializable]
	public struct Ad
	{
		public Sprite contentSprite;

		public string messageTerm;

		public string buttonTerm;

		public string link;
	}

	[Serializable]
	public struct AdViewer
	{
		public Image Content;

		public TMP_Text Message;

		public CanvasGroup CanvasGroup;
	}

	[SerializeField]
	private MMButton buttonObj;

	[SerializeField]
	private Transform _bannerObj;

	[SerializeField]
	private Image buttonRed;

	[SerializeField]
	private Transform scaleTransform;

	[SerializeField]
	private Image outline;

	[SerializeField]
	private AdViewer ad1;

	[SerializeField]
	private AdViewer ad2;

	[SerializeField]
	private TMP_Text buttonText;

	[SerializeField]
	private List<Ad> nonRemoteAds;

	private string link;

	private List<Ad> ads = new List<Ad>();

	private int index;

	private const float timeBetweenAds = 10f;

	private float timestamp;

	private void Start()
	{
		if (nonRemoteAds.Count > 0)
		{
			ads = nonRemoteAds;
		}
		buttonRed.DOFade(0f, 0.1f).SetUpdate(true);
		MMButton mMButton = buttonObj;
		mMButton.OnSelected = (Action)Delegate.Combine(mMButton.OnSelected, new Action(OnButtonSelect));
		MMButton mMButton2 = buttonObj;
		mMButton2.OnDeselected = (Action)Delegate.Combine(mMButton2.OnDeselected, new Action(OnButtonDeSelect));
		ads.Shuffle();
	}

	private void OnButtonSelect()
	{
		outline.transform.DOKill();
		outline.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetUpdate(true).SetEase(Ease.InCirc);
		_bannerObj.transform.DOKill();
		_bannerObj.transform.localScale = Vector3.one * 1.2f;
		_bannerObj.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
		scaleTransform.transform.DOKill();
		scaleTransform.transform.localScale = Vector3.one * 1.2f;
		scaleTransform.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
		buttonRed.DOKill();
		buttonRed.DOFade(1f, 0.25f).SetUpdate(true).SetEase(Ease.OutCirc);
	}

	private void OnButtonDeSelect()
	{
		outline.transform.DOKill();
		outline.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutCirc);
		_bannerObj.transform.DOKill();
		_bannerObj.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutCirc);
		buttonRed.DOKill();
		buttonRed.DOFade(0f, 0.25f).SetUpdate(true).SetEase(Ease.InCirc);
	}

	public void Configure(Ad ad)
	{
		ads.Add(ad);
	}

	private void Update()
	{
		if (Time.time > timestamp)
		{
			IncrementAd(1);
		}
	}

	public void IncrementAd(int count)
	{
		index = (int)Mathf.Repeat(index + count, ads.Count);
		ShowAd(ads[index]);
		timestamp = Time.time + 10f;
	}

	private void ShowAd(Ad ad)
	{
		AdViewer adViewer = ad2;
		AdViewer adViewer2 = ad1;
		if (ad1.CanvasGroup.alpha >= 1f)
		{
			adViewer2 = ad2;
			adViewer = ad1;
		}
		adViewer2.Content.sprite = ad.contentSprite;
		adViewer2.Message.text = ad.messageTerm;
		adViewer2.Message.GetComponent<Localize>().Term = ad.messageTerm;
		buttonText.text = ad.buttonTerm;
		buttonText.GetComponent<Localize>().Term = ad.buttonTerm;
		link = ad.link;
		adViewer.CanvasGroup.DOFade(0f, 0.5f);
		adViewer2.CanvasGroup.DOFade(1f, 0.5f);
	}

	public void ButtonPressed()
	{
		AudioManager.Instance.PlayOneShot("event:/shop/buy");
		Application.OpenURL(link);
	}
}
