using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnlockCurse : BaseMonoBehaviour
{
	public UI_NavigatorSimple UINav;

	public RectTransform CurrentHighlighted;

	public RectTransform IconContainer;

	public TextMeshProUGUI Title;

	public TextMeshProUGUI Description;

	public TextMeshProUGUI Lore;

	public Image Icon;

	[SerializeField]
	private TMP_Text damageText;

	[SerializeField]
	private TMP_Text speedText;

	public CanvasGroup CurrentHighlightedCanvasGroup;

	public CanvasGroup IconContainerCanvasGroup;

	public GameObject CurseIconPrefab;

	[Space]
	[SerializeField]
	private GameObject upgradeParent;

	[SerializeField]
	private TMP_Text upgradeNumberText;

	[SerializeField]
	private TMP_Text upgradeNextNumberText;

	[SerializeField]
	private TMP_Text upgradeDescriptionText;

	private UIUnlockCurseIcon CurrentCurseIcon;

	public GameObject HoldToUnlockObject;

	public Image HoldToUnlockRadialFill;

	[SerializeField]
	private ParticleSystem particlesystem;

	private Coroutine holdToUnlockRoutine;

	private Vector3 CurrentHighlightedStartingPosition;

	private Vector3 IconContainerStartingPosition;

	private Action continueCallback;

	private Action cancelCallback;

	private Tween shakeTween;

	private EventInstance holdLoop;

	private void OnEnable()
	{
		UI_NavigatorSimple uINav = UINav;
		uINav.OnSelectDown = (Action)Delegate.Combine(uINav.OnSelectDown, new Action(OnSelect));
		UI_NavigatorSimple uINav2 = UINav;
		uINav2.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Combine(uINav2.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnChangeSelection));
		UI_NavigatorSimple uINav3 = UINav;
		uINav3.OnDefaultSetComplete = (Action)Delegate.Combine(uINav3.OnDefaultSetComplete, new Action(OnDefaultSetComplete));
		UI_NavigatorSimple uINav4 = UINav;
		uINav4.OnCancelDown = (Action)Delegate.Combine(uINav4.OnCancelDown, new Action(OnCancelDown));
	}

	private void OnDisable()
	{
		UI_NavigatorSimple uINav = UINav;
		uINav.OnSelectDown = (Action)Delegate.Remove(uINav.OnSelectDown, new Action(OnSelect));
		UI_NavigatorSimple uINav2 = UINav;
		uINav2.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Remove(uINav2.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnChangeSelection));
		UI_NavigatorSimple uINav3 = UINav;
		uINav3.OnDefaultSetComplete = (Action)Delegate.Remove(uINav3.OnDefaultSetComplete, new Action(OnDefaultSetComplete));
		UI_NavigatorSimple uINav4 = UINav;
		uINav4.OnCancelDown = (Action)Delegate.Remove(uINav4.OnCancelDown, new Action(OnCancelDown));
		AudioManager.Instance.StopLoop(holdLoop);
		particlesystem.Stop();
		particlesystem.Clear();
	}

	private void Start()
	{
		HoldToUnlockObject.SetActive(false);
		CurrentHighlightedStartingPosition = CurrentHighlighted.localPosition;
		CurrentHighlighted.localPosition += Vector3.up * 100f;
		CurrentHighlighted.DOLocalMove(CurrentHighlightedStartingPosition, 1f).SetEase(Ease.OutBack);
		CurrentHighlightedCanvasGroup.alpha = 0f;
		DOTween.To(() => CurrentHighlightedCanvasGroup.alpha, delegate(float x)
		{
			CurrentHighlightedCanvasGroup.alpha = x;
		}, 1f, 1f);
		IconContainerStartingPosition = IconContainer.localPosition;
		IconContainer.localPosition += Vector3.down * 100f;
		IconContainer.DOLocalMove(IconContainerStartingPosition, 1f).SetEase(Ease.OutBack);
		IconContainerCanvasGroup.alpha = 0f;
		DOTween.To(() => IconContainerCanvasGroup.alpha, delegate(float x)
		{
			IconContainerCanvasGroup.alpha = x;
		}, 1f, 1f);
		Populate();
	}

	public void Init(Action continueCallback, Action cancelCallback)
	{
		this.continueCallback = continueCallback;
		this.cancelCallback = cancelCallback;
	}

	private void Populate()
	{
		UINav.enabled = true;
		UINav.selectable = null;
		UINav.startingItem = null;
		foreach (TarotCards.Card item in new List<TarotCards.Card>
		{
			TarotCards.Card.Fireball,
			TarotCards.Card.EnemyBlast,
			TarotCards.Card.ProjectileAOE,
			TarotCards.Card.Tentacles,
			TarotCards.Card.Vortex,
			TarotCards.Card.MegaSlash
		})
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(CurseIconPrefab, IconContainer);
			gameObject.SetActive(true);
			gameObject.GetComponent<UIUnlockCurseIcon>().Init(item);
			if (UINav.startingItem == null)
			{
				UINav.startingItem = gameObject.GetComponent<Selectable>();
			}
		}
	}

	private IEnumerator PlayRoutine(float Delay, bool SetDefault)
	{
		yield return new WaitForSecondsRealtime(Delay);
		UINav.canvasGroup.interactable = true;
		UINav.enabled = true;
		if (SetDefault)
		{
			UINav.setDefault();
		}
	}

	private void OnCancelDown()
	{
		if (holdToUnlockRoutine != null)
		{
			return;
		}
		GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(delegate
		{
			Action action = cancelCallback;
			if (action != null)
			{
				action();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		});
	}

	private void OnDefaultSetComplete()
	{
		OnChangeSelection(UINav.selectable, null);
	}

	private void OnChangeSelection(Selectable NewSelectable, Selectable PrevSelectable)
	{
		if (NewSelectable == null)
		{
			return;
		}
		AudioManager.Instance.PlayOneShot("event:/ui/change_selection", PlayerFarming.Instance.gameObject);
		UIUnlockCurseIcon component = NewSelectable.GetComponent<UIUnlockCurseIcon>();
		if (component == null)
		{
			DOTween.To(() => CurrentHighlightedCanvasGroup.alpha, delegate(float x)
			{
				CurrentHighlightedCanvasGroup.alpha = x;
			}, 0f, 0.3f);
			return;
		}
		Title.text = TarotCards.LocalisedName(component.Type);
		Lore.text = TarotCards.LocalisedLore(component.Type);
		Description.text = TarotCards.LocalisedDescription(component.Type, 0);
		Icon.sprite = component.Image.sprite;
		float num = Mathf.Round(0f * 100f) / 100f;
		speedText.text = "";
		CurrentHighlightedCanvasGroup.alpha = 0.5f;
		DOTween.To(() => CurrentHighlightedCanvasGroup.alpha, delegate(float x)
		{
			CurrentHighlightedCanvasGroup.alpha = x;
		}, 1f, 0.3f);
	}

	private void OnSelect()
	{
		UINav.selectable.GetComponent<UIUnlockCurseIcon>();
	}

	private IEnumerator HoldToUnlock()
	{
		ParticleSystem.EmissionModule emission = particlesystem.emission;
		emission.rateOverTime = 0f;
		holdLoop = AudioManager.Instance.CreateLoop("event:/hearts_of_the_faithful/draw_power_loop", true);
		UINav.enabled = false;
		Vector3 endValue = CurrentHighlighted.localPosition + Vector3.up * 100f;
		CurrentHighlighted.DOLocalMove(endValue, 0.5f).SetEase(Ease.InBack);
		DOTween.To(() => CurrentHighlightedCanvasGroup.alpha, delegate(float x)
		{
			CurrentHighlightedCanvasGroup.alpha = x;
		}, 0f, 0.5f);
		endValue = IconContainer.localPosition + Vector3.down * 100f;
		IconContainer.DOLocalMove(endValue, 0.5f).SetEase(Ease.InBack);
		DOTween.To(() => IconContainerCanvasGroup.alpha, delegate(float x)
		{
			IconContainerCanvasGroup.alpha = x;
		}, 0f, 0.5f);
		CurrentCurseIcon.costParent.SetActive(false);
		CurrentCurseIcon.upgradeLevel.gameObject.SetActive(false);
		CurrentCurseIcon.upgradeLevelBackground.gameObject.SetActive(false);
		CurrentCurseIcon.transform.DOLocalMove(Vector3.zero, 0.75f).SetEase(Ease.InOutBack);
		yield return new WaitForSecondsRealtime(0.75f);
		CurrentCurseIcon.transform.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
		yield return new WaitForSecondsRealtime(0.5f);
		HoldToUnlockObject.SetActive(true);
		HoldToUnlockObject.transform.localPosition = new Vector3(0f, -250f);
		HoldToUnlockObject.transform.DOLocalMove(new Vector3(0f, -200f), 0.5f).SetEase(Ease.OutBack);
		CanvasGroup c = HoldToUnlockObject.GetComponent<CanvasGroup>();
		c.alpha = 0f;
		DOTween.To(() => c.alpha, delegate(float x)
		{
			c.alpha = x;
		}, 1f, 0.5f);
		HoldToUnlockRadialFill.fillAmount = 0f;
		float Progress = 0f;
		float Duration = 3f;
		while (Progress < Duration)
		{
			emission.rateOverTime = 5f + Progress * 9f;
			holdLoop.setParameterByName("power", Progress / Duration);
			if (InputManager.UI.GetAcceptButtonHeld())
			{
				Progress = Mathf.Clamp(Progress + Time.unscaledDeltaTime, 0f, Duration);
				if (!particlesystem.isPlaying)
				{
					particlesystem.Play();
				}
			}
			else
			{
				Progress = Mathf.Clamp(Progress - Time.unscaledDeltaTime * 5f, 0f, Duration);
			}
			CurrentCurseIcon.transform.localScale = Vector3.one * 2f * (1f + Progress / 20f);
			CurrentCurseIcon.transform.localPosition = UnityEngine.Random.insideUnitCircle * Progress * 2f;
			HoldToUnlockRadialFill.fillAmount = Progress / Duration;
			if (InputManager.UI.GetCancelButtonDown())
			{
				emission.rateOverTime = 0f;
				AudioManager.Instance.StopLoop(holdLoop);
				CancelReset();
				holdToUnlockRoutine = null;
				yield break;
			}
			yield return null;
		}
		CurrentCurseIcon.SetUnlocked();
		emission.rateOverTime = 0f;
		RumbleManager.Instance.Rumble();
		AudioManager.Instance.StopLoop(holdLoop);
		AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/draw_power_end", PlayerFarming.Instance.gameObject);
		HoldToUnlockObject.SetActive(false);
		CameraManager.instance.ShakeCameraForDuration(1.2f, 1.5f, 0.3f);
		CurrentCurseIcon.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
		CurrentCurseIcon.WhiteFlash.color = new Color(1f, 1f, 1f, 0.6f);
		CurrentCurseIcon.WhiteFlash.DOColor(new Color(1f, 1f, 1f, 0f), 0.3f).SetUpdate(true);
		GameManager.GetInstance().HitStop();
		yield return new WaitForSecondsRealtime(0.4f);
		CurrentCurseIcon.SelectedIcon.enabled = true;
		CurrentCurseIcon.SelectedIcon.color = Color.white;
		CurrentCurseIcon.SelectedIcon.DOColor(new Color(1f, 1f, 1f, 0f), 0.3f).SetUpdate(true);
		CurrentCurseIcon.SelectedIcon.rectTransform.localScale = Vector3.one;
		CurrentCurseIcon.SelectedIcon.rectTransform.DOScale(new Vector3(1.5f, 1.5f), 0.3f).SetUpdate(true);
		CurrentCurseIcon.WhiteFlash.color = Color.white;
		CurrentCurseIcon.WhiteFlash.DOColor(new Color(1f, 1f, 1f, 0f), 1f).SetUpdate(true);
		CurrentCurseIcon.transform.DOShakePosition(0.5f, 20f).SetUpdate(true);
		yield return new WaitForSecondsRealtime(1f);
		if (false)
		{
			UpgradeCurse();
		}
		else
		{
			UnlockCurse();
		}
		CurrentCurseIcon.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true)
			.OnComplete(delegate
			{
				particlesystem.Stop();
				particlesystem.Clear();
				holdToUnlockRoutine = null;
				Action action = continueCallback;
				if (action != null)
				{
					action();
				}
				UnityEngine.Object.Destroy(base.gameObject);
			});
	}

	private void CancelReset()
	{
		CanvasGroup c = HoldToUnlockObject.GetComponent<CanvasGroup>();
		c.alpha = 1f;
		DOTween.To(() => c.alpha, delegate(float x)
		{
			c.alpha = x;
		}, 0f, 0.5f).OnComplete(delegate
		{
			HoldToUnlockObject.SetActive(false);
		});
		CurrentCurseIcon.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true)
			.OnComplete(delegate
			{
				UnityEngine.Object.Destroy(CurrentCurseIcon.gameObject);
				HoldToUnlockObject.SetActive(false);
				CurrentHighlighted.localPosition = CurrentHighlightedStartingPosition;
				CurrentHighlighted.localPosition += Vector3.up * 100f;
				CurrentHighlighted.DOLocalMove(CurrentHighlightedStartingPosition, 1f).SetEase(Ease.OutBack);
				CurrentHighlightedCanvasGroup.alpha = 0f;
				DOTween.To(() => CurrentHighlightedCanvasGroup.alpha, delegate(float x)
				{
					CurrentHighlightedCanvasGroup.alpha = x;
				}, 1f, 1f);
				IconContainer.localPosition = IconContainerStartingPosition;
				IconContainer.localPosition += Vector3.down * 100f;
				IconContainer.DOLocalMove(IconContainerStartingPosition, 1f).SetEase(Ease.OutBack);
				IconContainerCanvasGroup.alpha = 0f;
				DOTween.To(() => IconContainerCanvasGroup.alpha, delegate(float x)
				{
					IconContainerCanvasGroup.alpha = x;
				}, 1f, 1f);
				CurrentCurseIcon.costParent.SetActive(true);
				StartCoroutine(PlayRoutine(0.5f, false));
			});
	}

	private void UpgradeCurse()
	{
	}

	private void UnlockCurse()
	{
	}
}
