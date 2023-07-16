using UnityEngine;

[CreateAssetMenu(menuName = "Massive Monster/Sticker Data")]
public class StickerData : ScriptableObject
{
	public Sprite Sticker;

	public float MinScale = 0.5f;

	public float MaxScale = 2f;

	public bool UsePrefab;

	public StickerItem Prefab;
}
