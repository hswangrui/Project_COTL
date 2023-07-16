using System;

[Serializable]
public class StructureEffect
{
	public float TimeStarted;

	public float DurationInGameMinutes;

	public float CoolDownInGameMinutes;

	public int StructureID;

	public bool CoolingDown;

	public StructureEffectManager.EffectType Type;

	public float CoolDownProgress
	{
		get
		{
			return (TimeManager.TotalElapsedGameTime - TimeStarted - DurationInGameMinutes) / CoolDownInGameMinutes;
		}
	}

	public void Create(int StructureID, StructureEffectManager.EffectType Type)
	{
		TimeStarted = TimeManager.TotalElapsedGameTime;
		this.StructureID = StructureID;
		DurationInGameMinutes = 1440f;
		CoolDownInGameMinutes = 1440f;
		this.Type = Type;
		NotificationCentre.Instance.PlayGenericNotificationLocalizedParams("Notifications/Structure/StructureEffectStarted", StructureEffectManager.GetLocalizedKey(Type));
	}

	public void BeginCooldown()
	{
		CoolingDown = true;
		NotificationCentre.Instance.PlayGenericNotificationLocalizedParams("Notifications/Structure/StructureEffectEnded", StructureEffectManager.GetLocalizedKey(Type));
	}
}
