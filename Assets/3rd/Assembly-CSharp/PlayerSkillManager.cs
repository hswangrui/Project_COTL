using UnityEngine;

public class PlayerSkillManager
{
	public static float GetPlayerSkillValue()
	{
		return DataManager.Instance.PlayerDamageDealt / Mathf.Clamp(DataManager.Instance.PlayerDamageReceived, 0.1f, float.MaxValue);
	}

	public static float GetPlayerTotal()
	{
		return DataManager.Instance.PlayerDamageDealt + DataManager.Instance.PlayerDamageReceived;
	}
}
