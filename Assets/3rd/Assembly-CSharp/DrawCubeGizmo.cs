using UnityEngine;

[ExecuteInEditMode]
public class DrawCubeGizmo : BaseMonoBehaviour
{
	[SerializeField]
	protected Vector3 halfExtents = Vector3.one;

	[SerializeField]
	protected Vector3 center = Vector3.zero;
}
