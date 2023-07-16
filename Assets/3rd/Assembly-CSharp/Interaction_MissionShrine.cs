using UnityEngine;

public class Interaction_MissionShrine : Interaction
{
	[SerializeField]
	private GameObject missionAvailableEffect;

	[SerializeField]
	private GameObject container;

	private Structures_MissionShrine _StructureInfo;

	private string key;

	public Structure Structure { get; private set; }

	public StructuresData StructureInfo
	{
		get
		{
			return Structure.Structure_Info;
		}
	}

	public Structures_MissionShrine StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = Structure.Brain as Structures_MissionShrine;
			}
			return _StructureInfo;
		}
		set
		{
			_StructureInfo = value;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Structure = GetComponentInParent<Structure>();
	}

	protected override void Update()
	{
		base.Update();
		Interactable = StructureInfo != null && DataManager.Instance.MissionShrineUnlocked;
		missionAvailableEffect.SetActive(StructureInfo != null && DataManager.Instance.AvailableMissions.Count > 0);
		container.SetActive(Interactable);
		if (DataManager.Instance.NewMissionDayTimestamp == -1f || !((float)TimeManager.CurrentDay >= DataManager.Instance.NewMissionDayTimestamp))
		{
			return;
		}
		if (CanAddNewMission())
		{
			AddNewMission();
		}
		else
		{
			DataManager.Instance.NewMissionDayTimestamp = TimeManager.CurrentDay + 1;
		}
		for (int num = DataManager.Instance.ActiveMissions.Count - 1; num >= 0; num--)
		{
			if (TimeManager.TotalElapsedGameTime >= DataManager.Instance.ActiveMissions[num].ExpiryTimestamp)
			{
				MissionManager.RemoveMission(DataManager.Instance.ActiveMissions[num]);
			}
		}
		for (int num2 = DataManager.Instance.AvailableMissions.Count - 1; num2 >= 0; num2--)
		{
			if (TimeManager.TotalElapsedGameTime >= DataManager.Instance.AvailableMissions[num2].ExpiryTimestamp)
			{
				DataManager.Instance.AvailableMissions.RemoveAt(num2);
			}
		}
	}

	public void AddNewMission()
	{
		int num = 3 - (DataManager.Instance.AvailableMissions.Count + DataManager.Instance.ActiveMissions.Count);
		for (int i = 0; i < num; i++)
		{
			MissionManager.AddMission(MissionManager.MissionType.Bounty, Random.Range(1, 4), IsGoldenMission());
		}
		DataManager.Instance.NewMissionDayTimestamp = TimeManager.CurrentDay + 1;
	}

	private bool IsGoldenMission()
	{
		if (DataManager.Instance.LastGoldenMissionDay == -1)
		{
			DataManager.Instance.LastGoldenMissionDay = TimeManager.CurrentDay;
		}
		bool flag = false;
		int num = TimeManager.CurrentDay - DataManager.Instance.LastGoldenMissionDay;
		float num2 = Random.Range(0f, 1f);
		if (0.05f * (float)num >= num2)
		{
			flag = true;
		}
		if (flag)
		{
			DataManager.Instance.LastGoldenMissionDay = TimeManager.CurrentDay;
		}
		return flag;
	}

	private bool CanAddNewMission()
	{
		return DataManager.Instance.AvailableMissions.Count + DataManager.Instance.ActiveMissions.Count < 3;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
	}

	public override void GetLabel()
	{
		base.Label = (Interactable ? key : "");
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}
}
