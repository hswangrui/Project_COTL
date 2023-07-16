using System;

[Serializable]
public abstract class Objectives_RoomChallenge : ObjectivesData
{
	public override string Text
	{
		get
		{
			return "";
		}
	}

	public override bool AutoTrack
	{
		get
		{
			return true;
		}
	}

	public int RoomsRequired { get; set; }

	public int RoomsCompleted { get; set; }

	public Objectives_RoomChallenge()
	{
	}

	public Objectives_RoomChallenge(string groupId, int roomsRequired)
		: base(groupId)
	{
		RoomsRequired = roomsRequired;
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		RoomsCompleted = 0;
		AutoRemoveQuestOnceComplete = false;
	}

	private void RoomLockController_OnRoomCleared()
	{
		RoomsCompleted++;
		TryComplete();
	}

	protected override bool CheckComplete()
	{
		if (initialised && !IsFailed)
		{
			return RoomsCompleted >= RoomsRequired;
		}
		return false;
	}
}
