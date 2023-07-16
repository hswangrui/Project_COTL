using UnityEngine;

[RequireComponent(typeof(FollowPath))]
[RequireComponent(typeof(CircleCollider2D))]
public class DragToPath : BaseMonoBehaviour
{
	private FollowPath followpath;

	private void Start()
	{
		followpath = GetComponent<FollowPath>();
	}

	private void OnMouseDown()
	{
		MouseManager.ShowLine(base.transform);
	}

	private void OnMouseUp()
	{
		MouseManager.HideLine();
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 0f - Camera.main.transform.position.z;
		followpath.givePath(Camera.main.ScreenToWorldPoint(mousePosition));
	}
}
