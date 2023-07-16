using UnityEngine;

public class SpecialLeaderWhisper : MonoBehaviour
{
	public void AddLocation()
	{
		DataManager.Instance.LeaderSpecialEncounteredLocations.Add(PlayerFarming.Location);
	}
}
