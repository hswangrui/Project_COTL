using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using Lamb.UI;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICookingMinigameOverlayController : MonoBehaviour
{
	private const float kPauseDuration = 0.3f;

	private const float kAccessibleCooldown = 0.5f;

	public Image Background;

	public Image SafeZone;

	public Image Tracker;

	public Image Flash;

	public float TrackerNormalisedPosition;

	public float Speed = 1f;

	public float GameWidth = 300f;

	public Transform ControlPromptUI;

	private readonly List<float> _beadMealRanges = new List<float> { 50f, 50f };

	private readonly List<float> _mediumMealRanges = new List<float> { 75f, 75f };

	private readonly List<float> _goodMealRanges = new List<float> { 100f, 100f };

	public Action OnCook;

	public Action OnUnderCook;

	public Action OnBurn;

	public TextMeshProUGUI CounterText;

	public RecipeItem RecipeItem;

	private bool MiniGamePaused;

	private bool DidBurn;

	private EventInstance loopingSoundInstance;

	private float _randomOffset;

	private Interaction_Kitchen kitchen;

	private float _accessibleCooldown;

	private int CurrentDifficulty;

	private int StartingMeals;

	private StructuresData StructureInfo;

	public RectTransform RecipeItemRT;

	public CanvasGroup RecipeItemCG;

	public CanvasGroup CanvasGroup;

	private Sequence BurnFadeSequence;

	private float UnderCookedRange
	{
		get
		{
			switch (CurrentDifficulty)
			{
			case 1:
				return _beadMealRanges[0] + _randomOffset;
			case 2:
				return _mediumMealRanges[0] + _randomOffset;
			case 3:
				return _goodMealRanges[0] + _randomOffset;
			default:
				return 0f;
			}
		}
	}

	private float OverCookedRange
	{
		get
		{
			switch (CurrentDifficulty)
			{
			case 1:
				return _beadMealRanges[1] - _randomOffset;
			case 2:
				return _mediumMealRanges[1] - _randomOffset;
			case 3:
				return _goodMealRanges[1] - _randomOffset;
			default:
				return 0f;
			}
		}
	}

	public void Initialise(StructuresData StructureInfo, Interaction_Kitchen kitchen)
	{
		this.StructureInfo = StructureInfo;
		this.kitchen = kitchen;
		StartingMeals = StructureInfo.QueuedMeals.Count;
		UpdateText();
		Flash.color = StaticColors.OffWhiteColor;
		Flash.DOFade(0f, 0.5f);
		SetBarSizes();
		RecipeItem.GetComponent<MMButton>().enabled = false;
		base.transform.localScale = Vector3.one * 1.2f;
		base.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
		CanvasGroup.alpha = 0f;
		CanvasGroup.DOFade(1f, 0.2f);
		string text = "asdasD" + "asdasd";
		loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/cooking/cooking_loop", kitchen.gameObject, true);
		OnAutoCookSettingChanged(SettingsManager.Settings.Accessibility.AutoCook);
	}

	private void OnAutoCookSettingChanged(bool value)
	{
		ControlPromptUI.gameObject.SetActive(!value);
	}

	private void SetBarSizes()
	{
		CurrentDifficulty = CookingData.GetSatationLevel(StructureInfo.QueuedMeals[0].MealType);
		if (StructureInfo.QueuedMeals.Count != StartingMeals)
		{
			_randomOffset = 0f;
			_randomOffset = Mathf.Min(UnderCookedRange, OverCookedRange);
			_randomOffset = UnityEngine.Random.Range(0f - _randomOffset, _randomOffset);
		}
		SafeZone.rectTransform.SetRect(UnderCookedRange, 0f, OverCookedRange, 0f);
	}

	private void UpdateText()
	{
		if (StructureInfo.QueuedMeals.Count > 0)
		{
			CounterText.text = StructureInfo.QueuedMeals.Count + "/" + StartingMeals;
			RecipeItemRT.DOKill();
			RecipeItemRT.localScale = Vector3.one;
			RecipeItemRT.localPosition = new Vector3(0f, -100f);
			RecipeItemRT.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
			RecipeItemCG.alpha = 0f;
			RecipeItemCG.DOFade(1f, 0.25f);
			RecipeItem.Configure(StructureInfo.QueuedMeals[0].MealType, false);
		}
	}

	private void HideText(float Duration)
	{
		RecipeItemRT.DOScale(Vector3.zero, Duration).SetEase(Ease.InBack);
	}

	private void OnEnable()
	{
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnAutoCookChanged = (Action<bool>)Delegate.Combine(instance.OnAutoCookChanged, new Action<bool>(OnAutoCookSettingChanged));
	}

	private void OnDisable()
	{
		if (BurnFadeSequence != null)
		{
			BurnFadeSequence.Kill();
			BurnFadeSequence = null;
		}
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnAutoCookChanged = (Action<bool>)Delegate.Remove(instance.OnAutoCookChanged, new Action<bool>(OnAutoCookSettingChanged));
	}

	public void Close()
	{
		if (DidBurn && DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.BurntFood))
		{
			MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.BurntFood);
		}
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		base.transform.DOLocalMove(new Vector3(0f, -500f), 1f).SetEase(Ease.InBack);
		CanvasGroup.DOFade(0f, 0.5f).OnComplete(delegate
		{
			kitchen.HasChanged = true;
			UnityEngine.Object.Destroy(base.gameObject);
		});
	}

	private void ShakeRecipeItem(float Duration)
	{
		RecipeItemRT.DOKill();
		RecipeItemRT.DOShakePosition(Duration, new Vector3(50f, 0f, 0f), 10, 0f);
		if (BurnFadeSequence != null)
		{
			BurnFadeSequence.Kill();
		}
		BurnFadeSequence = DOTween.Sequence();
		BurnFadeSequence.AppendInterval(Duration - 0.15f);
		BurnFadeSequence.Append(RecipeItemCG.DOFade(0f, 0.15f));
		BurnFadeSequence.Play();
	}

	private void Update()
	{
		if (MiniGamePaused || MonoSingleton<UIManager>.Instance.MenusBlocked)
		{
			return;
		}
		TrackerNormalisedPosition += Speed * Time.deltaTime;
		Tracker.transform.localPosition = new Vector3((0f - GameWidth) / 2f + GameWidth * Mathf.PingPong(TrackerNormalisedPosition, 1f), 0f);
		bool flag = false;
		if (SettingsManager.Settings.Accessibility.AutoCook)
		{
			if (_accessibleCooldown <= 0f)
			{
				if (Tracker.rectTransform.anchoredPosition.x < GameWidth / 2f - OverCookedRange && Tracker.rectTransform.anchoredPosition.x > (0f - GameWidth) / 2f + UnderCookedRange)
				{
					flag = true;
					_accessibleCooldown = 0.5f;
				}
			}
			else
			{
				_accessibleCooldown -= Time.deltaTime;
			}
		}
		else
		{
			flag = InputManager.Gameplay.GetInteractButtonDown();
		}
		if (!flag)
		{
			return;
		}
		UIManager.PlayAudio("event:/ui/arrow_change_selection");
		Tracker.transform.DOKill();
		Tracker.rectTransform.localScale = Vector3.one;
		Tracker.rectTransform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.5f);
		Background.rectTransform.DOKill();
		if (Tracker.rectTransform.anchoredPosition.x < (0f - GameWidth) / 2f + UnderCookedRange)
		{
			ShakeRecipeItem(0.3f);
			AudioManager.Instance.PlayOneShot("event:/Stings/generic_negative", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Failure);
			Background.rectTransform.DOShakePosition(0.5f, new Vector3(25f, 0f, 0f), 10, 0f);
			Flash.color = StaticColors.RedColor;
			Action onUnderCook = OnUnderCook;
			if (onUnderCook != null)
			{
				onUnderCook();
			}
			DidBurn = true;
		}
		else if (Tracker.rectTransform.anchoredPosition.x > GameWidth / 2f - OverCookedRange)
		{
			ControlPromptUI.DOKill();
			ControlPromptUI.DOScale(0.75f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
			AudioManager.Instance.PlayOneShot("event:/Stings/generic_negative", PlayerFarming.Instance.transform.position);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Failure);
			ShakeRecipeItem(0.3f);
			Background.rectTransform.DOShakePosition(0.5f, new Vector3(25f, 0f, 0f), 10, 0f);
			Flash.color = StaticColors.RedColor;
			Action onBurn = OnBurn;
			if (onBurn != null)
			{
				onBurn();
			}
			DidBurn = true;
		}
		else
		{
			ControlPromptUI.DOKill();
			ControlPromptUI.DOScale(0.75f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
			HideText(0.3f);
			Flash.color = StaticColors.OffWhiteColor;
			Action onCook = OnCook;
			if (onCook != null)
			{
				onCook();
			}
		}
		Flash.DOFade(0f, 0.5f);
		StartCoroutine(InputPause());
	}

	private IEnumerator InputPause()
	{
		MiniGamePaused = true;
		int count = StructureInfo.QueuedMeals.Count;
		yield return new WaitForSeconds(0.3f);
		ControlPromptUI.DOKill();
		ControlPromptUI.DOScale(1f, 0.5f).SetEase(Ease.OutQuart).SetUpdate(true);
		if (count > 0)
		{
			SetBarSizes();
			UpdateText();
			MiniGamePaused = false;
		}
		else
		{
			Close();
		}
	}
}
