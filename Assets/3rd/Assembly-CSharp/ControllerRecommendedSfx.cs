using UnityEngine;

public class ControllerRecommendedSfx : MonoBehaviour
{
	public void PlayDing()
	{
		AudioManager.Instance.PlayOneShot("event:/ui/glass_ball_ding");
		MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact);
	}
}
