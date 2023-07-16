using System;
using UnityEngine;

public class Interaction_Temple : BaseMonoBehaviour
{
	public GameObject DoorLight;

	public EnterBuilding Entrance;

	public static Interaction_Temple Instance;

	public GameObject ExitPosition;

	public Animator shrineLevelUpAnimator;

	private Structure Structure;

	private void OnEnable()
	{
		Instance = this;
		Structure = GetComponent<Structure>();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		CheckDoorLight();
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(CheckDoorLight));
		UpgradeSystem.OnCoolDownAdded = (Action)Delegate.Combine(UpgradeSystem.OnCoolDownAdded, new Action(CheckXP));
		Inventory.OnInventoryUpdated = (Inventory.InventoryUpdated)Delegate.Combine(Inventory.OnInventoryUpdated, new Inventory.InventoryUpdated(CheckXP));
		CheckXP();
		shrineLevelUpAnimator.gameObject.SetActive(true);
	}

	private void OnDisable()
	{
		UpgradeSystem.OnCoolDownAdded = (Action)Delegate.Remove(UpgradeSystem.OnCoolDownAdded, new Action(CheckXP));
		Inventory.OnInventoryUpdated = (Inventory.InventoryUpdated)Delegate.Remove(Inventory.OnInventoryUpdated, new Inventory.InventoryUpdated(CheckXP));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(CheckDoorLight));
	}

	public void CheckXP()
	{
		if (UpgradeSystem.CanAffordDoctrine())
		{
			if (!shrineLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shown"))
			{
				shrineLevelUpAnimator.Play("Show");
			}
		}
		else if (shrineLevelUpAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shown"))
		{
			shrineLevelUpAnimator.Play("Hide");
		}
		else
		{
			shrineLevelUpAnimator.Play("Hidden");
		}
	}

	private void CheckDoorLight()
	{
		DoorLight.SetActive(DataManager.Instance.PreviousSermonDayIndex < TimeManager.CurrentDay);
	}

	private void Start()
	{
	}

	private void OnBrainAssigned()
	{
		if (PlayerFarming.Instance != null && Vector3.Distance(base.transform.position + new Vector3(0f, 2f), PlayerFarming.Instance.transform.position) <= 3f)
		{
			BoxCollider2D[] Colliders = GetComponentsInChildren<BoxCollider2D>();
			Debug.Log("DISTANCE: " + Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position));
			PlayerFarming.Instance.GoToAndStop(base.transform.position, base.gameObject, true, true, delegate
			{
				Debug.Log("COMPLETED!");
				Bounds bounds = default(Bounds);
				BoxCollider2D[] array = Colliders;
				foreach (BoxCollider2D boxCollider2D in array)
				{
					boxCollider2D.enabled = true;
					bounds.Encapsulate(boxCollider2D.bounds);
				}
				AstarPath.active.UpdateGraphs(bounds);
			});
			Entrance.Trigger.AddListener(BiomeBaseManager.Instance.ActivateChurch);
		}
		else if (Entrance != null && Entrance.Trigger != null && BiomeBaseManager.Instance != null)
		{
			Entrance.Trigger.AddListener(BiomeBaseManager.Instance.ActivateChurch);
		}
		if (Structure.Type == StructureBrain.TYPES.TEMPLE)
		{
			DataManager.Instance.HasBuiltTemple1 = true;
		}
		if (Structure.Type == StructureBrain.TYPES.TEMPLE_II)
		{
			DataManager.Instance.HasBuiltTemple2 = true;
		}
		if (Structure.Type == StructureBrain.TYPES.TEMPLE_III)
		{
			DataManager.Instance.HasBuiltTemple3 = true;
		}
		if (Structure.Type == StructureBrain.TYPES.TEMPLE_IV)
		{
			DataManager.Instance.HasBuiltTemple4 = true;
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + new Vector3(0f, 2f), 3f, Color.green);
	}

	private void OnDestroy()
	{
		Entrance.Trigger.RemoveAllListeners();
	}
}
