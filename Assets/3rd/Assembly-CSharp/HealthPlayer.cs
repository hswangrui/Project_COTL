using UnityEngine;

public class HealthPlayer : Health
{
	public delegate void HPUpdated(HealthPlayer Target);

	public delegate void TotalHPUpdated(HealthPlayer Target);

	public static bool ResetHealthData = true;

	public float PoisonNormalisedTime
	{
		get
		{
			return base.poisonTimer / poisonTickDuration;
		}
	}

	public new bool IsPoisoned
	{
		get
		{
			return base.poisonTimer > 0f;
		}
	}

	public override float poisonTickDuration
	{
		get
		{
			return 0.75f;
		}
	}

	protected override float playerPoisonDamage
	{
		get
		{
			return 1f;
		}
	}

	public override float HP
	{
		get
		{
			return DataManager.Instance.PLAYER_HEALTH;
		}
		set
		{
			DataManager.Instance.PLAYER_HEALTH = value;
			if (DataManager.Instance.PLAYER_HEALTH > DataManager.Instance.PLAYER_TOTAL_HEALTH)
			{
				DataManager.Instance.PLAYER_HEALTH = DataManager.Instance.PLAYER_TOTAL_HEALTH;
			}
			if (HealthPlayer.OnHPUpdated != null)
			{
				HealthPlayer.OnHPUpdated(this);
			}
		}
	}

	public override float totalHP
	{
		get
		{
			return DataManager.Instance.PLAYER_TOTAL_HEALTH;
		}
		set
		{
			DataManager.Instance.PLAYER_TOTAL_HEALTH = value;
			if (HealthPlayer.OnTotalHPUpdated != null)
			{
				HealthPlayer.OnTotalHPUpdated(this);
			}
		}
	}

	public override float BlueHearts
	{
		get
		{
			return DataManager.Instance.PLAYER_BLUE_HEARTS;
		}
		set
		{
			if (value > DataManager.Instance.PLAYER_BLUE_HEARTS)
			{
				DataManager.Instance.PLAYER_BLUE_HEARTS = value;
				if (HealthPlayer.OnTotalHPUpdated != null)
				{
					HealthPlayer.OnTotalHPUpdated(this);
				}
			}
			else
			{
				DataManager.Instance.PLAYER_BLUE_HEARTS = value;
				if (HealthPlayer.OnHPUpdated != null)
				{
					HealthPlayer.OnHPUpdated(this);
				}
			}
		}
	}

	public override float BlackHearts
	{
		get
		{
			return DataManager.Instance.PLAYER_BLACK_HEARTS;
		}
		set
		{
			if (value > DataManager.Instance.PLAYER_BLACK_HEARTS)
			{
				DataManager.Instance.PLAYER_BLACK_HEARTS = value;
				if (HealthPlayer.OnTotalHPUpdated != null)
				{
					HealthPlayer.OnTotalHPUpdated(this);
				}
			}
			else
			{
				DataManager.Instance.PLAYER_BLACK_HEARTS = value;
				if (HealthPlayer.OnHPUpdated != null)
				{
					HealthPlayer.OnHPUpdated(this);
				}
			}
		}
	}

	public override float TotalSpiritHearts
	{
		get
		{
			return _TotalSpiritHearts;
		}
		set
		{
			float pLAYER_SPIRIT_TOTAL_HEARTS = DataManager.Instance.PLAYER_SPIRIT_TOTAL_HEARTS;
			DataManager.Instance.PLAYER_SPIRIT_TOTAL_HEARTS = value;
			_TotalSpiritHearts = value;
			HPUpdated onTotalHPUpdated = HealthPlayer.OnTotalHPUpdated;
			if (onTotalHPUpdated != null)
			{
				onTotalHPUpdated(this);
			}
			if (TotalSpiritHearts != pLAYER_SPIRIT_TOTAL_HEARTS)
			{
				if (TotalSpiritHearts > pLAYER_SPIRIT_TOTAL_HEARTS)
				{
					SpiritHearts += TotalSpiritHearts - pLAYER_SPIRIT_TOTAL_HEARTS;
				}
				else if (SpiritHearts > TotalSpiritHearts)
				{
					SpiritHearts = TotalSpiritHearts;
				}
			}
		}
	}

	public override float SpiritHearts
	{
		get
		{
			return DataManager.Instance.PLAYER_SPIRIT_HEARTS;
		}
		set
		{
			float num = Mathf.Clamp(value, 0f, TotalSpiritHearts);
			if (num != DataManager.Instance.PLAYER_SPIRIT_HEARTS)
			{
				DataManager.Instance.PLAYER_SPIRIT_HEARTS = num;
				HPUpdated onHPUpdated = HealthPlayer.OnHPUpdated;
				if (onHPUpdated != null)
				{
					onHPUpdated(this);
				}
			}
		}
	}

