using System.Collections.Generic;
using UnityEngine;

public class JobManager : BaseMonoBehaviour
{
	public static void NewJob(Vector3 JobLocation, string WorkPlaceID, int WorkPlaceSlot)
	{
		List<Worshipper> list = new List<Worshipper>();
		foreach (Worshipper worshipper2 in Worshipper.worshippers)
		{
			if (worshipper2.wim.v_i.WorkPlace == WorkPlace.NO_JOB)
			{
				list.Add(worshipper2);
			}
		}
		Worshipper worshipper = null;
		float num = float.MaxValue;
		foreach (Worshipper item in list)
		{
			float num2 = Vector3.Distance(JobLocation, item.transform.position);
			if (num2 < num)
			{
				worshipper = item;
				num = num2;
			}
		}
		if (worshipper != null)
		{
			Worshipper.ClearJob(worshipper);
			worshipper.AssignJob(WorkPlaceID, 0);
		}
	}
}
