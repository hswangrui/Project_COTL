using UnityEngine;

[CreateAssetMenu(menuName = "COTL/Weapon Attachment Data")]
public class WeaponAttachmentData : ScriptableObject
{
	public AttachmentEffect Effect;

	public AttachmentState State;

	public string DescriptionKey;

	public float ExplosionRadius;

	public float ExplosionDamage;

	public float ExplosionOffset;

	public float DashSpeed;

	public float DamageMultiplierIncrement;

	public int ExtraSlotsAmount;

	public float CriticalMultiplierIncrement;

	public float RangeIncrement;

	public float AttackRateIncrement;

	public float MovementSpeedIncrement;

	public float xpDropIncrement;

	[Range(0f, 1f)]
	public float healChanceIncrement;

	public float healAmount;

	[Range(0f, 100f)]
	public float negateDamageChanceIncrement;

	[Range(0f, 1f)]
	public float poisonChance = 0.3f;

	[Range(0f, 1f)]
	public float necromancyChance = 0.3f;

	private bool hasExplodeEffect
	{
		get
		{
			return Effect == AttachmentEffect.Explode;
		}
	}

	private bool hasDashEffect
	{
		get
		{
			return Effect == AttachmentEffect.Dash;
		}
	}

	private bool hasDamageEffect
	{
		get
		{
			return Effect == AttachmentEffect.Damage;
		}
	}

	private bool hasExtraSlotsEffect
	{
		get
		{
			return Effect == AttachmentEffect.ExtraSlots;
		}
	}

	private bool hasCriticalEffect
	{
		get
		{
			return Effect == AttachmentEffect.Critical;
		}
	}

	private bool hasRangeEffect
	{
		get
		{
			return Effect == AttachmentEffect.Range;
		}
	}

	private bool hasAttackRateEffect
	{
		get
		{
			return Effect == AttachmentEffect.AttackRate;
		}
	}

	private bool hasMovementSpeedEffect
	{
		get
		{
			return Effect == AttachmentEffect.MovementSpeed;
		}
	}

	private bool hasIncreaseXPEffect
	{
		get
		{
			return Effect == AttachmentEffect.IncreasedXPDrop;
		}
	}

	private bool hasHealChanceEffect
	{
		get
		{
			return Effect == AttachmentEffect.HealChance;
		}
	}

	private bool hasNegateDamageChanceEffect
	{
		get
		{
			return Effect == AttachmentEffect.NegateDamageChance;
		}
	}

	private bool hasPoisonEffect
	{
		get
		{
			return Effect == AttachmentEffect.Poison;
		}
	}

	private bool hasNecromanyEffect
	{
		get
		{
			return Effect == AttachmentEffect.Necromancy;
		}
	}

	public bool IsAttachmentActive()
	{
		return true;
	}
}
