using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.U2D;

public class EnableRecieveShadowsSpriteRenderer : BaseMonoBehaviour
{
	public static EnableRecieveShadowsSpriteRenderer Instance;

	private void OnEnable()
	{
		BiomeGenerator.OnBiomeChangeRoom += UpdateSpriteRenderers;
	}

	private void OnDisable()
	{
		BiomeGenerator.OnBiomeChangeRoom -= UpdateSpriteRenderers;
	}

	public void UpdateSpriteRenderers()
	{
		SpriteRenderer[] array = Object.FindObjectsOfType(typeof(SpriteRenderer)) as SpriteRenderer[];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].receiveShadows = true;
		}
		SpriteShapeRenderer[] array2 = Object.FindObjectsOfType(typeof(SpriteShapeRenderer)) as SpriteShapeRenderer[];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].receiveShadows = true;
		}
	}

	public static void UpdateSpriteShadows()
	{
		Instance.UpdateSpriteRenderers();
	}

	private void Start()
	{
		Instance = this;
		UpdateSpriteRenderers();
	}
}
