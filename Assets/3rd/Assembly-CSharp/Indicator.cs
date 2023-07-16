using System;
using Lamb.UI;
using MMTools;
using Rewired;
using RewiredConsts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoSingleton<Indicator>
{
	public Image BG;

	public Image ContainerImage;

	public TextMeshProUGUI text;

	public TextMeshProUGUI SecondaryText;

	public TextMeshProUGUI Thirdtext;

	public TextMeshProUGUI Fourthtext;

	private float TargetWidth;

	private float TargetWidthSpeed;

	private float width;

	private float height;

	public CanvasGroup canvasGroup;

	private bool closing;

	public RectTransform ControlPrompt;

	public RectTransform ControlPromptContainer;

	public RectTransform SecondaryControlPrompt;

	public RectTransform ThirdControlPrompt;

	public RectTransform FourthControlPrompt;

	public float Progress;

	[SerializeField]
	private RadialProgress _radialProgress;

	public float Margin = 120f;

	public float ControlPromptSpacing = 15f;

	private float PromptDist;

	public RectTransform CenterObject;

	public bool HoldToInteract;

	public bool Interactable = true;

	public bool HasSecondaryInteraction;

	public bool SecondaryInteractable = true;

	public bool HasThirdInteraction;

	public bool ThirdInteractable = true;

	public bool FourthInteractable = true;

	public bool HasFourthInteraction;

	public GameObject ControlPromptUI;

	public MMControlPrompt primaryControlPrompt;

	public MMControlPrompt secondaryControlPrompt;

	public MMControlPrompt thirdControlPrompt;

	public MMControlPrompt fourthControlPrompt;

	[ActionIdProperty(typeof(RewiredConsts.Action))]
	public int Action = 9;

	public GameObject TopInfoContainer;

	public TMP_Text TopInfoText;

	public SimpleSFX sfx;

	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private RectTransform _container;

	[SerializeField]
	private CanvasGroup _controlPromptContainerCanvasGroup;

	[SerializeField]
	private LayoutElement _controlPromptLayoutElement;

	private Vector2 _cachedPosition;

	private Vector2 _cachedContainerPosition;

	private Vector2 Easing = new Vector2(0.2f, 0.55f);

	private float ClosingAlphaSpeed = 5f;

	private Vector3 WorldPosition;

	private Vector2 Shake;

	private float ShakeSpeed;

	public bool UpdatePosition = true;

	public bool WorldPositionShake = true;

	public float ShakeAmount = 25f;

	public float bounce = 0.4f;

	public float easing = 0.8f;

	private bool hidden;

	private float cacheAlpha;

	private bool PlacementObjectEnabled;

	public RectTransform RectTransform
	{
		get
		{
			return _rectTransform;
		}
	}

	public Vector2 CachedPosition
	{
		get
		{
			return _cachedPosition;
		}
	}

	public override void Start()
	{
		if (sfx == null)
		{
			sfx = GetComponent<SimpleSFX>();
		}
		_cachedPosition = _rectTransform.anchoredPosition;
		_cachedContainerPosition = _container.anchoredPosition;
		HideTopInfo();
	}

	private void OnEnable()
	{
		closing = false;
		canvasGroup.alpha = 0f;
		TargetWidth = (TargetWidthSpeed = 0f);
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnHoldActionToggleChanged = (Action<bool>)Delegate.Combine(instance.OnHoldActionToggleChanged, new Action<bool>(OnHoldActionToggleChanged));
		Reset();
	}

	private void OnDisable()
	{
		AccessibilityManager instance = Singleton<AccessibilityManager>.Instance;
		instance.OnHoldActionToggleChanged = (Action<bool>)Delegate.Remove(instance.OnHoldActionToggleChanged, new Action<bool>(OnHoldActionToggleChanged));
	}

	private void OnHoldActionToggleChanged(bool value)
	{
		_radialProgress.gameObject.SetActive(value);
	}

	public void Reset()
	{
		closing = false;
		ContainerImage.enabled = true;
		if (Interactable && text.text != "" && text.text != " ")
		{
			_controlPromptContainerCanvasGroup.alpha = 1f;
			_controlPromptLayoutElement.minWidth = 75f;
			_controlPromptLayoutElement.preferredWidth = 75f;
			if (HoldToInteract && SettingsManager.Settings.Accessibility.HoldActions)
			{
				float progress = (_radialProgress.Progress = 0f);
				Progress = progress;
				_radialProgress.gameObject.SetActive(true);
			}
			else
			{
				_radialProgress.gameObject.SetActive(false);
				float progress = (_radialProgress.Progress = 0f);
				Progress = progress;
			}
		}
		else if (_controlPromptContainerCanvasGroup.alpha == 1f)
		{
			_controlPromptContainerCanvasGroup.alpha = 0f;
			_controlPromptLayoutElement.minWidth = 0f;
			_controlPromptLayoutElement.preferredWidth = 0f;
			_radialProgress.gameObject.SetActive(false);
		}
		if (SecondaryInteractable && !string.IsNullOrEmpty(SecondaryText.text))
		{
			SecondaryControlPrompt.gameObject.SetActive(HasSecondaryInteraction);
		}
		else if (SecondaryControlPrompt.gameObject.activeSelf || !string.IsNullOrEmpty(SecondaryText.text))
		{
			SecondaryControlPrompt.gameObject.SetActive(false);
			SecondaryText.text = "";
		}
		if (ThirdInteractable && Thirdtext != null && !string.IsNullOrEmpty(Thirdtext.text))
		{
			ThirdControlPrompt.gameObject.SetActive(HasThirdInteraction);
		}
		else if (ThirdControlPrompt.gameObject.activeSelf || !string.IsNullOrEmpty(Thirdtext.text))
		{
			ThirdControlPrompt.gameObject.SetActive(false);
			Thirdtext.text = "";
		}
		if (FourthInteractable && Fourthtext != null && !string.IsNullOrEmpty(Fourthtext.text))
		{
			FourthControlPrompt.gameObject.SetActive(HasFourthInteraction);
		}
		else if (FourthControlPrompt.gameObject.activeSelf || !string.IsNullOrEmpty(Fourthtext.text))
		{
			FourthControlPrompt.gameObject.SetActive(false);
			Fourthtext.text = "";
		}
		if (string.IsNullOrEmpty(text.text) && string.IsNullOrEmpty(SecondaryText.text) && string.IsNullOrEmpty(Thirdtext.text) && string.IsNullOrEmpty(Fourthtext.text))
		{
			ContainerImage.enabled = false;
		}
	}

	public void ShowTopInfo(string text)
	{
		if ((bool)TopInfoContainer)
		{
			TopInfoContainer.SetActive(true);
			TopInfoText.text = text;
		}
	}

	public void HideTopInfo()
	{
		if ((bool)TopInfoContainer)
		{
			TopInfoContainer.SetActive(false);
		}
	}

	public void Deactivate()
	{
		if (!closing)
		{
			closing = true;
		}
	}

	public void Activate()
	{
		HideTopInfo();
		if ((bool)Interactor.CurrentInteraction)
		{
			Interactor.CurrentInteraction.HasChanged = true;
		}
	}

	private void SetSize()
	{
		PromptDist = text.preferredWidth / 2f + ControlPrompt.rect.width / 2f + ControlPromptSpacing;
		height = text.preferredHeight + 20f;
		BG.rectTransform.sizeDelta = new Vector2(width = text.preferredWidth + ControlPrompt.rect.width + Margin, height);
	}

	public void SetPosition(Vector3 WorldPosition)
	{
		this.WorldPosition = WorldPosition;
	}

	private void LateUpdate()
	{
		if (UpdatePosition)
		{
			_container.anchoredPosition = _cachedContainerPosition + Shake;
		}
	}

	public void PlayShake()
	{
		AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
		ShakeSpeed = ShakeAmount * (float)((!((double)UnityEngine.Random.value < 0.5)) ? 1 : (-1));
	}

	private void Update()
	{
		if (PlacementObject.Instance != null)
		{
			PlacementObjectEnabled = true;
			hidden = false;
			canvasGroup.alpha = 1f;
			return;
		}
		if (PlacementObjectEnabled)
		{
			Deactivate();
			PlacementObjectEnabled = false;
		}
		if (Time.timeScale == 0f)
		{
			hidden = true;
			cacheAlpha = canvasGroup.alpha;
			canvasGroup.alpha = 0f;
		}
		if (hidden)
		{
			if (Time.timeScale > 0f)
			{
				hidden = false;
				canvasGroup.alpha = cacheAlpha;
			}
		}
		else if (!closing && Interactor.CurrentInteraction != null)
		{
			if (canvasGroup.alpha < 1f)
			{
				canvasGroup.alpha += 20f * Time.deltaTime;
			}
			PromptDist = text.preferredWidth / 2f + ControlPrompt.rect.width / 2f + ControlPromptSpacing;
			if (_radialProgress.gameObject.activeSelf)
			{
				_radialProgress.Progress = Progress;
			}
			width = text.preferredWidth + ControlPrompt.rect.width + Margin;
			TargetWidthSpeed += (width - TargetWidth) * Easing.x;
			TargetWidth += (TargetWidthSpeed *= Easing.y);
			height = text.preferredHeight + 20f;
			BG.rectTransform.sizeDelta = new Vector2(TargetWidth, height);
		}
		else
		{
			if (canvasGroup.alpha > 0f)
			{
				canvasGroup.alpha -= ClosingAlphaSpeed * Time.deltaTime;
			}
			if (canvasGroup.alpha <= 0f)
			{
				text.text = "";
				base.gameObject.SetActive(false);
			}
		}
	}

	private void FixedUpdate()
	{
		ShakeSpeed += (0f - Shake.x) * bounce;
		Shake += new Vector2(ShakeSpeed *= easing, 0f);
	}
}
