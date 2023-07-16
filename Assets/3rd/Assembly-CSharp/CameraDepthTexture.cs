using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraDepthTexture : BaseMonoBehaviour
{
	private void Awake()
	{
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
	}
}
