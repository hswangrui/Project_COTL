using UnityEngine;

namespace src
{
	public class VFXDeviceLighting : MonoBehaviour
	{
		public enum LightingType
		{
			None,
			Flash
		}

		[SerializeField]
		private LightingType lightType;

		[SerializeField]
		private Color color;

		[SerializeField]
		private float duration = 0.5f;

		public void ShowLighting()
		{
			LightingType lightingType = lightType;
			if (lightingType == LightingType.Flash)
			{
				DeviceLightingManager.FlashColor(color, duration);
			}
		}
	}
}
