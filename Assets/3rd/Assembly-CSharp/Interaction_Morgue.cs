using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using UnityEngine;

public class Interaction_Morgue : Interaction
{
	public static List<Interaction_Morgue> Morgues = new List<Interaction_Morgue>();

	public Structure Structure;

	private Structures_Morgue _StructureInfo;

	[SerializeField]
	private GameObject holePosition;

	[SerializeField]
	private ItemGauge itemGauge;

	[SerializeField]
	private GameObject full;

	[SerializeField]
	private GameObject hasBodiesIcon;

	[SerializeField]
	private GameObject[] bodies;

	private bool activated;

	private string _label;

	private int _bodyDirection;

	private List<Vector3> _bodyDirections = new List<Vector3>
	{
		new Vector3(-1f, 0f, 0f),
		new Vector3(-1f, -1f, 0f),
		new Vector3(0f, -1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(1f, -1f, 0f)
	};

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Morgue structureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Morgue;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	private void Start()
	{
		_bodyDirections.Shuffle();
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (structureBrain != null)
		{
			OnBrainAssigned();
		}
		Morgues.Add(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateLocalisation();
		if (structureBrain != null)
		{
			UpdateGauge();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Morgues.Remove(this);
		if (Structure != null)
		{
			Structure structure = Structure;
			structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		}
		if (structureBrain != null)
		{
			structureBrain.OnBodyDeposited -= UpdateGauge;
			structureBrain.OnBodyWithdrawn -= UpdateGauge;
		}
	}

	private void OnBrainAssigned()
	{
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		structureBrain.OnBodyDeposited += UpdateGauge;
		structureBrain.OnBodyWithdrawn += UpdateGauge;
		UpdateGauge();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		_label = ScriptLocalization.Interactions.Collect;
	}

	public override void GetLabel()
	{
		base.GetLabel();
		base.Label = _label + " (" + structureBrain.Data.MultipleFollowerIDs.Count + "/" + structureBrain.DEAD_BODY_SLOTS + ")";
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (activated)
		{
			return;
		}
		activated = true;
		GameManager.GetInstance().OnConversationNew();
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		UIDeadFollowerSelectMenu uIDeadFollowerSelectMenu = MonoSingleton<UIManager>.Instance.DeadFollowerSelectMenuTemplate.Instantiate();
		List<FollowerInfo> list = new List<FollowerInfo>();
		foreach (FollowerInfo item in DataManager.Instance.Followers_Dead)
		{
			if (structureBrain.Data.MultipleFollowerIDs.Contains(item.ID))
			{
				list.Add(item);
			}
		}
		uIDeadFollowerSelectMenu.Show(list.Count, _StructureInfo.DEAD_BODY_SLOTS, list);
		uIDeadFollowerSelectMenu.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIDeadFollowerSelectMenu.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			StartCoroutine(SpawnQueue(followerInfo));
		});
		uIDeadFollowerSelectMenu.OnCancel = (Action)Delegate.Combine(uIDeadFollowerSelectMenu.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().WaitForSecondsRealtime(0.1f, delegate
			{
				OnHidden();
				base.HasChanged = true;
			});
		});
	}

	private IEnumerator SpawnQueue(FollowerInfo followerInfo)
	{
		yield return new WaitForSecondsRealtime(0.5f);
		Time.timeScale = 1f;
		HUD_Manager.Instance.Hide(true);
		structureBrain.Data.MultipleFollowerIDs.Remove(followerInfo.ID);
		StructuresData infoByType = StructuresData.GetInfoByType(StructureBrain.TYPES.DEAD_WORSHIPPER, 0);
		infoByType.Position = holePosition.transform.position;
		infoByType.FollowerID = followerInfo.ID;
		infoByType.BeenInMorgueAlready = true;
		StructureManager.BuildStructure(FollowerLocation.Base, infoByType, infoByType.Position, Vector2Int.one, false, delegate(GameObject g)
		{
			DeadWorshipper component = g.GetComponent<DeadWorshipper>();
			int mask = LayerMask.GetMask("Island");
			Collider2D collider2D = Physics2D.OverlapCircle(base.transform.position, 1f, mask);
			if ((bool)collider2D)
			{
				component.BounceOutFromPosition(10f, (collider2D.transform.position - base.transform.position).normalized);
			}
			else
			{
				component.BounceOutFromPosition(10f, _bodyDirections[_bodyDirection]);
			}
			if (_bodyDirection < _bodyDirections.Count - 1)
			{
				_bodyDirection++;
			}
			else
			{
				_bodyDirection = 0;
			}
			PlacementRegion.TileGridTile closestTileGridTileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(component.transform.position);
			if (closestTileGridTileAtWorldPosition != null)
			{
				component.Structure.Brain.AddToGrid(closestTileGridTileAtWorldPosition.Position);
			}
		});
		if (DataManager.Instance.TotalBodiesHarvested >= 10 && !DataManager.Instance.PlayerFoundRelics.Contains(RelicType.SpawnCombatFollowerFromBodies) && DataManager.Instance.OnboardedRelics)
		{
			bool waiting = true;
			RelicCustomTarget.Create(base.transform.position + Vector3.up * 0.5f, base.transform.position + Vector3.up * 0.5f - Vector3.forward, 1.5f, RelicType.SpawnCombatFollowerFromBodies, delegate
			{
				waiting = false;
			});
			while (waiting)
			{
				yield return null;
			}
		}
		OnHidden();
		base.HasChanged = true;
		UpdateGauge();
	}

	private void OnHidden()
	{
		Time.timeScale = 1f;
		activated = false;
		HUD_Manager.Instance.Show();
		GameManager.GetInstance().OnConversationEnd();
	}

	private void UpdateGauge()
	{
		GameObject[] array = bodies;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(false);
		}
		if (structureBrain.Data.MultipleFollowerIDs.Count > 0)
		{
			bodies[structureBrain.Data.MultipleFollowerIDs.Count - 1].gameObject.SetActive(true);
		}
		itemGauge.SetPosition((float)structureBrain.Data.MultipleFollowerIDs.Count / (float)structureBrain.DEAD_BODY_SLOTS);
		full.SetActive(structureBrain.Data.MultipleFollowerIDs.Count >= structureBrain.DEAD_BODY_SLOTS);
		hasBodiesIcon.gameObject.SetActive(structureBrain.Data.MultipleFollowerIDs.Count > 0 && !structureBrain.IsFull);
	}
}
