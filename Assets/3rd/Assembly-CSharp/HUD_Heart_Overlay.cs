using System.Collections;
using Spine.Unity;
using UnityEngine;

public class HUD_Heart_Overlay : BaseMonoBehaviour
{
	public SkeletonGraphic healthOverlay;

	public float playerHealth;

	private bool overlayAdded;

	private bool disabling;

	private void Start()
	{
		healthOverlay.enabled = false;
	}

	private void Update()
	{
		playerHealth = DataManager.Instance.PLAYER_HEALTH + DataManager.Instance.PLAYER_BLUE_HEARTS;
		if (playerHealth <= 2f)
		{
			if (!healthOverlay.enabled)
			{
				disabling = false;
				healthOverlay.enabled = true;
				overlayAdded = true;
				healthOverlay.AnimationState.SetAnimation(0, "fastIn", false);
				healthOverlay.AnimationState.AddAnimation(0, "animation", true, 0f);
			}
		}
		else if (healthOverlay.enabled && !disabling)
		{
			StopAllCoroutines();
			StartCoroutine(disable());
		}
	}

	private IEnumerator disable()
	{
		disabling = true;
		float duration = healthOverlay.Skeleton.Data.FindAnimation("fastOut").Duration;
		healthOverlay.AnimationState.SetAnimation(0, "fastOut", false);
		yield return new WaitForSeconds(duration);
		healthOverlay.enabled = false;
	}
}
