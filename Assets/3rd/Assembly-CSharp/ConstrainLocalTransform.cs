using UnityEngine;

[ExecuteInEditMode]
public class ConstrainLocalTransform : BaseMonoBehaviour
{
	public bool ConstrainTransform = true;

	private Vector3 m_LastPosition = Vector3.zero;

	private Quaternion m_LastRotation = Quaternion.identity;

	private Vector3 m_LastScale = Vector3.zero;
}
