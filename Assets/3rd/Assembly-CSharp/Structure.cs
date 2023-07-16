using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Structure : BaseMonoBehaviour
{
	public delegate void StructureInventoryChanged(Structure structure, InventoryItem item);

	public StructureBrain.TYPES Type;

	public int VariantIndex;

	private StructureBrain _brain;

	public Action OnBrainAssigned;

	public Action OnBrainRemoved;

	public static List<Structure> Structures = new List<Structure>();

	public static StructureInventoryChanged OnItemDeposited;

	public UnityEvent OnProgressCompleted;

	private Health health;

	public StructuresData Structure_Info
	{
		get
		{
			StructureBrain brain = Brain;
			if (brain == null)
			{
				return null;
			}
			return brain.Data;
		}
	}

	public StructureBrain Brain
	{
		get
		{
			return _brain;
		}
		set
		{
			if (_brain != null)
			{
				base.transform.position -= Structure_Info.Offset;
			}
			_brain = value;
			Action onBrainAssigned = OnBrainAssigned;
			if (onBrainAssigned != null)
			{
				onBrainAssigned();
			}
			base.transform.position += Structure_Info.Offset;
		}
	}

	public List<InventoryItem> Inventory
	{
		get
		{
			return Structure_Info.Inventory;
		}
		set
		{
			Structure_Info.Inventory = value;
		}
	}

	public static int CountStructuresOfType(StructureBrain.TYPES structureType)
	{
		int num = 0;
		foreach (Structure structure in Structures)
		{
			if (structure.Type == structureType)
			{
				num++;
			}
		}
		return num;
	}

	private void OnEnable()
	{
		if (Structure_Info != null && Structure_Info.Destroyed)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
		Structures.Add(this);
	}

	private void OnDisable()
	{
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
		Structures.Remove(this);
	}

	private void Start()
	{
		health = GetComponent<Health>();
		if (health != null)
		{
			health.OnHit += OnHit;
			health.OnDie += OnDie;
		}
	}

	private void OnStructureRemoved(StructuresData structure)
	{
		if (structure == Structure_Info)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void CreateStructure(FollowerLocation location, Vector3 position, bool emitParticles = true, bool save = true)
	{
		CreateStructure(location, position, Vector2Int.one, emitParticles, save);
	}

	public void CreateStructure(FollowerLocation location, Vector3 position, Vector2Int bounds, bool emitParticles = true, bool save = true)
	{
		StructuresData infoByType = StructuresData.GetInfoByType(Type, VariantIndex);
		if (infoByType != null)
		{
			infoByType.CreateStructure(location, position, bounds);
			Brain = StructureManager.AddStructure(location, infoByType, emitParticles, save);
		}
	}

	public void CreateStructure(FollowerLocation location, Vector3 position, Vector2Int bounds, StructureBrain.TYPES ToBuildType)
	{
		StructuresData infoByType = StructuresData.GetInfoByType(Type, VariantIndex);
		infoByType.ToBuildType = ToBuildType;
		if (infoByType != null)
		{
			infoByType.CreateStructure(location, position, bounds);
			Brain = StructureManager.AddStructure(location, infoByType);
		}
	}

	public void RemoveStructure()
	{
		if (Structure_Info.ToBuildType == StructureBrain.TYPES.SURVEILLANCE)
		{
			DataManager.Instance.HasBuiltSurveillance = false;
		}
		if (Type == StructureBrain.TYPES.SURVEILLANCE)
		{
			DataManager.Instance.HasBuiltSurveillance = false;
		}
		Brain.Remove();
		Action onBrainRemoved = OnBrainRemoved;
		if (onBrainRemoved != null)
		{
			onBrainRemoved();
		}
	}

	public static bool TypeExists(StructureBrain.TYPES Type)
	{
		foreach (Structure structure in Structures)
		{
			if (structure.Type == Type)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsType(StructureBrain.TYPES Type)
	{
		return this.Type == Type;
	}

	public static Structure GetOfType(StructureBrain.TYPES Type)
	{
		foreach (Structure structure in Structures)
		{
			if (structure.Type == Type)
			{
				return structure;
			}
		}
		return null;
	}

	public static List<Structure> GetListOfType(StructureBrain.TYPES Type)
	{
		List<Structure> list = new List<Structure>();
		foreach (Structure structure in Structures)
		{
			if (structure.Type == Type)
			{
				list.Add(structure);
			}
		}
		return list;
	}

	public static int GetTypeCount(StructureBrain.TYPES Type)
	{
		int num = 0;
		foreach (Structure structure in Structures)
		{
			if (structure.Type == Type)
			{
				num++;
			}
		}
		return num;
	}

	public void DepositInventory(InventoryItem item)
	{
		Inventory.Add(item);
		StructureInventoryChanged onItemDeposited = OnItemDeposited;
		if (onItemDeposited != null)
		{
			onItemDeposited(this, item);
		}
	}

	public bool HasInventoryType(InventoryItem.ITEM_TYPE Type)
	{
		foreach (InventoryItem item in Inventory)
		{
			if (item.type == (int)Type)
			{
				return true;
			}
		}
		return false;
	}

	public int GetInventoryTypeCount(InventoryItem.ITEM_TYPE Type)
	{
		int num = 0;
		foreach (InventoryItem item in Inventory)
		{
			if (item.type == (int)Type)
			{
				num++;
			}
		}
		return num;
	}

	public void RemoveInventoryByType(InventoryItem.ITEM_TYPE Type)
	{
		foreach (InventoryItem item in Inventory)
		{
			if (item.type == (int)Type)
			{
				Inventory.Remove(item);
				break;
			}
		}
	}

	public void ProgressCompleted()
	{
		if (OnProgressCompleted != null)
		{
			OnProgressCompleted.Invoke();
		}
	}

	public virtual void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		CameraManager.shakeCamera(0.5f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		if (Structure_Info.RemoveOnDie)
		{
			RemoveStructure();
		}
	}

	public virtual void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		CameraManager.shakeCamera(0.1f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
	}

	public virtual void OnDestroy()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
			health.OnHit -= OnHit;
		}
	}
}
