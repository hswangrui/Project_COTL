using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class BloodStoneStructure : Interaction
{
	private float Health = 3f;

	private float TotalHealth = 3f;

	public DropLootOnDeath DropLootOnDeath;

	public Transform[] tToShake;

	private List<Vector2> tShakes;

	public GameObject BuildSiteProgressUIPrefab;

	private BuildSitePlotProgressUI ProgressUI;

	public static Action<BloodStoneStructure> PlayerActivatingStart;

	public static Action<BloodStoneStructure> PlayerActivatingEnd;

	public static List<BloodStoneStructure> BloodStones = new List<BloodStoneStructure>();

	public Structure Structure;

	private Structures_Rubble _StructureInfo;

	private string sString;

	private bool Activating;

	private float ShowTimer;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_Rubble StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_Rubble;
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
		UpdateLocalisation();
		tShakes = new List<Vector2>();
		Transform[] array = tToShake;
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform2 = array[i];
			tShakes.Add(Vector2.zero);
		}
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Combine(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (Structure.Brain != null)
		{
			OnBrainAssigned();
		}
	}

	private void OnBrainAssigned()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(CanvasConstants.instance.BuildSiteProgressUIPrefab, GameObject.FindWithTag("Canvas").transform);
		ProgressUI = gameObject.GetComponent<BuildSitePlotProgressUI>();
		ProgressUI.gameObject.SetActive(false);
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		Structures_Rubble structureBrain = StructureBrain;
		structureBrain.OnRemovalProgressChanged = (Action<int>)Delegate.Combine(structureBrain.OnRemovalProgressChanged, new Action<int>(OnRemovalProgressChanged));
		OnRemovalProgressChanged(-1);
	}

	public override void OnEnableInteraction()
	{
		BloodStones.Add(this);
		base.OnEnableInteraction();
	}

	public override void OnDisableInteraction()
	{
		BloodStones.Remove(this);
		base.OnDisableInteraction();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.MineBloodStone;
	}

	public override void GetLabel()
	{
		base.Label = sString;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent += SimpleSpineAnimator_OnSpineEvent;
			Activating = true;
			StartCoroutine(DoBuild());
		}
	}

	private new void OnDestroy()
	{
		Action<BloodStoneStructure> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(this);
		}
		if (StructureInfo.Destroyed)
		{
			DropLootOnDeath.Play();
		}
		Structures_Rubble structureBrain = StructureBrain;
		structureBrain.OnRemovalProgressChanged = (Action<int>)Delegate.Remove(structureBrain.OnRemovalProgressChanged, new Action<int>(OnRemovalProgressChanged));
		Structure structure = Structure;
		structure.OnBrainAssigned = (Action)Delegate.Remove(structure.OnBrainAssigned, new Action(OnBrainAssigned));
		if (ProgressUI != null)
		{
			UnityEngine.Object.Destroy(ProgressUI.gameObject);
		}
		if (Activating)
		{
			StopAllCoroutines();
			PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent -= SimpleSpineAnimator_OnSpineEvent;
			state.CURRENT_STATE = StateMachine.State.Idle;
		}
	}

	public void ShowUI()
	{
		if (!ProgressUI.gameObject.activeSelf)
		{
			ProgressUI.Show();
		}
		ShowTimer = 3f;
	}

	private new void Update()
	{
		int num = -1;
		while (++num < tShakes.Count)
		{
			Vector2 value = tShakes[num];
			value.y += (0f - value.x) * 0.3f;
			value.x += (value.y *= 0.7f);
			tShakes[num] = value;
		}
		num = -1;
		while (++num < tToShake.Length)
		{
			if (tToShake[num] != null)
			{
				tToShake[num].localPosition = new Vector3(tShakes[num].x, tToShake[num].localPosition.y, tToShake[num].localPosition.z);
			}
		}
		if (ShowTimer > 0f)
		{
			ShowTimer -= Time.deltaTime;
			if (ShowTimer <= 0f)
			{
				ProgressUI.Hide();
			}
		}
	}

	private void LateUpdate()
	{
		ProgressUI.SetPosition(base.transform.position);
	}

	private IEnumerator DoBuild()
	{
		Action<BloodStoneStructure> playerActivatingStart = PlayerActivatingStart;
		if (playerActivatingStart != null)
		{
			playerActivatingStart(this);
		}
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("actions/chop-stone", 0, true);
		while (InputManager.Gameplay.GetInteractButtonHeld())
		{
			yield return null;
		}
		PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent -= SimpleSpineAnimator_OnSpineEvent;
		state.CURRENT_STATE = StateMachine.State.Idle;
		Activating = false;
		Action<BloodStoneStructure> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(this);
		}
	}

	private void SimpleSpineAnimator_OnSpineEvent(string EventName)
	{
		if (EventName == "Chop")
		{
			CameraManager.shakeCamera(0.5f);
			int num = -1;
			while (++num < tShakes.Count)
			{
				tShakes[num] = new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
			}
		}
	}

	private void OnRemovalProgressChanged(int followerID)
	{
		if (StructureBrain.RemovalProgress == 0f)
		{
			ProgressUI.gameObject.SetActive(false);
		}
		else
		{
			ShowUI();
		}
	}
}
