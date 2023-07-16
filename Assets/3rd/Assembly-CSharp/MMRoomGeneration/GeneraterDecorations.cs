using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace MMRoomGeneration
{
	public class GeneraterDecorations : BaseMonoBehaviour
	{
		[Serializable]
		public class ListOfDecorations
		{
			public List<DecorationAndProbability> DecorationAndProabilies;

			private List<int> Weights;

			private int Index;

			public DecorationAndProbability GetRandomGameObject(double RandomSeed)
			{
				if (DecorationAndProabilies.Count <= 0)
				{
					return null;
				}
				Weights = new List<int>();
				int num = -1;
				while (++num < DecorationAndProabilies.Count)
				{
					Weights.Add(DecorationAndProabilies[num].Probability);
				}
				Index = GetRandomWeightedIndex(Weights, RandomSeed);
				return DecorationAndProabilies[Index];
			}
		}

		[Serializable]
		public class ListOfPerlinSpriteShape
		{
			public List<DecorationPerlinSpriteShape> DecorationAndProabilies = new List<DecorationPerlinSpriteShape>();
		}

		[Serializable]
		public class DecorationPerlinSpriteShape
		{
			public string ObjectPath;

			[Range(0f, 1f)]
			public float PerlinThreshold = 0.6f;

			public float PerlinScale = 350f;

			public float PerlinOffset;
		}

		[Serializable]
		public class DecorationAndProbability
		{
			[Range(0f, 100f)]
			public int Probability = 50;

			public string ObjectPath;

			public Vector2 RandomOffset;

			public Vector2 ZOffset = Vector2.zero;

			public Vector3 GetRandomOffset()
			{
				return new Vector3(UnityEngine.Random.Range(0f - RandomOffset.x, RandomOffset.x), UnityEngine.Random.Range(0f - RandomOffset.y, RandomOffset.y), 0f - UnityEngine.Random.Range(ZOffset.x, ZOffset.y));
			}
		}

		public Color biomeColor;

		public SpriteShape SpriteShape;

		public SpriteShape SpriteShapeSecondary;

		public SpriteShape SpriteShapeBack;

		public Material SpriteShapeMaterial;

		public Material SpriteShapeBackMaterial;

		[Range(0f, 1f)]
		public float CritterThreshold = 0.25f;

		public float MaxRadiusFromMiddle = -1f;

		public ListOfDecorations Critters = new ListOfDecorations();

		public float NoiseScale = 100f;

		[Range(0f, 1f)]
		public float NoiseThreshold = 0.65f;

		public ListOfPerlinSpriteShape DecorationPerlinSpriteShapePrimary = new ListOfPerlinSpriteShape();

		public ListOfPerlinSpriteShape DecorationPerlinSpriteShapeSecondary = new ListOfPerlinSpriteShape();

		public ListOfDecorations DecorationPiece = new ListOfDecorations();

		public ListOfDecorations DecorationPiece2x2 = new ListOfDecorations();

		public ListOfDecorations DecorationPiece3x3 = new ListOfDecorations();

		public ListOfDecorations DecorationPiece3x3Tall = new ListOfDecorations();

		public ListOfDecorations DecorationPerlinNoiseOffPath = new ListOfDecorations();

		public ListOfDecorations DecorationPerlinNoiseOnPath = new ListOfDecorations();

		public static int GetRandomWeightedIndex(List<int> weights, double Random)
		{
			if (weights == null || weights.Count == 0)
			{
				return -1;
			}
			int num = 0;
			for (int i = 0; i < weights.Count; i++)
			{
				if (weights[i] >= 0)
				{
					num += weights[i];
				}
			}
			float num2 = 0f;
			for (int i = 0; i < weights.Count; i++)
			{
				if (!((float)weights[i] <= 0f))
				{
					num2 += (float)weights[i] / (float)num;
					if ((double)num2 >= Random)
					{
						return i;
					}
				}
			}
			return -1;
		}
	}
}
