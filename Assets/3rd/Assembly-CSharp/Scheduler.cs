using UnityEngine;

[RequireComponent(typeof(WorshipperInfoManager))]
public class Scheduler : BaseMonoBehaviour
{
	public enum Activities
	{
		None,
		Free,
		TravelToWork,
		Working,
		TravelToDwelling,
		Sleeping,
		TravelToWorship,
		Worship
	}

	private Activities currentActivity;

	public WorshipperInfoManager wim;

	public Activities CurrentActivity
	{
		get
		{
			if (wim == null || wim.v_i == null)
			{
				currentActivity = Activities.Free;
			}
			else
			{
				GetCurrentSchedule();
			}
			return currentActivity;
		}
		set
		{
			currentActivity = value;
		}
	}

	private void Start()
	{
		wim = GetComponent<WorshipperInfoManager>();
	}

	public void GetCurrentSchedule()
	{
	}
}
