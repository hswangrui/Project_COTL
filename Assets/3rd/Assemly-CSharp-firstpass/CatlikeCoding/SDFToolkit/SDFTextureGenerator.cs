using UnityEngine;

namespace CatlikeCoding.SDFToolkit
{
	public static class SDFTextureGenerator
	{
		private class Pixel
		{
			public float alpha;

			public float distance;

			public Vector2 gradient;

			public int dX;

			public int dY;
		}

		private static int width;

		private static int height;

		private static Pixel[,] pixels;

		public static void Generate(Texture2D source, Texture2D destination, float maxInside, float maxOutside, float postProcessDistance, RGBFillMode rgbMode)
		{
			if (source.height != destination.height || source.width != destination.width)
			{
				Debug.LogError("Source and destination textures must be the same size.");
				return;
			}
			width = source.width;
			height = source.height;
			pixels = new Pixel[width, height];
			Color color = ((rgbMode == RGBFillMode.White) ? Color.white : Color.black);
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					pixels[j, i] = new Pixel();
				}
			}
			if (maxInside > 0f)
			{
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						pixels[j, i].alpha = 1f - source.GetPixel(j, i).a;
					}
				}
				ComputeEdgeGradients();
				GenerateDistanceTransform();
				if (postProcessDistance > 0f)
				{
					PostProcess(postProcessDistance);
				}
				float num = 1f / maxInside;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						color.a = Mathf.Clamp01(pixels[j, i].distance * num);
						destination.SetPixel(j, i, color);
					}
				}
			}
			if (maxOutside > 0f)
			{
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						pixels[j, i].alpha = source.GetPixel(j, i).a;
					}
				}
				ComputeEdgeGradients();
				GenerateDistanceTransform();
				if (postProcessDistance > 0f)
				{
					PostProcess(postProcessDistance);
				}
				float num = 1f / maxOutside;
				if (maxInside > 0f)
				{
					for (int i = 0; i < height; i++)
					{
						for (int j = 0; j < width; j++)
						{
							color.a = 0.5f + (destination.GetPixel(j, i).a - Mathf.Clamp01(pixels[j, i].distance * num)) * 0.5f;
							destination.SetPixel(j, i, color);
						}
					}
				}
				else
				{
					for (int i = 0; i < height; i++)
					{
						for (int j = 0; j < width; j++)
						{
							color.a = Mathf.Clamp01(1f - pixels[j, i].distance * num);
							destination.SetPixel(j, i, color);
						}
					}
				}
			}
			switch (rgbMode)
			{
			case RGBFillMode.Distance:
			{
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						color = destination.GetPixel(j, i);
						color.r = color.a;
						color.g = color.a;
						color.b = color.a;
						destination.SetPixel(j, i, color);
					}
				}
				break;
			}
			case RGBFillMode.Source:
			{
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						color = source.GetPixel(j, i);
						color.a = destination.GetPixel(j, i).a;
						destination.SetPixel(j, i, color);
					}
				}
				break;
			}
			}
			pixels = null;
		}

		private static void ComputeEdgeGradients()
		{
			float num = Mathf.Sqrt(2f);
			for (int i = 1; i < height - 1; i++)
			{
				for (int j = 1; j < width - 1; j++)
				{
					Pixel pixel = pixels[j, i];
					if (pixel.alpha > 0f && pixel.alpha < 1f)
					{
						float num2 = 0f - pixels[j - 1, i - 1].alpha - pixels[j - 1, i + 1].alpha + pixels[j + 1, i - 1].alpha + pixels[j + 1, i + 1].alpha;
						pixel.gradient.x = num2 + (pixels[j + 1, i].alpha - pixels[j - 1, i].alpha) * num;
						pixel.gradient.y = num2 + (pixels[j, i + 1].alpha - pixels[j, i - 1].alpha) * num;
						pixel.gradient.Normalize();
					}
				}
			}
		}

		private static float ApproximateEdgeDelta(float gx, float gy, float a)
		{
			if (gx == 0f || gy == 0f)
			{
				return 0.5f - a;
			}
			float num = Mathf.Sqrt(gx * gx + gy * gy);
			gx /= num;
			gy /= num;
			gx = Mathf.Abs(gx);
			gy = Mathf.Abs(gy);
			if (gx < gy)
			{
				float num2 = gx;
				gx = gy;
				gy = num2;
			}
			float num3 = 0.5f * gy / gx;
			if (a < num3)
			{
				return 0.5f * (gx + gy) - Mathf.Sqrt(2f * gx * gy * a);
			}
			if (a < 1f - num3)
			{
				return (0.5f - a) * gx;
			}
			return -0.5f * (gx + gy) + Mathf.Sqrt(2f * gx * gy * (1f - a));
		}

		private static void UpdateDistance(Pixel p, int x, int y, int oX, int oY)
		{
			Pixel pixel = pixels[x + oX, y + oY];
			Pixel pixel2 = pixels[x + oX - pixel.dX, y + oY - pixel.dY];
			if (pixel2.alpha != 0f && pixel2 != p)
			{
				int num = pixel.dX - oX;
				int num2 = pixel.dY - oY;
				float num3 = Mathf.Sqrt(num * num + num2 * num2) + ApproximateEdgeDelta(num, num2, pixel2.alpha);
				if (num3 < p.distance)
				{
					p.distance = num3;
					p.dX = num;
					p.dY = num2;
				}
			}
		}

		private static void GenerateDistanceTransform()
		{
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					Pixel pixel = pixels[j, i];
					pixel.dX = 0;
					pixel.dY = 0;
					if (pixel.alpha <= 0f)
					{
						pixel.distance = 1000000f;
					}
					else if (pixel.alpha < 1f)
					{
						pixel.distance = ApproximateEdgeDelta(pixel.gradient.x, pixel.gradient.y, pixel.alpha);
					}
					else
					{
						pixel.distance = 0f;
					}
				}
			}
			for (int i = 1; i < height; i++)
			{
				Pixel pixel = pixels[0, i];
				if (pixel.distance > 0f)
				{
					UpdateDistance(pixel, 0, i, 0, -1);
					UpdateDistance(pixel, 0, i, 1, -1);
				}
				for (int j = 1; j < width - 1; j++)
				{
					pixel = pixels[j, i];
					if (pixel.distance > 0f)
					{
						UpdateDistance(pixel, j, i, -1, 0);
						UpdateDistance(pixel, j, i, -1, -1);
						UpdateDistance(pixel, j, i, 0, -1);
						UpdateDistance(pixel, j, i, 1, -1);
					}
				}
				pixel = pixels[width - 1, i];
				if (pixel.distance > 0f)
				{
					UpdateDistance(pixel, width - 1, i, -1, 0);
					UpdateDistance(pixel, width - 1, i, -1, -1);
					UpdateDistance(pixel, width - 1, i, 0, -1);
				}
				for (int j = width - 2; j >= 0; j--)
				{
					pixel = pixels[j, i];
					if (pixel.distance > 0f)
					{
						UpdateDistance(pixel, j, i, 1, 0);
					}
				}
			}
			for (int i = height - 2; i >= 0; i--)
			{
				Pixel pixel = pixels[width - 1, i];
				if (pixel.distance > 0f)
				{
					UpdateDistance(pixel, width - 1, i, 0, 1);
					UpdateDistance(pixel, width - 1, i, -1, 1);
				}
				for (int j = width - 2; j > 0; j--)
				{
					pixel = pixels[j, i];
					if (pixel.distance > 0f)
					{
						UpdateDistance(pixel, j, i, 1, 0);
						UpdateDistance(pixel, j, i, 1, 1);
						UpdateDistance(pixel, j, i, 0, 1);
						UpdateDistance(pixel, j, i, -1, 1);
					}
				}
				pixel = pixels[0, i];
				if (pixel.distance > 0f)
				{
					UpdateDistance(pixel, 0, i, 1, 0);
					UpdateDistance(pixel, 0, i, 1, 1);
					UpdateDistance(pixel, 0, i, 0, 1);
				}
				for (int j = 1; j < width; j++)
				{
					pixel = pixels[j, i];
					if (pixel.distance > 0f)
					{
						UpdateDistance(pixel, j, i, -1, 0);
					}
				}
			}
		}

		private static void PostProcess(float maxDistance)
		{
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					Pixel pixel = pixels[j, i];
					if ((pixel.dX == 0 && pixel.dY == 0) || pixel.distance >= maxDistance)
					{
						continue;
					}
					float num = pixel.dX;
					float num2 = pixel.dY;
					Pixel pixel2 = pixels[j - pixel.dX, i - pixel.dY];
					Vector2 gradient = pixel2.gradient;
					if (gradient.x != 0f || gradient.y != 0f)
					{
						float num3 = ApproximateEdgeDelta(gradient.x, gradient.y, pixel2.alpha);
						float num4 = num2 * gradient.x - num * gradient.y;
						float num5 = (0f - num3) * gradient.x + num4 * gradient.y;
						float num6 = (0f - num3) * gradient.y - num4 * gradient.x;
						if (Mathf.Abs(num5) <= 0.5f && Mathf.Abs(num6) <= 0.5f)
						{
							pixel.distance = Mathf.Sqrt((num + num5) * (num + num5) + (num2 + num6) * (num2 + num6));
						}
					}
				}
			}
		}
	}
}
