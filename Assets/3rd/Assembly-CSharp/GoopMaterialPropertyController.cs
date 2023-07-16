using UnityEngine;

public class GoopMaterialPropertyController : MaterialImagePropertyController
{
	private const string KPositionOffset = "_PositionOffset";

	[SerializeField]
	private Vector2 positionOffsetProperty = new Vector2(0f, 0f);

	private static readonly int PositionOffset = Shader.PropertyToID("_PositionOffset");

	protected override void UpdateMaterialProperties()
	{
		_material.SetVector(PositionOffset, new Vector4(positionOffsetProperty.x, positionOffsetProperty.y, 0f, 0f));
	}
}
