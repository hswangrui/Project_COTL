using System;
using System.Collections.Generic;

[Serializable]
public class DayObject
{
	public int MoonPhase;

	public int TotalDays;

	public void Init(int MoonPhase, int TotalDays)
	{
		this.MoonPhase = MoonPhase;
		this.TotalDays = TotalDays;
	}

	public static void Reset()
	{
		DataManager.Instance.DayList = new List<DayObject>();
		int num = -1;
		while (++num < 3)
		{
			DayObject dayObject = new DayObject();
			dayObject.Init(num, num + 1);
			DataManager.Instance.DayList.Add(dayObject);
		}
		DataManager.Instance.CurrentDay = DataManager.Instance.DayList[0];
	}

	public static void NewDay()
	{
		if (DataManager.Instance.DayList.Count > 0)
		{
			DataManager.Instance.DayList.RemoveAt(0);
			DataManager.Instance.CurrentDay = DataManager.Instance.DayList[0];
			DayObject dayObject = DataManager.Instance.DayList[DataManager.Instance.DayList.Count - 1];
			int moonPhase = (dayObject.MoonPhase + 1) % 6;
			int totalDays = dayObject.TotalDays + 1;
			DayObject dayObject2 = new DayObject();
			dayObject2.Init(moonPhase, totalDays);
			DataManager.Instance.DayList.Add(dayObject2);
		}
	}
}
