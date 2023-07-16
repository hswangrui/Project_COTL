using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class WorshipperInfoManager : BaseMonoBehaviour
{
	public enum Outfit
	{
		Rags,
		Sherpa,
		Warrior,
		Follower
	}

	public static Action OnWorshipperDied;

	public Villager_Info v_i;

	public TextMeshPro Name;

	public static List<WorshipperInfoManager> worshipperInfoManagers = new List<WorshipperInfoManager>();

	private Health health;

	public bool RandomOnStart;

	public SkeletonAnimation Spine;

	public bool IsHooded;

	private StateMachine state;

	public void OnEnable()
	{
		worshipperInfoManagers.Add(this);
		health = GetComponent<Health>();
		if (health != null)
		{
			health.OnDie += OnDie;
			health.OnHit += OnHit;
		}
		if (RandomOnStart)
		{
			NewV_I();
		}
	}

	public void OnDisable()
	{
		worshipperInfoManagers.Remove(this);
		if (health != null)
		{
			health.OnDie -= OnDie;
			health.OnHit -= OnHit;
		}
	}

	public void SetV_I(Villager_Info v_i)
	{
		this.v_i = v_i;
		Initialise();
	}

	public void NewV_I()
	{
		v_i = Villager_Info.NewCharacter();
		Initialise();
	}

	private void Initialise()
	{
		SetColor();
		AddName();
		SetHealth();
	}

	public IDAndRelationship GetRelationship(int ID)
	{
		foreach (IDAndRelationship relationship in v_i.Relationships)
		{
			if (relationship.ID == ID)
			{
				return relationship;
			}
		}
		IDAndRelationship iDAndRelationship = new IDAndRelationship();
		iDAndRelationship.ID = ID;
		iDAndRelationship.Relationship = 0;
		v_i.Relationships.Add(iDAndRelationship);
		return iDAndRelationship;
	}

	public void SetOutfit(Outfit outfit, bool Hooded)
	{
		Skin skin = new Skin("New Skin");
		skin.AddSkin(Spine.Skeleton.Data.FindSkin(v_i.SkinName));
		switch (outfit)
		{
		case Outfit.Rags:
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Rags"));
			break;
		case Outfit.Sherpa:
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Sherpa"));
			break;
		case Outfit.Warrior:
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Warrior"));
			break;
		case Outfit.Follower:
			if (v_i.DwellingClaimed)
			{
				skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Robes_Lvl" + v_i.Level));
			}
			else
			{
				skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/NoHouse_Lvl" + v_i.Level));
			}
			break;
		}
		if (Hooded)
		{
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Hooded_Lvl" + v_i.Level));
			IsHooded = true;
		}
		else
		{
			IsHooded = false;
		}
		Spine.Skeleton.SetSkin(skin);
		Spine.skeleton.SetSlotsToSetupPose();
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(v_i.SkinName);
		if (colourData == null)
		{
			return;
		}
		foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(v_i.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
		{
			SetSlotColour(slotAndColour.Slot, slotAndColour.color);
		}
	}

	private void SetColor()
	{
		if (Spine == null)
		{
			Spine = GetComponentInChildren<SkeletonAnimation>();
		}
		state = GetComponent<StateMachine>();
		if (Spine != null)
		{
			SetOutfit(v_i.Outfit, false);
		}
	}

	public void SetSlotColour(string SlotName, Color color)
	{
		Slot slot = Spine.skeleton.FindSlot(SlotName);
		if (slot != null)
		{
			slot.SetColor(color);
		}
	}

	private void AddName()
	{
		if (Name != null)
		{
			Name.text = v_i.Name;
		}
	}

	private void SetHealth()
	{
		Health component = GetComponent<Health>();
		if (component != null)
		{
			component.HP = v_i.HP;
			component.totalHP = v_i.TotalHP;
		}
	}

	public static WorshipperInfoManager GetWorshipperInfoManagerByJobID(string ID)
	{
		foreach (WorshipperInfoManager worshipperInfoManager in worshipperInfoManagers)
		{
			if (worshipperInfoManager.v_i != null && worshipperInfoManager.v_i.WorkPlace == ID)
			{
				return worshipperInfoManager;
			}
		}
		return null;
	}

	public static WorshipperInfoManager GetWorshipperInfoManagerByDwellingID(string ID)
	{
		foreach (WorshipperInfoManager worshipperInfoManager in worshipperInfoManagers)
		{
			if (worshipperInfoManager.v_i != null && worshipperInfoManager.v_i.Dwelling == ID)
			{
				return worshipperInfoManager;
			}
		}
		return null;
	}

	public virtual void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		OnDie();
	}

	public void OnDie()
	{
		Action onWorshipperDied = OnWorshipperDied;
		if (onWorshipperDied != null)
		{
			onWorshipperDied();
		}
	}

	public virtual void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		v_i.HP = health.HP;
	}
}
