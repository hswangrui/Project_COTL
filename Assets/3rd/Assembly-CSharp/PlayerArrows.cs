using UnityEngine;

public class PlayerArrows : BaseMonoBehaviour
{
	public delegate void AmmoUpdated(PlayerArrows playerArrows);

	public GrowAndFade growAndFade;

	private bool _Reloading;

	public float ReloadProgress;

	public float ReloadTarget = 2f;

	private bool GiveExtraSpirit;

	public Vector2 TargetRange = new Vector2(0.2f, 0.35f);

	private float ReloadSpeed = 1f;

	private int _totalSpiritAmmo;

	public int PLAYER_ARROW_AMMO
	{
		get
		{
			return DataManager.Instance.PLAYER_ARROW_AMMO;
		}
		set
		{
			DataManager.Instance.PLAYER_ARROW_AMMO = value;
			ReloadSpeed = ((!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Combat_Arrows2)) ? 1 : 2);
			if (DataManager.Instance.PLAYER_ARROW_AMMO > DataManager.Instance.PLAYER_ARROW_TOTAL_AMMO)
			{
				DataManager.Instance.PLAYER_ARROW_AMMO = DataManager.Instance.PLAYER_ARROW_TOTAL_AMMO;
			}
			if (DataManager.Instance.PLAYER_ARROW_AMMO <= 0)
			{
				DataManager.Instance.PLAYER_ARROW_AMMO = 0;
				AmmoUpdated onNoAmmoShake = PlayerArrows.OnNoAmmoShake;
				if (onNoAmmoShake != null)
				{
					onNoAmmoShake(this);
				}
				PlayerFarming.Instance.simpleSpineAnimator.SetSkin("Lamb_BW");
				if (GiveExtraSpirit)
				{
					GiveExtraSpirit = false;
					int pLAYER_SPIRIT_TOTAL_AMMO = PLAYER_SPIRIT_TOTAL_AMMO - 1;
					PLAYER_SPIRIT_TOTAL_AMMO = pLAYER_SPIRIT_TOTAL_AMMO;
					AmmoUpdated onAmmoUpdated = PlayerArrows.OnAmmoUpdated;
					if (onAmmoUpdated != null)
					{
						onAmmoUpdated(this);
					}
				}
			}
			else
			{
				Reloading = false;
				if (DataManager.Instance.PLAYER_ARROW_AMMO < PLAYER_ARROW_TOTAL_AMMO)
				{
					Reloading = true;
				}
				AmmoUpdated onAmmoUpdated2 = PlayerArrows.OnAmmoUpdated;
				if (onAmmoUpdated2 != null)
				{
					onAmmoUpdated2(this);
				}
			}
		}
	}

	public int PLAYER_ARROW_TOTAL_AMMO
	{
		get
		{
			return DataManager.Instance.PLAYER_ARROW_TOTAL_AMMO;
		}
		set
		{
			DataManager.Instance.PLAYER_ARROW_TOTAL_AMMO = value;
			AmmoUpdated onAmmoUpdated = PlayerArrows.OnAmmoUpdated;
			if (onAmmoUpdated != null)
			{
				onAmmoUpdated(this);
			}
		}
	}

	public bool Reloading
	{
		get
		{
			return _Reloading;
		}
		set
		{
			if (_Reloading != value && value)
			{
				ReloadProgress = 0f;
				GiveExtraSpirit = false;
				AmmoUpdated onBeginReloading = PlayerArrows.OnBeginReloading;
				if (onBeginReloading != null)
				{
					onBeginReloading(this);
				}
			}
			_Reloading = value;
		}
	}

	public int PLAYER_SPIRIT_AMMO
	{
		get
		{
			return DataManager.Instance.PLAYER_SPIRIT_AMMO;
		}
		set
		{
			DataManager.Instance.PLAYER_SPIRIT_AMMO = value;
			if (DataManager.Instance.PLAYER_SPIRIT_AMMO <= 0)
			{
				DataManager.Instance.PLAYER_SPIRIT_AMMO = 0;
			}
			if (DataManager.Instance.PLAYER_SPIRIT_AMMO > DataManager.Instance.PLAYER_ARROW_TOTAL_AMMO)
			{
				DataManager.Instance.PLAYER_SPIRIT_AMMO = DataManager.Instance.PLAYER_ARROW_TOTAL_AMMO;
			}
			AmmoUpdated onAmmoUpdated = PlayerArrows.OnAmmoUpdated;
			if (onAmmoUpdated != null)
			{
				onAmmoUpdated(this);
			}
		}
	}

	public int PLAYER_SPIRIT_TOTAL_AMMO
	{
		get
		{
			return _totalSpiritAmmo;
		}
		set
		{
			int totalSpiritAmmo = _totalSpiritAmmo;
			DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO = value;
			_totalSpiritAmmo = value + TrinketManager.GetSpiritAmmo();
			if (_totalSpiritAmmo != totalSpiritAmmo)
			{
				if (_totalSpiritAmmo > totalSpiritAmmo)
				{
					PLAYER_SPIRIT_AMMO += _totalSpiritAmmo - totalSpiritAmmo;
				}
				else if (PLAYER_SPIRIT_AMMO > _totalSpiritAmmo)
				{
					PLAYER_SPIRIT_AMMO = _totalSpiritAmmo;
				}
				AmmoUpdated onAmmoUpdated = PlayerArrows.OnAmmoUpdated;
				if (onAmmoUpdated != null)
				{
					onAmmoUpdated(this);
				}
			}
		}
	}

	public static event AmmoUpdated OnAmmoUpdated;

	public static event AmmoUpdated OnNoAmmoShake;

	public static event AmmoUpdated OnBeginReloading;

	public void OnEnable()
	{
		TrinketManager.OnTrinketAdded += OnTrinketsChanged;
		TrinketManager.OnTrinketRemoved += OnTrinketsChanged;
	}

	public void OnDisable()
	{
		TrinketManager.OnTrinketAdded -= OnTrinketsChanged;
		TrinketManager.OnTrinketRemoved -= OnTrinketsChanged;
	}

	private void OnTrinketsChanged(TarotCards.Card trinket)
	{
		PLAYER_SPIRIT_TOTAL_AMMO = DataManager.Instance.PLAYER_SPIRIT_TOTAL_AMMO;
	}

	public void RestockArrow()
	{
		if (PLAYER_ARROW_AMMO <= 0)
		{
			growAndFade.Play();
		}
		PlayerFarming.Instance.simpleSpineAnimator.SetSkin("Lamb");
		if (PLAYER_ARROW_AMMO < PLAYER_ARROW_TOTAL_AMMO)
		{
			PLAYER_ARROW_AMMO++;
		}
		if (PLAYER_SPIRIT_AMMO < PLAYER_SPIRIT_TOTAL_AMMO)
		{
			PLAYER_SPIRIT_AMMO++;
		}
		AmmoUpdated onAmmoUpdated = PlayerArrows.OnAmmoUpdated;
		if (onAmmoUpdated != null)
		{
			onAmmoUpdated(this);
		}
		if (PLAYER_ARROW_AMMO >= PLAYER_ARROW_TOTAL_AMMO && PLAYER_SPIRIT_AMMO >= PLAYER_SPIRIT_TOTAL_AMMO)
		{
			Debug.Log("Ammo full");
			Reloading = false;
			return;
		}
		ReloadProgress = 0f;
		AmmoUpdated onBeginReloading = PlayerArrows.OnBeginReloading;
		if (onBeginReloading != null)
		{
			onBeginReloading(this);
		}
	}

	public void RestockAllArrows()
	{
		Reloading = false;
		if (PLAYER_ARROW_AMMO <= 0)
		{
			growAndFade.Play();
		}
		PlayerFarming.Instance.simpleSpineAnimator.SetSkin("Lamb");
		PLAYER_ARROW_AMMO = PLAYER_ARROW_TOTAL_AMMO;
		PLAYER_SPIRIT_AMMO = PLAYER_SPIRIT_TOTAL_AMMO;
		AmmoUpdated onAmmoUpdated = PlayerArrows.OnAmmoUpdated;
		if (onAmmoUpdated != null)
		{
			onAmmoUpdated(this);
		}
	}

	private void Update()
	{
		if (Reloading && (ReloadProgress += Time.deltaTime * ReloadSpeed) > ReloadTarget)
		{
			RestockArrow();
		}
	}
}
