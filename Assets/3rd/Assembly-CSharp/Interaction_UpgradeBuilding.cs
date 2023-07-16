using System.Collections.Generic;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_UpgradeBuilding : Interaction
{
	public Structure Structure;

	public List<GameObject> BuildingObjects = new List<GameObject>();

	public Transform PlayerPosition;

	public string LocTermInteraction;

	private bool Activating;

	private string sString;

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public StructureBrain StructureBrain
	{
		get
		{
			return Structure.Brain;
		}
	}

	private void Start()
	{
		ContinuouslyHold = true;
		SetPrefabs();
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = LocalizationManager.GetTranslation(LocTermInteraction);
	}

	public override void GetLabel()
	{
		base.Label = ((StructureBrain.Data.UpgradeLevel == 0 && !Activating) ? sString : "");
	}

	public override void OnBecomeCurrent()
	{
	}

	public override void OnBecomeNotCurrent()
	{
	}

	private void SetPrefabs()
	{
		int num = -1;
		while (++num < BuildingObjects.Count)
		{
			BuildingObjects[num].SetActive(num == StructureBrain.Data.UpgradeLevel);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if ((StructureBrain.Data.UpgradeLevel == 0) ? true : false)
		{
			Activating = true;
			base.OnInteract(state);
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 8f);
			PlayerFarming.Instance.GoToAndStop(PlayerPosition.position, base.gameObject, false, false, delegate
			{
				UpgradeBuilding(state);
			});
		}
	}

	private void UpgradeBuilding(StateMachine player)
	{
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		StructureBrain.Data.UpgradeLevel++;
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 1f, "", delegate
		{
			MMTransition.ResumePlay();
			SetPrefabs();
			if (player != null)
			{
				player.transform.position = PlayerPosition.position;
				player.CURRENT_STATE = StateMachine.State.InActive;
			}
			GameManager.GetInstance().OnConversationEnd();
		});
	}
}
