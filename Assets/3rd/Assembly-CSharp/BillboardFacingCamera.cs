using UnityEngine;

public class BillboardFacingCamera : BaseMonoBehaviour
{
	private Camera m_Camera;

	private void OnEnable()
	{
		m_Camera = Camera.main;
		Canvas component = GetComponent<Canvas>();
		if (component != null)
		{
			component.worldCamera = m_Camera;
		}
	}

	private void Update()
	{
		if (!(m_Camera == null))
		{
			base.transform.LookAt(base.transform.position + m_Camera.transform.rotation * Vector3.forward, m_Camera.transform.rotation * Vector3.up);
		}
	}
}
