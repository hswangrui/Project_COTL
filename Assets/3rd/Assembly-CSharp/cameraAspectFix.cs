using UnityEngine;

[RequireComponent(typeof(Camera))]
public class cameraAspectFix : BaseMonoBehaviour
{
	public bool copyMainCameraSettings;

	private Camera _referenceCamera;

	private Camera _thisCamera;

	private void Awake()
	{
		_thisCamera = base.gameObject.GetComponent<Camera>();
		_referenceCamera = Camera.main;
		_thisCamera.aspect = Camera.main.aspect;
		_thisCamera.nearClipPlane = Camera.main.nearClipPlane;
		_thisCamera.farClipPlane = Camera.main.farClipPlane;
	}

	private void Update()
	{
		_thisCamera.aspect = _referenceCamera.aspect;
		_thisCamera.nearClipPlane = _referenceCamera.nearClipPlane;
		_thisCamera.farClipPlane = _referenceCamera.farClipPlane;
		if (copyMainCameraSettings)
		{
			base.transform.position = _referenceCamera.transform.position;
		}
	}
}