	public static event HPUpdated OnHPUpdated;

	public static event HPUpdated OnHeal;

	public new static event HPUpdated OnDamaged;

	public static event HPUpdated OnPlayerDied;

	public static event HPUpdated OnTotalHPUpdated;

	public override void OnEnable()
	{
		base.OnEnable();
		TrinketManager.OnTrinketAdded += OnTrinketsChanged;
		TrinketManager.OnTrinketRemoved += OnTrinketsChanged;
		base.OnDie += HealthPlayer_OnDie;
	}

	public override void InitHP()
	{
		if (ResetHealthData || PlayerFarming.Location == FollowerLocation.Base)
		{
			ResetHealthData = false;
			DataManager.Instance.PLAYER_TOTAL_HEALTH = (float)(DataManager.Instance.PLAYER_STARTING_HEALTH_CACHED + DataManager.Instance.PLAYER_HEALTH_MODIFIED + DataManager.Instance.PLAYER_HEARTS_LEVEL - DataManager.Instance.RedHeartsTemporarilyRemoved) * PlayerFleeceManager.GetHealthMultiplier();
			DataManager.Instance.PLAYER_STARTING_HEALTH_CACHED = DataManager.Instance.PLAYER_STARTING_HEALTH;
			DataManager.Instance.PLAYER_HEALTH = DataManager.Instance.PLAYER_TOTAL_HEALTH;
		}
		if (DataManager.Instance.PlayerFleece == 7)
		{
			totalHP = PlayerFleeceManager.OneHitKillHP;
			HP = PlayerFleeceManager.OneHitKillHP;
		}
		else
		{
			if (PlayerFarming.Location == FollowerLocation.Base)
			{
				HP = DataManager.Instance.PLAYER_HEALTH;
			}
			totalHP = DataManager.Instance.PLAYER_TOTAL_HEALTH;
		}
		BlueHearts = DataManager.Instance.PLAYER_BLUE_HEARTS;
		TotalSpiritHearts = DataManager.Instance.PLAYER_SPIRIT_TOTAL_HEARTS;
		SpiritHearts = DataManager.Instance.PLAYER_SPIRIT_HEARTS;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		TrinketManager.OnTrinketAdded -= OnTrinketsChanged;
		TrinketManager.OnTrinketRemoved -= OnTrinketsChanged;
		base.OnDie -= HealthPlayer_OnDie;
	}

	private void HealthPlayer_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, AttackTypes AttackType, AttackFlags AttackFlags)
	{
		HPUpdated onPlayerDied = HealthPlayer.OnPlayerDied;
		if (onPlayerDied != null)
		{
			onPlayerDied(this);
		}
	}

	private void OnTrinketsChanged(TarotCards.Card trinket)
	{
		TotalSpiritHearts = DataManager.Instance.PLAYER_SPIRIT_TOTAL_HEARTS;
	}

	public override bool DealDamage(float Damage, GameObject Attacker, Vector3 AttackLocation, bool BreakBlocking = false, AttackTypes AttackType = AttackTypes.Melee, bool dealDamageImmediately = false, AttackFlags AttackFlags = (AttackFlags)0)
	{
		bool num = base.DealDamage(Damage, Attacker, AttackLocation, BreakBlocking, AttackType, dealDamageImmediately, AttackFlags);
		if (num)
		{
			HPUpdated onDamaged = HealthPlayer.OnDamaged;
			if (onDamaged == null)
			{
				return num;
			}
			onDamaged(this);
		}
		return num;
	}

	public override void Heal(float healing)
	{
		base.Heal(healing);
		HPUpdated onHeal = HealthPlayer.OnHeal;
		if (onHeal != null)
		{
			onHeal(this);
		}
	}

	public static int GainRandomHeart()
	{
		Health health = PlayerFarming.Instance.health;
		int num = Random.Range(0, 3);
		switch (num)
		{
		case 0:
			health.BlackHearts += 2f;
			break;
		case 1:
			health.BlueHearts += 2f;
			break;
		case 2:
			health.TotalSpiritHearts += 2f;
			break;
		}
		return num;
	}

	public static void LoseAllSpecialHearts()
	{
		Health health = PlayerFarming.Instance.health;
		health.BlueHearts = 0f;
		health.BlackHearts = 0f;
		health.TotalSpiritHearts = 0f;
	}
}
