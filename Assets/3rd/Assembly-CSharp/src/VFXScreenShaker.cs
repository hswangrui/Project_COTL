using Unity.Mathematics;
using UnityEngine;

namespace src
{
	public class VFXScreenShaker : MonoBehaviour
	{
		[SerializeField]
		private float2 _intensityMinMax = new float2(0.1f, 0.2f);

		[SerializeField]
		private float _duration = 0.5f;

		public void ScreenShake()
		{
			CameraManager.instance.ShakeCameraForDuration(_intensityMinMax.x, _intensityMinMax.y, _duration, false);
		}
	}
}
