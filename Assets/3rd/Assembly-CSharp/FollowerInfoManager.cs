using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class FollowerInfoManager : BaseMonoBehaviour
{
	public enum Outfit
	{
		Rags,
		Sherpa,
		Warrior,
		Follower
	}

	public static Action OnWorshipperDied;

	public FollowerInfo v_i;

	public FollowerOutfitType outfit;

	public bool Hooded;

	public TextMeshPro Name;

	public static List<FollowerInfoManager> followerInfoManagers = new List<FollowerInfoManager>();

	private Health health;

	public bool RandomOnStart;

	public bool InitOnStart = true;

	public SkeletonAnimation Spine;

	[SpineSkin("", "", true, false, false)]
	public string ForceSkinOverride;

	[SpineSkin("", "", true, false, false)]
	public string ForceOutfitSkinOverride;

	[SpineSkin("", "", true, false, false)]
	public string ForceExtraSkinOverride;

	public bool ForceSkin;

	public bool ForceOutfitSkin;

	public bool ForceExtraSkin;

	public bool IsHooded;

	private StateMachine state;

	public void OnEnable()
	{
		if (v_i == null && InitOnStart)
		{
			NewV_I();
		}
		followerInfoManagers.Add(this);
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
		followerInfoManagers.Remove(this);
		if (health != null)
		{
			health.OnDie -= OnDie;
			health.OnHit -= OnHit;
		}
	}

	public void SetV_I(FollowerInfo v_i)
	{
		this.v_i = v_i;
		Initialise();
	}

	public void NewV_I()
	{
		if (!ForceSkin)
		{
			v_i = FollowerInfo.NewCharacter(PlayerFarming.Location);
		}
		else
		{
			v_i = FollowerInfo.NewCharacter(PlayerFarming.Location, ForceSkinOverride);
		}
		v_i.Outfit = outfit;
		Initialise();
		SetOutfit();
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

	public void SetOutfit()
	{
		Skin skin = new Skin("New Skin");
		if (Spine == null)
		{
			Spine = base.gameObject.GetComponent<SkeletonAnimation>();
		}
		skin.AddSkin(Spine.Skeleton.Data.FindSkin(v_i.SkinName));
		switch (v_i.Outfit)
		{
		case FollowerOutfitType.Rags:
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Rags"));
			break;
		case FollowerOutfitType.Sherpa:
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Sherpa"));
			break;
		case FollowerOutfitType.Warrior:
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Warrior"));
			break;
		case FollowerOutfitType.Follower:
			if (v_i.DwellingSlot == 1)
			{
				skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Robes_Lvl" + v_i.XPLevel));
			}
			else
			{
				skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/NoHouse_Lvl" + v_i.XPLevel));
			}
			break;
		case FollowerOutfitType.Old:
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Other/Old"));
			break;
		case FollowerOutfitType.Custom:
			skin.AddSkin(Spine.Skeleton.Data.FindSkin(ForceOutfitSkinOverride));
			if (ForceExtraSkin)
			{
				skin.AddSkin(Spine.Skeleton.Data.FindSkin(ForceExtraSkinOverride));
			}
			break;
		}
		if (Hooded)
		{
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Clothes/Hooded_Lvl" + v_i.XPLevel));
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
			SetOutfit();
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
			component.totalHP = v_i.MaxHP;
		}
	}

	public static FollowerInfoManager GetWorshipperInfoManagerByDwellingID(int ID)
	{
		foreach (FollowerInfoManager followerInfoManager in followerInfoManagers)
		{
			if (followerInfoManager.v_i != null && followerInfoManager.v_i.DwellingID == ID)
			{
				return followerInfoManager;
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
