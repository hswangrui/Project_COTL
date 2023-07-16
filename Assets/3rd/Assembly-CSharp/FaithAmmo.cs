using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FaithAmmo : BaseMonoBehaviour
{
	[Serializable]
	public struct BiomeColour
	{
		public Color Color;

		public FollowerLocation Location;
	}

	public GameObject Container;

	public Image Background;

	public static Action OnAmmoChanged;

	public static Action OnAmmoCountChanged;

	public static Action OnCantAfford;

	public TextMeshProUGUI AmmoText;

	public Image AmmoGlow;

	private UI_Transitions transition;

	private static float _Ammo = 54f;

	public Transform ProgressBar;

	public Image WhiteFlash;

	public static FaithAmmo Instance;

	public GameObject[] RedGlow;

	public List<BiomeColour> Colours;

	private bool IsOnTotalAmmoShakePerformed;

	public static float Ammo
	{
		get
		{
			return _Ammo;
		}
		set
		{
			int num = Mathf.FloorToInt(Ammo / (float)PlayerSpells.AmmoCost);
			int num2 = Mathf.FloorToInt(value / (float)PlayerSpells.AmmoCost);
			if (value < (float)PlayerSpells.AmmoCost && _Ammo >= (float)PlayerSpells.AmmoCost)
			{
				PlayerFarming.Instance.SetSkin(true);
				if (Instance != null)
				{
					GameObject[] redGlow = Instance.RedGlow;
					for (int i = 0; i < redGlow.Length; i++)
					{
						redGlow[i].SetActive(false);
					}
				}
			}
			if (value >= (float)PlayerSpells.AmmoCost && _Ammo < (float)PlayerSpells.AmmoCost && (bool)PlayerFarming.Instance)
			{
				PlayerFarming.Instance.SetSkin(false);
				PlayerFarming.Instance.growAndFade.Play();
				if (Instance != null)
				{
					GameObject[] redGlow = Instance.RedGlow;
					for (int i = 0; i < redGlow.Length; i++)
					{
						redGlow[i].SetActive(true);
					}
				}
			}
			_Ammo = Mathf.Clamp(value, 0f, Total);
			Action onAmmoChanged = OnAmmoChanged;
			if (onAmmoChanged != null)
			{
				onAmmoChanged();
			}
			if (num != num2)
			{
				Action onAmmoCountChanged = OnAmmoCountChanged;
				if (onAmmoCountChanged != null)
				{
					onAmmoCountChanged();
				}
			}
		}
	}

	public static float Total
	{
		get
		{
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_Ammo_1))
			{
				return 180f;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_Ammo_2))
			{
				return 225f;
			}
			if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_Ammo_3))
			{
				return 270f;
			}
			return 135f;
		}
	}

	public bool DoFlash { get; set; } = true;


	private void Start()
	{
		Ammo = Total;
	}

	private void Awake()
	{
		Instance = this;
		transition = GetComponent<UI_Transitions>();
	}

	private void DoShake()
	{
		Container.transform.DOKill();
		Container.transform.localPosition = transition.StartPos;
		Container.transform.DOShakePosition(0.5f, new Vector3(15f, 0f), 10, 0f);
	}

	private void OnEnable()
	{
		PlayerFarming.OnGetBlackSoul += OnGetSoul;
		OnAmmoChanged = (Action)Delegate.Combine(OnAmmoChanged, new Action(UpdateBar));
		UpdateBar();
		if (Colours.Count <= 0)
		{
			return;
		}
		Background.color = Colours[0].Color;
		foreach (BiomeColour colour in Colours)
		{
			if (colour.Location == PlayerFarming.Location)
			{
				Background.color = colour.Color;
				break;
			}
		}
	}

	private void OnDisable()
	{
		Container.transform.DOKill();
		AmmoGlow.DOKill();
		PlayerFarming.OnGetBlackSoul -= OnGetSoul;
		OnAmmoChanged = (Action)Delegate.Remove(OnAmmoChanged, new Action(UpdateBar));
	}

	private void OnDestroy()
	{
		if (Instance != null)
		{
			Instance.DOKill();
		}
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public static bool CanAfford(float Amount)
	{
		return Ammo - Amount >= 0f;
	}

	public static bool UseAmmo(float Amount, bool curseAttack = true)
	{
		if (!CanAfford(Amount))
		{
			if (Instance != null)
			{
				Instance.StartCoroutine(Instance.WhiteFlashRoutine());
				Instance.DoShake();
			}
			if (curseAttack)
			{
				Action onCantAfford = OnCantAfford;
				if (onCantAfford != null)
				{
					onCantAfford();
				}
			}
			return false;
		}
		Ammo -= Amount;
		return true;
	}

	public static void Flash()
	{
		if (Instance != null)
		{
			Instance.StartCoroutine(Instance.WhiteFlashRoutine());
		}
	}

	private void OnGetSoul(int DeltaValue)
	{
		if (Ammo < Total)
		{
			Ammo += DeltaValue * 2;
			StartCoroutine(WhiteFlashRoutine());
			IsOnTotalAmmoShakePerformed = false;
		}
		if (Ammo >= Total && !IsOnTotalAmmoShakePerformed)
		{
			StartCoroutine(WhiteFlashRoutine());
			DoShake();
			IsOnTotalAmmoShakePerformed = true;
		}
	}

	public static void Reload()
	{
		Ammo = Total;
	}

	private void UpdateBar()
	{
		ProgressBar.transform.localScale = new Vector3(1f, 1f - (0.2f + Ammo / Total * 0.8f));
		AmmoText.text = string.Format("{0} {1}", Mathf.FloorToInt(Ammo / (float)PlayerSpells.AmmoCost), "<sprite name=\"icon_UI_Curse\">");
		if (Ammo >= Total)
		{
			if (!AmmoGlow.gameObject.activeSelf)
			{
				AmmoGlow.gameObject.SetActive(true);
				AmmoGlow.color = new Color(1f, 1f, 1f, 0f);
				AmmoGlow.DOKill();
				AmmoGlow.DOFade(1f, 1f);
			}
		}
		else
		{
			if (!AmmoGlow.gameObject.activeSelf)
			{
				return;
			}
			AmmoGlow.DOKill();
			AmmoGlow.DOFade(0f, 1f).OnComplete(delegate
			{
				if (Ammo < Total)
				{
					AmmoGlow.gameObject.SetActive(false);
				}
			});
		}
	}

	private IEnumerator WhiteFlashRoutine()
	{
		if (!DoFlash)
		{
			yield break;
		}
		WhiteFlash.enabled = true;
		float Progress = 0f;
		float Duration = 0.3f;
		Color StartColor = new Color(1f, 1f, 1f, 0.4f);
		Color TargetColor = new Color(1f, 1f, 1f, 0f);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			WhiteFlash.color = Color.Lerp(StartColor, TargetColor, Progress / Duration);
			yield return null;
		}
		WhiteFlash.enabled = false;
	}
}
