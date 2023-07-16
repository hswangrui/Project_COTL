using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class DeadWorshipper : BaseMonoBehaviour
{
	public static List<DeadWorshipper> DeadWorshippers = new List<DeadWorshipper>();

	public SkeletonAnimation Spine;

	public Structure Structure;

	public ParticleSystem RottenParticles;

	public FollowerInfo followerInfo;

	public GameObject[] Flowers;

	public GameObject ItemIndicator;

	public bool PlayAnimation;

	public string DeadAnimation = "dead";

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	private void Data()
	{
		Debug.Log("PlayAnimation " + PlayAnimation);
	}

	private void Start()
	{
		DeadWorshippers.Add(this);
		StructureManager.OnStructureMoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureMoved, new StructureManager.StructureChanged(StructureModified));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(StructureModified));
	}

	private void OnEnable()
	{
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		HideBody();
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnDestroy()
	{
		DeadWorshippers.Remove(this);
		StructureManager.OnStructureMoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureMoved, new StructureManager.StructureChanged(StructureModified));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(StructureModified));
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Spine.Initialize(false);
		Setup();
	}

	private void StructureModified(StructuresData s)
	{
		if (s == null || StructureInfo == null || s.FollowerID != StructureInfo.FollowerID)
		{
			return;
		}
		if (s.ID == StructureInfo.ID)
		{
			foreach (Structures_Prison item in StructureManager.GetAllStructuresOfType<Structures_Prison>())
			{
				if (item.Data.FollowerID == StructureInfo.FollowerID)
				{
					item.Data.FollowerID = -1;
				}
			}
			return;
		}
		DeadAnimation = "dead";
		StructureInfo.Animation = DeadAnimation;
		s.FollowerID = -1;
		Setup();
	}

	public void Setup()
	{
		Debug.Log("Set up ID: " + StructureInfo.FollowerID);
		followerInfo = FollowerManager.GetDeadFollowerInfoByID(StructureInfo.FollowerID);
		Debug.Log("Dead follower: " + followerInfo);
		if (followerInfo == null)
		{
			followerInfo = FollowerManager.FindFollowerInfo(StructureInfo.FollowerID);
			Debug.Log("Living follower: " + followerInfo);
		}
		if (StructureInfo.FollowerID == -1 || followerInfo == null)
		{
			base.gameObject.SetActive(false);
			StructureManager.OnStructureMoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureMoved, new StructureManager.StructureChanged(StructureModified));
			StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(StructureModified));
			StructureManager.RemoveStructure(Structure.Brain);
			return;
		}
		SetOutfit();
		Spine.skeleton.ScaleX = StructureInfo.Dir;
		base.gameObject.name = "dead body " + followerInfo.Name;
		if (StructureInfo.Animation != "" && StructureInfo.Animation != "dead")
		{
			DeadAnimation = StructureInfo.Animation;
		}
		StructureInfo.Animation = DeadAnimation;
		if (PlayAnimation)
		{
			Spine.AnimationState.SetAnimation(0, "die", false);
			Spine.AnimationState.End += AnimationState_End;
			Spine.AnimationState.AddAnimation(0, DeadAnimation, true, 0f);
		}
		else if (StructureInfo.BodyWrapped)
		{
			Spine.AnimationState.SetAnimation(0, "corpse", true);
			WrapBody();
		}
		else if (StructureInfo.Rotten)
		{
			Debug.Log("Set Body Rotten");
			Spine.AnimationState.SetAnimation(0, DeadAnimation + "-rotten", true);
			RottenParticles.gameObject.SetActive(true);
		}
		else
		{
			Debug.Log("Set Body normal dead");
			Spine.AnimationState.SetAnimation(0, DeadAnimation, true);
			RottenParticles.gameObject.SetActive(false);
		}
		ShowBody();
	}

	public void SetOutfit()
	{
		if (followerInfo == null || Spine == null || Spine.Skeleton == null)
		{
			return;
		}
		FollowerOutfit followerOutfit = new FollowerOutfit(followerInfo);
		Skin skin = new Skin("New Skin");
		Skin skin2 = Spine.Skeleton.Data.FindSkin(followerInfo.SkinName);
		if (skin2 != null)
		{
			skin.AddSkin(skin2);
		}
		else
		{
			skin.AddSkin(Spine.Skeleton.Data.FindSkin("Cat"));
			followerInfo.SkinName = "Cat";
		}
		string outfitSkinName = followerOutfit.GetOutfitSkinName(followerInfo.Outfit);
		if (!string.IsNullOrEmpty(outfitSkinName))
		{
			skin.AddSkin(Spine.skeleton.Data.FindSkin(outfitSkinName));
		}
		if (followerInfo.Necklace != 0)
		{
			skin.AddSkin(Spine.skeleton.Data.FindSkin("Necklaces/" + followerInfo.Necklace));
		}
		Spine.Skeleton.SetSkin(skin);
		Spine.skeleton.SetSlotsToSetupPose();
		WorshipperData.SkinAndData colourData = WorshipperData.Instance.GetColourData(followerInfo.SkinName);
		if (colourData == null)
		{
			return;
		}
		foreach (WorshipperData.SlotAndColor slotAndColour in colourData.SlotAndColours[Mathf.Clamp(followerInfo.SkinColour, 0, colourData.SlotAndColours.Count - 1)].SlotAndColours)
		{
			Slot slot = Spine.skeleton.FindSlot(slotAndColour.Slot);
			if (slot != null)
			{
				slot.SetColor(slotAndColour.color);
			}
		}
	}

	public void SetOutfit(FollowerOutfitType outfit)
	{
		FollowerOutfit followerOutfit = new FollowerOutfit(followerInfo);
		if (followerInfo.CursedState == Thought.OldAge)
		{
			followerOutfit.SetOutfit(Spine, FollowerOutfitType.Old, InventoryItem.ITEM_TYPE.NONE, false);
		}
		else
		{
			followerOutfit.SetOutfit(Spine, outfit, InventoryItem.ITEM_TYPE.NONE, false);
		}
	}

	private void OnNewDayStarted()
	{
		Spine.enabled = false;
		StartCoroutine(WaitRotten());
	}

	private IEnumerator WaitRotten()
	{
		yield return new WaitForSeconds(0.1f);
		if (StructureInfo.Rotten && !StructureInfo.BodyWrapped)
		{
			Spine.enabled = true;
			yield return new WaitForEndOfFrame();
			SetRotten();
			yield return new WaitForEndOfFrame();
			Spine.enabled = false;
		}
	}

	private IEnumerator DeactivateSpine()
	{
		yield return new WaitForSeconds(2.5f);
		if (Spine != null)
		{
			Spine.enabled = false;
		}
	}

	private void SetRotten()
	{
		if (!StructureInfo.BodyWrapped)
		{
			Debug.Log("Set Body Rotten");
			Spine.AnimationState.SetAnimation(0, DeadAnimation + "-rotten", true);
			RottenParticles.gameObject.SetActive(true);
		}
	}

	public void WrapBody()
	{
		Spine.enabled = true;
		StructureInfo.BodyWrapped = true;
		Spine.AnimationState.SetAnimation(0, "corpse", true);
		RottenParticles.gameObject.SetActive(false);
		GameManager.GetInstance().StartCoroutine(DeactivateSpine());
	}

	public void HideBody()
	{
		Spine.gameObject.SetActive(false);
		RottenParticles.gameObject.SetActive(false);
		ItemIndicator.SetActive(false);
	}

	public void ShowBody()
	{
		ItemIndicator.SetActive(true);
		Spine.gameObject.SetActive(true);
	}

	private void AnimationState_End(TrackEntry trackEntry)
	{
		PlayAnimation = false;
		foreach (Follower follower in Follower.Followers)
		{
			if (Vector3.Distance(base.transform.position, base.transform.position) < 12f && follower.Brain.Location == StructureInfo.Location && (follower.Brain.CurrentTask == null || !follower.Brain.CurrentTask.BlockReactTasks))
			{
				follower.Brain.HardSwapToTask(new FollowerTask_ReactCorpse(StructureInfo.ID));
			}
		}
	}

	public void AddWorshipper()
	{
		DeadWorshippers.Add(this);
	}

	public void RemoveWorshipper()
	{
		DeadWorshippers.Remove(this);
	}

	private void OnDisable()
	{
		Spine.AnimationState.End -= AnimationState_End;
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	public void BounceOutFromPosition(float speed)
	{
		PickUp component = GetComponent<PickUp>();
		component.enabled = true;
		component.Speed = speed;
	}

	public void BounceOutFromPosition(float speed, Vector3 dir)
	{
		PickUp component = GetComponent<PickUp>();
		component.enabled = true;
		component.SetInitialSpeedAndDiraction(speed, Utils.GetAngle(Vector3.zero, dir));
	}
}
