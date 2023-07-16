using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class MMCanvasScaler : CanvasScaler
	{
		public const int kReferenceResolution_Width = 1920;

		public const int kReferenceResolution_Height = 1080;

		public static float CanvasScale
		{
			get
			{
				return Mathf.Min((float)Screen.width / 1920f, (float)Screen.height / 1080f);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (Application.isPlaying)
			{
				if (SettingsManager.Settings != null)
				{
					OnUIScaleChanged();
				}
			}
			else
			{
				base.referenceResolution = new Vector2(1920f, 1080f);
				base.uiScaleMode = ScaleMode.ScaleWithScreenSize;
				base.screenMatchMode = ScreenMatchMode.Expand;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			bool isPlaying = Application.isPlaying;
		}

		private void OnUIScaleChanged()
		{
			float num = 1f;
			base.referenceResolution = new Vector2(1920f + 1920f * (1f - num), 1080f + 1080f * (1f - num));
		}
	}
}
