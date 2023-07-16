using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class StencilBufferRender : BaseMonoBehaviour
{
	private CommandBuffer _cmdBuffer;

	private static RenderTexture _stencilRenderTexture;

	private Material _postProcessMat;

	private Resolution _currentScreenRes;

	private const string cmdBufferName = "StencilBufferRT";

	private CameraEvent camInsertPoint = CameraEvent.AfterForwardOpaque;

	private static Mesh s_Quad;

	private Camera _cam
	{
		get
		{
			return base.gameObject.GetComponent<Camera>();
		}
	}

	public static Mesh quad
	{
		get
		{
			if (s_Quad != null)
			{
				return s_Quad;
			}
			Vector3[] vertices = new Vector3[4]
			{
				new Vector3(-0.5f, -0.5f, 0f),
				new Vector3(0.5f, 0.5f, 0f),
				new Vector3(0.5f, -0.5f, 0f),
				new Vector3(-0.5f, 0.5f, 0f)
			};
			Vector2[] uv = new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f),
				new Vector2(0f, 1f)
			};
			int[] triangles = new int[6] { 0, 1, 2, 1, 0, 3 };
			s_Quad = new Mesh
			{
				vertices = vertices,
				uv = uv,
				triangles = triangles
			};
			s_Quad.RecalculateNormals();
			s_Quad.RecalculateBounds();
			return s_Quad;
		}
	}

	private void OnPreRender()
	{
		if (_currentScreenRes.height != _cam.pixelHeight || _currentScreenRes.width != _cam.pixelWidth)
		{
			SetCurrentDim();
			CreateCommandBuffer();
		}
		SetCurrentDim();
	}

	private void SetCurrentDim()
	{
		_currentScreenRes.height = _cam.pixelHeight;
		_currentScreenRes.width = _cam.pixelWidth;
	}

	private void CreateCommandBuffer()
	{
		DestroyCommandBuffer();
		_cmdBuffer = new CommandBuffer();
		_cmdBuffer.name = "StencilBufferRT";
		_stencilRenderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
		{
			antiAliasing = 1,
			autoGenerateMips = false,
			filterMode = FilterMode.Point,
			name = "StencilRT" + Time.frameCount
		};
		_cmdBuffer.SetRenderTarget(_stencilRenderTexture, BuiltinRenderTextureType.CameraTarget);
		_cmdBuffer.ClearRenderTarget(false, true, Color.black);
		Shader shader = Shader.Find("Hidden/StencilToRT");
		_postProcessMat = new Material(shader);
		Matrix4x4 cameraToWorldMatrix = _cam.cameraToWorldMatrix;
		_cmdBuffer.DrawMesh(quad, Matrix4x4.identity, _postProcessMat, 0, 0);
		_cmdBuffer.SetGlobalTexture("_StencilBufferGlobalTex", _stencilRenderTexture);
		_cam.AddCommandBuffer(camInsertPoint, _cmdBuffer);
	}

	private void DestroyCommandBuffer()
	{
		if (_cmdBuffer != null)
		{
			_cam.RemoveCommandBuffer(camInsertPoint, _cmdBuffer);
			_cmdBuffer.Clear();
			_cmdBuffer.Dispose();
			_cmdBuffer = null;
		}
		CommandBuffer[] commandBuffers = _cam.GetCommandBuffers(camInsertPoint);
		foreach (CommandBuffer commandBuffer in commandBuffers)
		{
			if (commandBuffer.name == "StencilBufferRT")
			{
				_cam.RemoveCommandBuffer(camInsertPoint, commandBuffer);
				commandBuffer.Clear();
				commandBuffer.Dispose();
			}
		}
		if (_stencilRenderTexture != null)
		{
			_stencilRenderTexture.Release();
			_stencilRenderTexture.DiscardContents();
			Object.Destroy(_stencilRenderTexture);
			_stencilRenderTexture = null;
		}
	}

	private void OnDestroy()
	{
		DestroyCommandBuffer();
	}
}
