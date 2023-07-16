using UnityEngine;

public class ModifierIcon : MonoBehaviour
{
	private EnemyModifier modifier;

	public SpriteRenderer spriteRenderer;

	public GameObject TimerObject;

	public SpriteRenderer TimerProgress;

	private bool hasTimer;

	public void Init(EnemyModifier _modifier)
	{
		modifier = _modifier;
		spriteRenderer.sprite = modifier.ModifierIconSprite;
		hasTimer = _modifier.HasTimer;
		if (hasTimer)
		{
			TimerObject.SetActive(true);
			TimerProgress.material = new Material(TimerProgress.material);
		}
		else
		{
			TimerObject.SetActive(false);
		}
	}

	public void UpdateTimer(float progress)
	{
		TimerProgress.material.SetFloat("_Progress", progress);
	}
}
