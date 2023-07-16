using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using MMBiomeGeneration;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class HUD_Manager : BaseMonoBehaviour
{
	public static HUD_Manager Instance;

	public static Action OnShown;

	public static Action OnHidden;

	[SerializeField]
	private GameObject _baseDetailsContainer;

	public UI_Transitions BaseDetailsTransition;

	[SerializeField]
	private GameObject _dungeonDetailsContainer;

	public UI_Transitions FaithAmmoTransition;

	[SerializeField]
	private GameObject _miniMapContainer;

	public UI_Transitions MiniMapTransition;

	[SerializeField]
	private GameObject _timeContainer;

	public UI_Transitions TimeTransitions;

	[Space]
	[SerializeField]
	private CanvasGroup _topRightCanvasGroup;

	[SerializeField]
	private TMP_Text _currentDungeonModifierText;

	[SerializeField]
	private GameObject _damageMultiplierContainer;

	[SerializeField]
	private UI_Transitions _damageMultiplierTransition;

	[SerializeField]
	private TMP_Text _fleeceDamageMultiplierText;

	[SerializeField]
	private GameObject _healthContainer;

	[SerializeField]
	private UI_Transitions _healthTransitions;

	[SerializeField]
	private GameObject _xpBarContainer;

	public UI_Transitions XPBarTransitions;

	[SerializeField]
	private GameObject _returnToBaseContainer;

	public UI_Transitions ReturnToBaseTransitions;

	public UI_Transitions _curseTransitions;

	public UI_Transitions _weaponTransitions;

	public UI_Transitions _relicTransitions;

	public UI_Transitions _doctrineTransitions;

	[SerializeField]
	private GameObject returnToBaseContainer;

	[SerializeField]
	private GameObject _healContainer;

	[SerializeField]
	private GameObject _heavyAttacksContainer;

	public UI_Transitions HoldTransitions;

	[SerializeField]
	private NotificationCentre _notificatioCenter;

	[SerializeField]
	private UIObjectivesController _objectivesController;

	[SerializeField]
	private UI_Transitions _centerElementTransitions;

	[SerializeField]
	private UI_Transitions _editModeTransition;

	public static bool IsTransitioning;

	private Tween topRightTween;

	private string _localizedDamage;

	public GameObject BWImage;

	private Tween punchTween;

	public TMP_Text CurrentDungeonModifierText
	{
		get
		{
			return _currentDungeonModifierText;
		}
	}

	public bool Hidden { get; set; }

	private void Awake()
	{
		_fleeceDamageMultiplierText.text = "";
		_damageMultiplierContainer.SetActive(false);
		UpdateLocalisation();
		LocalizationManager.OnLocalizeEvent += UpdateLocalisation;
	}

	private void Start()
	{
		Hidden = false;
		Hide(true);
	}

	private void OnDestroy()
	{
		if ((bool)_currentDungeonModifierText)
		{
			_currentDungeonModifierText.text = "";
		}
		if ((bool)_fleeceDamageMultiplierText)
		{
			_fleeceDamageMultiplierText.text = "";
		}
		LocalizationManager.OnLocalizeEvent -= UpdateLocalisation;
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		PlayerFleeceManager.OnDamageMultiplierModified = (PlayerFleeceManager.DamageEvent)Delegate.Remove(PlayerFleeceManager.OnDamageMultiplierModified, new PlayerFleeceManager.DamageEvent(DamageMultiplierModified));
		TrinketManager.OnTrinketAdded -= TrinketModified;
		TrinketManager.OnTrinketRemoved -= TrinketModified;
	}

	public void Hide(bool Snap, int Delay = 1, bool both = false)
	{
		if (Hidden)
		{
			return;
		}
		IsTransitioning = true;
		if (Snap)
		{
			StopAllCoroutines();
			_notificatioCenter.Hide(true);
			_healthTransitions.hideBar();
			BaseDetailsTransition.hideBar();
			FaithAmmoTransition.hideBar();
			XPBarTransitions.hideBar();
			TimeTransitions.hideBar();
			MiniMapTransition.hideBar();
			HoldTransitions.hideBar();
			_damageMultiplierTransition.hideBar();
			ReturnToBaseTransitions.hideBar();
			_relicTransitions.hideBar();
			if (_curseTransitions != null)
			{
				_curseTransitions.hideBar();
			}
			if (_weaponTransitions != null)
			{
				_weaponTransitions.hideBar();
			}
			if (_doctrineTransitions != null)
			{
				_doctrineTransitions.hideBar();
			}
			if (_relicTransitions != null)
			{
				_relicTransitions.hideBar();
			}
			if ((bool)_dungeonDetailsContainer)
			{
				_dungeonDetailsContainer.SetActive(false);
			}
			if (FaithAmmoTransition != null)
			{
				FaithAmmoTransition.hideBar();
			}
			IsTransitioning = false;
			if ((bool)_centerElementTransitions)
			{
				_centerElementTransitions.hideBar();
			}
			returnToBaseContainer.SetActive(false);
			_healContainer.SetActive(false);
			_heavyAttacksContainer.SetActive(false);
		}
		else if (!both)
		{
			if (!GameManager.IsDungeon(PlayerFarming.Location))
			{
				StartCoroutine(HideBaseHUD(Delay));
			}
			else
			{
				StartCoroutine(HideDungeonHUD(Delay));
			}
		}
		else
		{
			StartCoroutine(HideBaseHUD(Delay));
			StartCoroutine(HideDungeonHUD(Delay));
		}
		if ((bool)_objectivesController)
		{
			_objectivesController.Hide(Snap);
		}
		if (NotificationCentre.Instance != null)
		{
			NotificationCentre.Instance.Hide(Snap);
		}
		Hidden = true;
	}

	private void OnEnable()
	{
		Instance = this;
		IsTransitioning = false;
		Hide(true);
		_editModeTransition.hideBar();
		PlayerFleeceManager.OnDamageMultiplierModified = (PlayerFleeceManager.DamageEvent)Delegate.Combine(PlayerFleeceManager.OnDamageMultiplierModified, new PlayerFleeceManager.DamageEvent(DamageMultiplierModified));
		TrinketManager.OnTrinketAdded += TrinketModified;
		TrinketManager.OnTrinketRemoved += TrinketModified;
	}

	public void ShowEditMode(bool show)
	{
		if (show)
		{
			_editModeTransition.MoveBackInFunction();
		}
		else
		{
			_editModeTransition.MoveBackOutFunction();
		}
	}

	public void ShowBW(float Duration, float StartValue, float EndValue)
	{
		StopCoroutine(FadeInBW(Duration, StartValue, EndValue));
		StartCoroutine(FadeInBW(Duration, StartValue, EndValue));
	}

	private IEnumerator FadeInBW(float Duration, float StartValue, float EndValue)
	{
		BWImage.SetActive(true);
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			BWImage.GetComponent<CanvasGroup>().alpha = Mathf.SmoothStep(StartValue, EndValue, Progress / Duration);
			yield return null;
		}
		BWImage.GetComponent<CanvasGroup>().alpha = EndValue;
		if (EndValue == 0f)
		{
			BWImage.SetActive(false);
		}
	}

	public void Show(int Delay = 1, bool Force = false)
	{
		if (!Hidden && !Force)
		{
			return;
		}
		if (LetterBox.IsPlaying)
		{
			StartCoroutine(FrameDelay(delegate
			{
				Show(Delay, Force);
			}));
			return;
		}
		IsTransitioning = true;
		_healthContainer.SetActive(DataManager.Instance.dungeonRun > 0);
		_baseDetailsContainer.SetActive(true);
		_dungeonDetailsContainer.SetActive(true);
		_timeContainer.SetActive(true);
		_xpBarContainer.SetActive(true);
		_miniMapContainer.SetActive(true);
		if (_curseTransitions != null)
		{
			_curseTransitions.hideBar();
		}
		if (_weaponTransitions != null)
		{
			_weaponTransitions.hideBar();
		}
		if (_doctrineTransitions != null)
		{
			_doctrineTransitions.hideBar();
		}
		if (_relicTransitions != null)
		{
			_relicTransitions.hideBar();
		}
		_currentDungeonModifierText.text = "";
		_currentDungeonModifierText.gameObject.SetActive(false);
		float num = -0.5f;
		if ((bool)returnToBaseContainer)
		{
			returnToBaseContainer.SetActive(UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ability_TeleportHome) && AbleToMeditateBackToBase() && !DungeonSandboxManager.Active);
			if (returnToBaseContainer.activeSelf && BiomeGenerator.RevealHudAbilityIcons)
			{
				returnToBaseContainer.GetComponent<HUD_AbilityIcon>().Play(num += 0.5f);
			}
		}
		if ((bool)_heavyAttacksContainer)
		{
			_heavyAttacksContainer.SetActive(true);
		}
		if ((bool)_healContainer)
		{
			_healContainer.SetActive(IsHoldToHealActive());
			if (_healContainer.activeSelf && BiomeGenerator.RevealHudAbilityIcons)
			{
				_healContainer.GetComponent<HUD_AbilityIcon>().Play(num += 0.5f);
			}
		}
		BiomeGenerator.RevealHudAbilityIcons = false;
		_healthTransitions.hideBar();
		BaseDetailsTransition.hideBar();
		FaithAmmoTransition.hideBar();
		XPBarTransitions.hideBar();
		TimeTransitions.hideBar();
		MiniMapTransition.hideBar();
		HoldTransitions.hideBar();
		_damageMultiplierTransition.hideBar();
		_notificatioCenter.Hide(true);
		_fleeceDamageMultiplierText.text = "";
		_damageMultiplierContainer.SetActive(false);
		_objectivesController.Show();
		if ((bool)_centerElementTransitions)
		{
			_centerElementTransitions.MoveBackInFunction();
		}
		if (NotificationCentre.Instance != null)
		{
			NotificationCentre.Instance.Show();
		}
		if (GameManager.IsDungeon(PlayerFarming.Location))
		{
			StartCoroutine(ShowDungeonHUD(Delay));
		}
		else
		{
			StartCoroutine(ShowBaseHUD(Delay));
		}
		Hidden = false;
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator ShowDungeonHUD(int Delay)
	{
		_healthContainer.SetActive(PlayerFarming.Location != FollowerLocation.IntroDungeon);
		_dungeonDetailsContainer.SetActive(DataManager.Instance.EnabledSpells && PlayerFarming.Location != FollowerLocation.IntroDungeon);
		_timeContainer.SetActive(!TimeManager.PauseGameTime);
		_xpBarContainer.SetActive(DataManager.Instance.XPEnabled && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten));
		_miniMapContainer.SetActive(true);
		_baseDetailsContainer.SetActive(false);
		yield return new WaitForSecondsRealtime(Delay);
		if (PlayerFarming.Location != FollowerLocation.IntroDungeon)
		{
			_notificatioCenter.Show();
			_healthTransitions.MoveBackInFunction();
			ReturnToBaseTransitions.MoveBackInFunction();
			_curseTransitions.MoveBackInFunction();
			_weaponTransitions.MoveBackInFunction();
			_doctrineTransitions.MoveBackInFunction();
			_relicTransitions.MoveBackInFunction();
			if (DataManager.Instance.EnabledSpells)
			{
				_dungeonDetailsContainer.SetActive(true);
				FaithAmmoTransition.MoveBackInFunction();
			}
			if (DataManager.Instance.XPEnabled && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten))
			{
				_xpBarContainer.SetActive(true);
			}
			if (!TimeManager.PauseGameTime)
			{
				_timeContainer.SetActive(true);
				TimeTransitions.MoveBackInFunction();
			}
			HoldTransitions.MoveBackInFunction();
		}
		MiniMapTransition.MoveBackInFunction();
		_damageMultiplierContainer.SetActive(DataManager.Instance.PlayerFleece == 1 && GameManager.IsDungeon(PlayerFarming.Location));
		_damageMultiplierTransition.MoveBackInFunction();
		if (_damageMultiplierContainer.activeSelf)
		{
			DamageMultiplierModified(PlayerFleeceManager.GetWeaponDamageMultiplier());
		}
		_currentDungeonModifierText.text = DungeonModifier.GetCurrentModifierText();
		_currentDungeonModifierText.gameObject.SetActive(!_currentDungeonModifierText.text.IsNullOrEmpty());
		if (_centerElementTransitions != null)
		{
			_centerElementTransitions.MoveBackInFunction();
		}
		yield return new WaitForSecondsRealtime(1.5f);
		IsTransitioning = false;
		Action onShown = OnShown;
		if (onShown != null)
		{
			onShown();
		}
	}

	private IEnumerator ShowBaseHUD(int Delay)
	{
		_baseDetailsContainer.SetActive(true);
		_xpBarContainer.SetActive(DataManager.Instance.XPEnabled && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten));
		_timeContainer.SetActive(!TimeManager.PauseGameTime);
		_dungeonDetailsContainer.SetActive(false);
		yield return new WaitForSeconds(Delay);
		_notificatioCenter.Show();
		_healthTransitions.MoveBackInFunction();
		_baseDetailsContainer.SetActive(true);
		BaseDetailsTransition.MoveBackInFunction();
		if (DataManager.Instance.XPEnabled && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten))
		{
			_xpBarContainer.SetActive(true);
			XPBarTransitions.MoveBackInFunction();
		}
		if (!TimeManager.PauseGameTime)
		{
			_timeContainer.SetActive(true);
			TimeTransitions.MoveBackInFunction();
		}
		yield return new WaitForSeconds(1f);
		IsTransitioning = false;
		Action onShown = OnShown;
		if (onShown != null)
		{
			onShown();
		}
	}

	private IEnumerator HideDungeonHUD(int Delay)
	{
		_baseDetailsContainer.SetActive(false);
		yield return new WaitForSeconds(Delay);
		_healthTransitions.MoveBackOutFunction();
		_notificatioCenter.Hide();
		ReturnToBaseTransitions.MoveBackOutFunction();
		_curseTransitions.MoveBackOutFunction();
		_relicTransitions.MoveBackOutFunction();
		_weaponTransitions.MoveBackOutFunction();
		_doctrineTransitions.MoveBackOutFunction();
		_relicTransitions.MoveBackOutFunction();
		if (DataManager.Instance.EnabledSpells)
		{
			FaithAmmoTransition.MoveBackOutFunction();
		}
		if (DataManager.Instance.XPEnabled && (GameManager.HasUnlockAvailable() || DataManager.Instance.DeathCatBeaten))
		{
			XPBarTransitions.MoveBackOutFunction();
		}
		if (!TimeManager.PauseGameTime)
		{
			TimeTransitions.MoveBackOutFunction();
		}
		MiniMapTransition.MoveBackOutFunction();
		HoldTransitions.MoveBackOutFunction();
		_centerElementTransitions.MoveBackOutFunction();
		_damageMultiplierTransition.MoveBackOutFunction();
		yield return new WaitForSeconds(1f);
		IsTransitioning = false;
		Action onHidden = OnHidden;
		if (onHidden != null)
		{
			onHidden();
		}
	}

	private IEnumerator HideBaseHUD(int Delay)
	{
		_dungeonDetailsContainer.SetActive(false);
		returnToBaseContainer.SetActive(false);
		_healContainer.SetActive(false);
		_heavyAttacksContainer.SetActive(false);
		yield return new WaitForSecondsRealtime(Delay);
		BaseDetailsTransition.MoveBackOutFunction();
		XPBarTransitions.MoveBackOutFunction();
		if (_curseTransitions != null)
		{
			_curseTransitions.MoveBackOutFunction();
		}
		if (_weaponTransitions != null)
		{
			_weaponTransitions.MoveBackOutFunction();
		}
		if (_doctrineTransitions != null)
		{
			_doctrineTransitions.MoveBackOutFunction();
		}
		if (_relicTransitions != null)
		{
			_relicTransitions.MoveBackOutFunction();
		}
		if (!TimeManager.PauseGameTime)
		{
			TimeTransitions.MoveBackOutFunction();
		}
		if (_centerElementTransitions != null)
		{
			_centerElementTransitions.MoveBackOutFunction();
		}
		_healthTransitions.MoveBackOutFunction();
		_notificatioCenter.Hide();
		yield return new WaitForSeconds(1f);
		IsTransitioning = false;
		Action onHidden = OnHidden;
		if (onHidden != null)
		{
			onHidden();
		}
	}

	private void UpdateLocalisation()
	{
		_localizedDamage = ScriptLocalization.UI_WeaponSelect.Damage;
		_currentDungeonModifierText.text = DungeonModifier.GetCurrentModifierText();
		_currentDungeonModifierText.gameObject.SetActive(!_currentDungeonModifierText.text.IsNullOrEmpty());
	}

	public void HideTopRight()
	{
		if (topRightTween != null && topRightTween.active)
		{
			topRightTween.Complete();
		}
		topRightTween = _topRightCanvasGroup.DOFade(0f, 1f);
		_objectivesController.HideAllExcluding("Objectives/GroupTitles/Challenge");
	}

	public void ShowTopRight()
	{
		if (topRightTween != null && topRightTween.active)
		{
			topRightTween.Complete();
		}
		topRightTween = _topRightCanvasGroup.DOFade(1f, 1f);
		_objectivesController.ShowAll();
	}

	private void DamageMultiplierModified(float damageMultiplier)
	{
		if (punchTween != null && punchTween.active)
		{
			punchTween.Complete();
		}
		double num = Math.Round(damageMultiplier, 2) * 100.0;
		_fleeceDamageMultiplierText.text = string.Format("{0} +{1}%", _localizedDamage, num);
		if (damageMultiplier == 1f)
		{
			punchTween = _fleeceDamageMultiplierText.transform.DOShakePosition(1f, new Vector3(10f, 0f));
		}
		else
		{
			punchTween = _fleeceDamageMultiplierText.transform.DOPunchScale(Vector3.one * 0.25f, 0.25f);
		}
	}

	private void TrinketModified(TarotCards.Card card)
	{
		_healContainer.SetActive(IsHoldToHealActive());
	}

	private bool IsHoldToHealActive()
	{
		if (DataManager.Instance.PlayerFleece != 8 || !GameManager.IsDungeon(PlayerFarming.Location))
		{
			return TrinketManager.HasTrinket(TarotCards.Card.HoldToHeal);
		}
		return true;
	}

	private bool AbleToMeditateBackToBase()
	{
		if (RespawnRoomManager.Instance != null && RespawnRoomManager.Instance.gameObject.activeSelf)
		{
			return false;
		}
		FollowerLocation location = PlayerFarming.Location;
		if ((uint)(location - 7) <= 3u)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (CultFaithManager.Instance != null && !CultFaithManager.Instance.gameObject.activeInHierarchy)
		{
			CultFaithManager.Instance.BarController.Update();
		}
	}
}
