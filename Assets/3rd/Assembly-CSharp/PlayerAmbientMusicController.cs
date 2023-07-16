using UnityEngine;

public class PlayerAmbientMusicController : BaseMonoBehaviour
{
	public Health health;

	private bool isPlaying;

	private float Timer;

	private void Update()
	{
		if (!AmbientMusicController.AmbientIsPlaying)
		{
			return;
		}
		if (!isPlaying && health.attackers.Count > 0)
		{
			isPlaying = true;
			AmbientMusicController.PlayAmbientCombat();
			AudioManager.Instance.SetMusicCombatState();
			Timer = 0f;
		}
		if (!isPlaying || health.attackers.Count > 0)
		{
			return;
		}
		foreach (Health item in Health.team2)
		{
			if (item != null && item.team != health.team && !item.InanimateObject && Vector3.Distance(base.transform.position, item.transform.position) < 6f)
			{
				return;
			}
		}
		if (!((Timer += Time.deltaTime) < 1f))
		{
			isPlaying = false;
			AmbientMusicController.StopAmbientCombat();
			AudioManager.Instance.SetMusicCombatState(false);
		}
	}
}
