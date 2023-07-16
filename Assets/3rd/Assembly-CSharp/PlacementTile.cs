using UnityEngine;
using UnityFx.Outline;

public class PlacementTile : BaseMonoBehaviour
{
	protected OutlineEffect Outliner;

	public Vector3 Position;

	public SpriteRenderer spriteRenderer;

	public Vector2Int GridPosition;

	[SerializeField]
	private SpriteRenderer _blockedOutline;

	[SerializeField]
	private Sprite normalOutline;

	[SerializeField]
	private Sprite editOutline;

	private bool hadOutline;

	private int changed;

	private void OnEnable()
	{
	}

	public void SetColor(Color color, Vector3 buildingPos)
	{
		spriteRenderer.color = color;
		if (color == Color.red)
		{
			if (!CheatConsole.HidingUI)
			{
				_blockedOutline.enabled = true;
			}
			spriteRenderer.color = new Vector4(1f, 1f, 1f, 1f);
		}
		else if (color == Color.white)
		{
			_blockedOutline.enabled = false;
			spriteRenderer.color = new Vector4(1f, 1f, 1f, 0.5f);
			spriteRenderer.sprite = normalOutline;
		}
		else if (color == StaticColors.OrangeColor)
		{
			_blockedOutline.enabled = false;
			spriteRenderer.color = StaticColors.OrangeColor;
			spriteRenderer.sprite = editOutline;
		}
		else
		{
			spriteRenderer.color = new Vector4(0f, 1f, 0f, 1f);
			spriteRenderer.sprite = editOutline;
		}
	}
}
