using System;
using System.Collections.Generic;
using UnityEngine;

public class Dwelling : BaseMonoBehaviour
{
	public class DwellingAndSlot
	{
		public int ID;

		public int dwellingslot;

		public int dwellingLevel;

		public DwellingAndSlot(int ID, int dwellingslot, int dwellingLevel)
		{
			this.ID = ID;
			this.dwellingslot = dwellingslot;
			this.dwellingLevel = dwellingLevel;
		}
	}

	public enum SlotState
	{
		UNCLAIMED,
		CLAIMED,
		IN_USE
	}

	public static int NO_HOME = 0;

	public List<DwellingSlot> Positions = new List<DwellingSlot>();

	public StructureBrain.TYPES Type;

	private bool init;

	public static List<Dwelling> dwellings = new List<Dwelling>();

	public Structure Structure;

	private Structures_Bed _StructureInfo;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Bed StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Bed;
			}
			return _StructureInfo;
		}
	}

	private void Start()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnEnable()
	{
		dwellings.Add(this);
		InitImages();
		FollowerBrain.OnDwellingAssigned = (FollowerBrain.DwellingAssignmentChanged)Delegate.Combine(FollowerBrain.OnDwellingAssigned, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
		FollowerBrain.OnDwellingCleared = (FollowerBrain.DwellingAssignmentChanged)Delegate.Combine(FollowerBrain.OnDwellingCleared, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
	}

	private void OnDisable()
	{
		dwellings.Remove(this);
		FollowerBrain.OnDwellingAssigned = (FollowerBrain.DwellingAssignmentChanged)Delegate.Remove(FollowerBrain.OnDwellingAssigned, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
		FollowerBrain.OnDwellingCleared = (FollowerBrain.DwellingAssignmentChanged)Delegate.Remove(FollowerBrain.OnDwellingCleared, new FollowerBrain.DwellingAssignmentChanged(OnDwellingAssignmentChanged));
	}

	private void OnDestroy()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		if (Structure.Brain.Data.FollowersClaimedSlots.Count > 0)
		{
			bool flag = false;
			foreach (FollowerInfo follower in DataManager.Instance.Followers)
			{
				if (follower.DwellingID == Structure.Structure_Info.ID || Structure.Structure_Info.MultipleFollowerIDs.Contains(follower.DwellingID))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Structure.Brain.Data.MultipleFollowerIDs.Remove(Structure.Brain.Data.FollowerID);
				Structure.Brain.Data.FollowerID = -1;
				Structure.Brain.Data.FollowersClaimedSlots.Clear();
			}
		}
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureBrain.CheckForAndClearDuplicateBeds();
		InitImages();
	}

	public Vector3 GetDwellingSlotPosition(int slotID)
	{
		if (slotID < Positions.Count)
		{
			return Positions[slotID].BedOccupied.transform.position;
		}
		return Positions[0].BedOccupied.transform.position;
	}

	private void OnDwellingAssignmentChanged(int followerID, DwellingAndSlot d)
	{
		if (Structure != null && Structure.Structure_Info != null && d.ID == Structure.Structure_Info.ID)
		{
			InitImages();
		}
	}

	private void InitImages()
	{
		for (int i = 0; i < Positions.Count; i++)
		{
			SetBedImage(i, SlotState.UNCLAIMED);
		}
		if (Structure == null || Structure.Structure_Info == null)
		{
			return;
		}
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			if (follower.DwellingID == Structure.Structure_Info.ID && StructureBrain.Data.FollowersClaimedSlots.Contains(follower.ID))
			{
				SetBedImage(follower.DwellingSlot, SlotState.CLAIMED);
			}
		}
	}

	public void SetBedImage(int slot, SlotState slotState)
	{
		switch (slotState)
		{
		case SlotState.UNCLAIMED:
			Positions[slot].BedOccupied.SetActive(false);
			Positions[slot].BedUnoccupied.SetActive(false);
			Positions[slot].BedUnclaimed.SetActive(true);
			break;
		case SlotState.CLAIMED:
			Positions[slot].BedOccupied.SetActive(false);
			Positions[slot].BedUnoccupied.SetActive(true);
			Positions[slot].BedUnclaimed.SetActive(false);
			break;
		case SlotState.IN_USE:
			Positions[slot].BedOccupied.SetActive(true);
			Positions[slot].BedUnoccupied.SetActive(false);
			Positions[slot].BedUnclaimed.SetActive(false);
			break;
		}
	}

	public static Dwelling GetDwellingByID(int ID)
	{
		foreach (Dwelling dwelling in dwellings)
		{
			if (dwelling.Structure.Structure_Info.ID == ID)
			{
				return dwelling;
			}
		}
		return null;
	}
}
