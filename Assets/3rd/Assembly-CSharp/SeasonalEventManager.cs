using UnityEngine;

public static class SeasonalEventManager
{
	private static SeasonalEventData[] _cachedSeasonalEvents;

	private const string SeasonalEventsPath = "Data/Seasonal Event Data";

	private static SeasonalEventData[] SeasonalEvents
	{
		get
		{
			if (_cachedSeasonalEvents == null)
			{
				_cachedSeasonalEvents = Resources.LoadAll<SeasonalEventData>("Data/Seasonal Event Data");
			}
			return _cachedSeasonalEvents;
		}
	}

	public static bool InitialiseEvents()
	{
		bool result = false;
		SeasonalEventData[] seasonalEvents = SeasonalEvents;
		foreach (SeasonalEventData seasonalEventData in seasonalEvents)
		{
			if (seasonalEventData.IsEventActive() && !DataManager.Instance.ActiveSeasonalEvents.Contains(seasonalEventData.EventType))
			{
				ActivateEvent(seasonalEventData.EventType);
				result = true;
			}
			else if (!seasonalEventData.IsEventActive() && DataManager.Instance.ActiveSeasonalEvents.Contains(seasonalEventData.EventType))
			{
				DeactivateEvent(seasonalEventData.EventType);
			}
		}
		return result;
	}

	public static SeasonalEventData GetSeasonalEventData(SeasonalEventType eventType)
	{
		SeasonalEventData[] seasonalEvents = SeasonalEvents;
		foreach (SeasonalEventData seasonalEventData in seasonalEvents)
		{
			if (seasonalEventData.EventType == eventType)
			{
				return seasonalEventData;
			}
		}
		return null;
	}

	public static bool IsSeasonalEventActive(SeasonalEventType eventType)
	{
		SeasonalEventData[] seasonalEvents = SeasonalEvents;
		foreach (SeasonalEventData seasonalEventData in seasonalEvents)
		{
			if (seasonalEventData.EventType == eventType)
			{
				return seasonalEventData.IsEventActive();
			}
		}
		return false;
	}

	private static void ActivateEvent(SeasonalEventType eventType)
	{
		if (eventType == SeasonalEventType.Halloween)
		{
			ActivateHalloween();
		}
		DataManager.Instance.ActiveSeasonalEvents.Add(eventType);
	}

	private static void DeactivateEvent(SeasonalEventType eventType)
	{
		if (eventType == SeasonalEventType.Halloween)
		{
			DeactivateHalloween();
		}
		DataManager.Instance.ActiveSeasonalEvents.Remove(eventType);
	}

	public static SeasonalEventData GetActiveEvent()
	{
		SeasonalEventData[] seasonalEvents = SeasonalEvents;
		int num = 0;
		if (num < seasonalEvents.Length)
		{
			return seasonalEvents[num];
		}
		return null;
	}

	private static void ActivateHalloween()
	{
		UpgradeSystem.UnlockAbility(UpgradeSystem.Type.Ritual_Halloween);
		DataManager.Instance.LastHalloween = float.MinValue;
	}

	private static void DeactivateHalloween()
	{
		UpgradeSystem.LockAbility(UpgradeSystem.Type.Ritual_Halloween);
	}
}
