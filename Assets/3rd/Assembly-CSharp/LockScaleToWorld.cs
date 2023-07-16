using UnityEngine;

public class LockScaleToWorld : MonoBehaviour
{
	private void Update()
	{
		base.transform.localScale = new Vector3(base.transform.parent.localScale.x, base.transform.localScale.y, base.transform.localScale.z);
	}
}
