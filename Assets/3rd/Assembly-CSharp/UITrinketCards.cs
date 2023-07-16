using System;
using System.Collections;
using DG.Tweening;
using MMTools.UIInventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITrinketCards : UIInventoryController, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public UIWeaponCard TrinketCard;

	public GameObject List;

	public Image background;

	private bool CameFromPauseScreen;

	[SerializeField]
	private Image selectedGameobject;

	private CanvasGroup canvasGroup;

	private TarotCards.TarotCard _drawnCard;

	private bool _requiresCallbacks = true;

	public bool DestroyAfter { get; set; } = true;


	public static GameObject Play(TarotCards.TarotCard drawnCard, Action CallBack, float pauseTimeSpeed = 1f)
	{
		UITrinketCards uITrinketCards = UnityEngine.Object.Instantiate(Resources.Load<UITrinketCards>("MMUIInventory/UI Trinket Cards"), GlobalCanvasReference.Instance);
		uITrinketCards.Callback = CallBack;
		uITrinketCards.PauseTimeSpeed = pauseTimeSpeed;
		uITrinketCards._drawnCard = drawnCard;
		uITrinketCards.TrinketCard.CameFromPauseMenu = false;
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_reveal", PlayerFarming.Instance.transform.position);
		return uITrinketCards.gameObject;
	}

	public static void PlayFromPause(TarotCards.TarotCard drawnCard, Action CallBack)
	{
		UITrinketCards uITrinketCards = UnityEngine.Object.Instantiate(Resources.Load<UITrinketCards>("MMUIInventory/UI Trinket Cards"), GlobalCanvasReference.Instance);
		uITrinketCards.Callback = CallBack;
		uITrinketCards.PauseTimeSpeed = 0f;
		uITrinketCards._drawnCard = drawnCard;
		uITrinketCards.CameFromPauseScreen = true;
		uITrinketCards.TrinketCard.CameFromPauseMenu = true;
	}

	public static void PlayFromPause(TarotCards.TarotCard drawnCard, Action CallBack, GameObject parent)
	{
		UITrinketCards uITrinketCards = UnityEngine.Object.Instantiate(Resources.Load<UITrinketCards>("MMUIInventory/UI Trinket Cards"), parent.transform);
		uITrinketCards.Callback = CallBack;
		uITrinketCards.PauseTimeSpeed = 0f;
		uITrinketCards._drawnCard = drawnCard;
		uITrinketCards.CameFromPauseScreen = true;
		uITrinketCards.TrinketCard.CameFromPauseMenu = true;
	}

	public void Play(TarotCards.TarotCard drawnCard)
	{
		_drawnCard = drawnCard;
		PauseTimeSpeed = 1f;
		DestroyAfter = false;
		CameFromPauseScreen = false;
		_requiresCallbacks = false;
		TrinketCard.CameFromPauseMenu = false;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		base.transform.DOKill();
		canvasGroup.DOKill();
		selectedGameobject.transform.DOKill();
		base.transform.DOScale(0.9f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuart);
		canvasGroup.DOFade(0.9f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuart);
		selectedGameobject.DOFade(0f, 0.25f).SetUpdate(true);
	}

	public void OnSelect(BaseEventData eventData)
	{
		base.transform.DOKill();
		canvasGroup.DOKill();
		selectedGameobject.transform.DOKill();
		base.transform.DOScale(1f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuart);
		canvasGroup.DOFade(1f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuart);
		selectedGameobject.transform.DOKill();
		selectedGameobject.DOFade(1f, 0.25f).SetUpdate(true);
	}

	public override void StartUIInventoryController()
	{
		StartCoroutine(DealCards());
	}

	private void OnEnable()
	{
		if (canvasGroup == null)
		{
			base.gameObject.GetComponent<CanvasGroup>();
		}
		base.transform.DOScale(0.9f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuart);
		canvasGroup.DOFade(0.9f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuart);
	}

	private IEnumerator DealCards()
	{
		yield return new WaitForEndOfFrame();
		yield return StartCoroutine(TrinketCard.Play(_drawnCard, Vector3.zero));
		if (!CameFromPauseScreen && Time.timeScale != 0f)
		{
			yield return new WaitForSeconds(1.5f);
		}
		if (!_requiresCallbacks)
		{
			yield break;
		}
		while (!InputManager.UI.GetAcceptButtonDown() && !InputManager.UI.GetCancelButtonDown())
		{
			yield return null;
		}
		if (!CameFromPauseScreen)
		{
			Close();
		}
		else
		{
			Action callback = Callback;
			if (callback != null)
			{
				callback();
			}
		}
		if (DestroyAfter)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
