using UnityEngine;

namespace Unify
{
	public class ScreenShot
	{
		public enum ImageFormats
		{
			JPEG,
			PNG
		}

		private static ScreenShot singleton;

		public string ProjectName;

		public static ScreenShot Instance
		{
			get
			{
				return singleton;
			}
		}

		public static void Init()
		{
			Debug.Log("ScreenShot Init called");
			singleton = new ScreenShot();
			singleton.ProjectName = Application.productName;
		}

		public virtual void Update()
		{
		}

		public virtual void Terminate()
		{
		}

		public virtual void TakeAndShareScreenshot(Texture2D screenShot, ImageFormats _imageFormat = ImageFormats.JPEG)
		{
			screenShot.EncodeToJPG(100);
			Debug.Log("ScreenShot Share Encoding successful");
		}

		public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
		{
			Texture2D texture2D = new Texture2D(targetWidth, targetHeight, source.format, false);
			Color[] pixels = texture2D.GetPixels(0);
			float num = 1f / (float)targetWidth;
			float num2 = 1f / (float)targetHeight;
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = source.GetPixelBilinear(num * ((float)i % (float)targetWidth), num2 * Mathf.Floor(i / targetWidth));
			}
			texture2D.SetPixels(pixels, 0);
			texture2D.Apply();
			return texture2D;
		}

		public virtual void SavingScreenshot()
		{
		}
	}
}
