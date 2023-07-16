using UnityEngine;

public class DebugBar : MonoBehaviour
{
	private Texture2D blackTexture;

	private int _cameraCount;

	private double _sumFPS;

	private int _sumCount;

	private double _avg;

	private float _min = 999f;

	private float _max;

	public static int CameraCount;

	public float _fps
	{
		get
		{
			return DynamicResolutionManager._fps;
		}
	}

	public double _previousInterp
	{
		get
		{
			return DynamicResolutionManager._previousInterp;
		}
	}

	public double m_gpuFrameTime
	{
		get
		{
			return DynamicResolutionManager.m_gpuFrameTime;
		}
	}
}
