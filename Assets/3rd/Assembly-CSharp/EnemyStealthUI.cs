using UnityEngine;

public class EnemyStealthUI : BaseMonoBehaviour
{
	public SpriteRenderer RadialProgress;

	public void UpdateProgress(float Progress)
	{
		RadialProgress.material.SetFloat("_Arc2", 360f - Progress * 360f);
	}
}
