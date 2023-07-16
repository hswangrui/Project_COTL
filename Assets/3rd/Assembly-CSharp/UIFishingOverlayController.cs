using System;
using DG.Tweening;
using FMOD.Studio;
using Lamb.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIFishingOverlayController : UIMenuBase
{
	[Serializable]
	public class Difficulty
	{
		public float sectionMinGap;

		public float sectionMaxGap;

		public float sectionMoveSpeed;

		public Vector2 sectionRange;

		public Vector2 sectionRandomTimer;
	}

	[SerializeField]
	private Vector3 offset;

	[SerializeField]
	private CanvasGroup castingCanvasGroup;

	[SerializeField]
	private CanvasGroup reelingCanvasGroup;

	[SerializeField]
	private Transform castingParent;

	[SerializeField]
	private Image castingStrengthBar;

	[SerializeField]
	private Transform reelingParent;

	[SerializeField]
	private RectTransform reelingNeedle;

	[SerializeField]
	private Image reelingNeedleIcon;

	[SerializeField]
	private Rigidbody2D needleRigidbody;

	[SerializeField]
	private Image reelingBarBG;

	[SerializeField]
	private Transform reelingBarParent;

	[SerializeField]
	private RectTransform targetSection;

	[Space]
	[SerializeField]
	private Difficulty[] difficulties;

	[SerializeField]
	private RectTransform holdButtonAdjust;

	[SerializeField]
	private GameObject _reelButotnContainer;

	[SerializeField]
	private RectTransform holdButtonCast;

	[SerializeField]
	private GameObject _castButtonContainer;

	private StateMachine.State currentState;

	private GameObject lockPosition;

	private int difficulty;

	private Vector2 sectionMinVelocity;

	private Vector2 sectionMaxVelocity;

	private float sectionTimer;

	private float sectionDirection;

	private float sectionIncrease;

	private float sectionMin;

	private float sectionMax;

	private int rapidMode = -1;

	private float rapidModeChance;

	private float rapidModeTimer;

	private Vector2 rapidModeDuration = new Vector2(4f, 7.5f);

	private const float needleUpForce = 2000f;

	private const float needleDownForce = -50000f;

	private Vector3[] sectionWorldCorners = new Vector3[4];

	private Vector3[] needleWorldCorners = new Vector3[4];

	private EventInstance loopingSoundInstance;

	private Material _needleHighlightedMaterial;

	private bool cacheResult;

	private bool _changedUpState;

	private bool _changedDownState;

	private Difficulty CurrentDifficulty
	{
		get
		{
			return difficulties[difficulty];
		}
	}

	protected override bool _addToActiveMenus
	{
		get
		{
			return false;
		}
	}

	public void CastingButtonDown(bool down)
	{
		if (down)
		{
			Transform parent = holdButtonCast.transform.parent;
			parent.DOKill();
			parent.DOScale(1f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
			Transform parent2 = holdButtonAdjust.transform.parent;
			parent2.DOKill();
			parent2.DOScale(1f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
		}
		else
		{
			Transform parent3 = holdButtonCast.transform.parent;
			parent3.DOKill();
			parent3.DOScale(0f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
			Transform parent4 = holdButtonAdjust.transform.parent;
			parent4.DOKill();
			parent4.DOScale(0f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
		}
	}

	public override void Awake()
	{
		base.Awake();
		_needleHighlightedMaterial = reelingNeedleIcon.material;
	}

	protected override void OnShowStarted()
	{
		targetSection.anchorMin = new Vector2(targetSection.anchorMin.x, 0.4f);
		targetSection.anchorMax = new Vector2(targetSection.anchorMax.x, 0.5f);
		reelingNeedle.anchoredPosition = new Vector2(reelingNeedle.anchoredPosition.x, 0f);
		_castButtonContainer.SetActive(!SettingsManager.Settings.Accessibility.AutoFish);
		_reelButotnContainer.SetActive(!SettingsManager.Settings.Accessibility.AutoFish);
	}

	protected override void OnHideCompleted()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void SetState(StateMachine.State state)
	{
		currentState = state;
		castingCanvasGroup.alpha = 0f;
		reelingCanvasGroup.alpha = 0f;
		if (state != StateMachine.State.Casting)
		{
			castingCanvasGroup.DOFade(0f, 1f);
		}
		else
		{
			castingCanvasGroup.DOFade(1f, 1f);
		}
		if (state != StateMachine.State.Attacking)
		{
			reelingCanvasGroup.DOFade(0f, 1f);
		}
		else
		{
			reelingCanvasGroup.DOFade(1f, 1f);
		}
		if (state == StateMachine.State.Attacking)
		{
			rapidMode = -1;
		}
	}

	public void UpdateCastingStrength(float strength)
	{
		castingStrengthBar.fillAmount = strength;
	}

	public void SetReelingDifficulty(int difficulty)
	{
		this.difficulty = difficulty;
		sectionDirection = ((UnityEngine.Random.Range(0f, 1f) > 0.5f) ? 1 : (-1));
		rapidModeChance = 0.5f;
		sectionMin = 0.5f - CurrentDifficulty.sectionMinGap / 2f;
		sectionMax = 0.5f + CurrentDifficulty.sectionMaxGap / 2f;
	}

	public void UpdateReelBar(float reeledAmount)
	{
		if (reeledAmount > 0.7f && rapidMode == -1)
		{
			if (UnityEngine.Random.Range(0f, 1f) > rapidModeChance)
			{
				BeginRapidMode();
			}
			else
			{
				rapidMode = 0;
			}
		}
	}

	private void BeginRapidMode()
	{
		float num = UnityEngine.Random.Range(rapidModeDuration.x, rapidModeDuration.y);
		rapidModeTimer = Time.time + num;
		SetReelingDifficulty(difficulty + 1);
		rapidMode = 1;
		reelingBarParent.DOShakeScale(num, 0.05f, 10, 90f, false).SetEase(Ease.Linear);
	}

	public bool IsNeedleWithinSection()
	{
		targetSection.GetWorldCorners(sectionWorldCorners);
		reelingNeedle.GetWorldCorners(needleWorldCorners);
		Vector3 vector = sectionWorldCorners[0];
		Vector3 vector2 = sectionWorldCorners[1];
		Vector3 vector3 = needleWorldCorners[0];
		Vector3 vector4 = needleWorldCorners[1];
		bool result = (vector3.y > vector.y && vector4.y < vector2.y) || (vector3.y <= vector2.y && vector4.y >= vector.y) || (vector4.y >= vector.y && vector4.y <= vector2.y);
		reelingBarBG.enabled = result;
		return result;
	}

	private void FixedUpdate()
	{
		if (currentState == StateMachine.State.Attacking)
		{
			if (InputManager.Gameplay.GetInteractButtonHeld())
			{
				_changedDownState = false;
				needleRigidbody.AddForce(Vector2.up * 2000f, ForceMode2D.Force);
				if (!_changedUpState)
				{
					Transform parent = holdButtonCast.transform.parent;
					parent.DOKill();
					parent.DOScale(0.75f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
					Transform parent2 = holdButtonAdjust.transform.parent;
					parent2.DOKill();
					parent2.DOScale(0.75f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
					MMVibrate.StopRumble();
					MMVibrate.RumbleContinuous(0.1f, 0.25f);
					AudioManager.Instance.PlayOneShot("event:/fishing/caught_something_tap", PlayerFarming.Instance.transform.position);
					reelingNeedleIcon.material = _needleHighlightedMaterial;
					_changedUpState = true;
				}
			}
			else
			{
				_changedUpState = false;
				needleRigidbody.AddForce(Vector2.up * -50000f * Time.deltaTime);
				if (!_changedDownState)
				{
					MMVibrate.StopRumble();
					MMVibrate.RumbleContinuous(0f, 0.025f);
					Transform parent3 = holdButtonCast.transform.parent;
					parent3.DOKill();
					parent3.DOScale(1f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
					Transform parent4 = holdButtonAdjust.transform.parent;
					parent4.DOKill();
					parent4.DOScale(1f, 0.33f).SetEase(Ease.OutQuart).SetUpdate(true);
					reelingNeedleIcon.material = null;
					_changedDownState = true;
				}
			}
			if (reelingNeedle.anchoredPosition.y >= 100f || reelingNeedle.anchoredPosition.y <= -100f)
			{
				needleRigidbody.velocity = Vector2.zero;
				reelingNeedle.anchoredPosition = new Vector2(reelingNeedle.anchoredPosition.x, Mathf.Clamp(reelingNeedle.anchoredPosition.y, -100f, 100f));
			}
			if (GameManager.GetInstance().CurrentTime > sectionTimer)
			{
				if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
				{
					sectionDirection *= -1f;
				}
				sectionTimer = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(CurrentDifficulty.sectionRandomTimer.x, CurrentDifficulty.sectionRandomTimer.y);
				sectionIncrease = UnityEngine.Random.Range(CurrentDifficulty.sectionRange.x, CurrentDifficulty.sectionRange.y);
			}
			sectionMin = Mathf.Clamp(sectionMin + sectionIncrease * sectionDirection, Mathf.Clamp01(sectionMax - CurrentDifficulty.sectionMaxGap), Mathf.Clamp01(sectionMax - CurrentDifficulty.sectionMinGap));
			sectionMax = Mathf.Clamp(sectionMax + sectionIncrease * sectionDirection, Mathf.Clamp01(sectionMin + CurrentDifficulty.sectionMinGap), Mathf.Clamp01(sectionMin + CurrentDifficulty.sectionMaxGap));
			sectionIncrease = Mathf.Clamp01(sectionIncrease - Time.deltaTime);
			targetSection.anchorMin = Vector2.SmoothDamp(targetSection.anchorMin, new Vector2(targetSection.anchorMin.x, sectionMin), ref sectionMinVelocity, CurrentDifficulty.sectionMoveSpeed);
			targetSection.anchorMax = Vector2.SmoothDamp(targetSection.anchorMax, new Vector2(targetSection.anchorMax.x, sectionMax), ref sectionMaxVelocity, CurrentDifficulty.sectionMoveSpeed);
			if (SettingsManager.Settings.Accessibility.AutoFish)
			{
				needleRigidbody.velocity = Vector2.zero;
				reelingNeedle.position = targetSection.position;
			}
			if (rapidMode == 1 && Time.time > rapidModeTimer)
			{
				SetReelingDifficulty(difficulty - 1);
				rapidMode = 0;
			}
		}
		else
		{
			MMVibrate.StopRumble();
		}
	}
}
