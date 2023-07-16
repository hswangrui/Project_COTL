using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace UnityEngine
{
	[Serializable]
	[CreateAssetMenu]
	public class RuleTile : TileBase
	{
		[Serializable]
		public class TilingRule
		{
			public enum Transform
			{
				Fixed,
				Rotated,
				MirrorX,
				MirrorY
			}

			public enum Neighbor
			{
				DontCare,
				This,
				NotThis
			}

			public enum OutputSprite
			{
				Single,
				Random,
				Animation
			}

			public Neighbor[] m_Neighbors;

			public Sprite[] m_Sprites;

			public float m_AnimationSpeed;

			public float m_PerlinScale;

			public Transform m_RuleTransform;

			public OutputSprite m_Output;

			public Tile.ColliderType m_ColliderType;

			public Transform m_RandomTransform;

			public TilingRule()
			{
				m_Output = OutputSprite.Single;
				m_Neighbors = new Neighbor[8];
				m_Sprites = new Sprite[1];
				m_AnimationSpeed = 1f;
				m_PerlinScale = 0.5f;
				m_ColliderType = Tile.ColliderType.Sprite;
				for (int i = 0; i < m_Neighbors.Length; i++)
				{
					m_Neighbors[i] = Neighbor.DontCare;
				}
			}
		}

		public Sprite m_DefaultSprite;

		public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;

		[HideInInspector]
		public List<TilingRule> m_TilingRules;

		public override void GetTileData(Vector3Int position, ITilemap tileMap, ref TileData tileData)
		{
			tileData.sprite = m_DefaultSprite;
			tileData.colliderType = m_DefaultColliderType;
			tileData.flags = TileFlags.LockTransform;
			tileData.transform = Matrix4x4.identity;
			foreach (TilingRule tilingRule in m_TilingRules)
			{
				Matrix4x4 transform = Matrix4x4.identity;
				if (!RuleMatches(tilingRule, position, tileMap, ref transform))
				{
					continue;
				}
				switch (tilingRule.m_Output)
				{
				case TilingRule.OutputSprite.Single:
				case TilingRule.OutputSprite.Animation:
					tileData.sprite = tilingRule.m_Sprites[0];
					break;
				case TilingRule.OutputSprite.Random:
				{
					int num = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, tilingRule.m_PerlinScale, 100000f) * (float)tilingRule.m_Sprites.Length), 0, tilingRule.m_Sprites.Length - 1);
					tileData.sprite = tilingRule.m_Sprites[num];
					if (tilingRule.m_RandomTransform != 0)
					{
						transform = ApplyRandomTransform(tilingRule.m_RandomTransform, transform, tilingRule.m_PerlinScale, position);
					}
					break;
				}
				}
				tileData.transform = transform;
				tileData.colliderType = tilingRule.m_ColliderType;
				break;
			}
		}

		private static float GetPerlinValue(Vector3Int position, float scale, float offset)
		{
			return Mathf.PerlinNoise(((float)position.x + offset) * scale, ((float)position.y + offset) * scale);
		}

		public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
		{
			foreach (TilingRule tilingRule in m_TilingRules)
			{
				Matrix4x4 transform = Matrix4x4.identity;
				if (RuleMatches(tilingRule, position, tilemap, ref transform) && tilingRule.m_Output == TilingRule.OutputSprite.Animation)
				{
					tileAnimationData.animatedSprites = tilingRule.m_Sprites;
					tileAnimationData.animationSpeed = tilingRule.m_AnimationSpeed;
					return true;
				}
			}
			return false;
		}

		public override void RefreshTile(Vector3Int location, ITilemap tileMap)
		{
			if (m_TilingRules != null && m_TilingRules.Count > 0)
			{
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						base.RefreshTile(location + new Vector3Int(j, i, 0), tileMap);
					}
				}
			}
			else
			{
				base.RefreshTile(location, tileMap);
			}
		}

		public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 transform)
		{
			for (int i = 0; i <= ((rule.m_RuleTransform == TilingRule.Transform.Rotated) ? 270 : 0); i += 90)
			{
				if (RuleMatches(rule, position, tilemap, i))
				{
					transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -i), Vector3.one);
					return true;
				}
			}
			if (rule.m_RuleTransform == TilingRule.Transform.MirrorX && RuleMatches(rule, position, tilemap, true, false))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
				return true;
			}
			if (rule.m_RuleTransform == TilingRule.Transform.MirrorY && RuleMatches(rule, position, tilemap, false, true))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
				return true;
			}
			return false;
		}

		private static Matrix4x4 ApplyRandomTransform(TilingRule.Transform type, Matrix4x4 original, float perlinScale, Vector3Int position)
		{
			float perlinValue = GetPerlinValue(position, perlinScale, 200000f);
			switch (type)
			{
			case TilingRule.Transform.MirrorX:
				return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(((double)perlinValue < 0.5) ? 1f : (-1f), 1f, 1f));
			case TilingRule.Transform.MirrorY:
				return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, ((double)perlinValue < 0.5) ? 1f : (-1f), 1f));
			case TilingRule.Transform.Rotated:
			{
				int num = Mathf.Clamp(Mathf.FloorToInt(perlinValue * 4f), 0, 3) * 90;
				return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -num), Vector3.one);
			}
			default:
				return original;
			}
		}

		public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, int angle)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (j != 0 || i != 0)
					{
						Vector3Int vector3Int = new Vector3Int(j, i, 0);
						Vector3Int rotatedPos = GetRotatedPos(vector3Int, angle);
						int indexOfOffset = GetIndexOfOffset(rotatedPos);
						TileBase tile = tilemap.GetTile(position + vector3Int);
						if ((rule.m_Neighbors[indexOfOffset] == TilingRule.Neighbor.This && tile != this) || (rule.m_Neighbors[indexOfOffset] == TilingRule.Neighbor.NotThis && tile == this))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, bool mirrorX, bool mirrorY)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (j != 0 || i != 0)
					{
						Vector3Int vector3Int = new Vector3Int(j, i, 0);
						Vector3Int mirroredPos = GetMirroredPos(vector3Int, mirrorX, mirrorY);
						int indexOfOffset = GetIndexOfOffset(mirroredPos);
						TileBase tile = tilemap.GetTile(position + vector3Int);
						if ((rule.m_Neighbors[indexOfOffset] == TilingRule.Neighbor.This && tile != this) || (rule.m_Neighbors[indexOfOffset] == TilingRule.Neighbor.NotThis && tile == this))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private int GetIndexOfOffset(Vector3Int offset)
		{
			int num = offset.x + 1 + (-offset.y + 1) * 3;
			if (num >= 4)
			{
				num--;
			}
			return num;
		}

		public Vector3Int GetRotatedPos(Vector3Int original, int rotation)
		{
			switch (rotation)
			{
			case 0:
				return original;
			case 90:
				return new Vector3Int(-original.y, original.x, original.z);
			case 180:
				return new Vector3Int(-original.x, -original.y, original.z);
			case 270:
				return new Vector3Int(original.y, -original.x, original.z);
			default:
				return original;
			}
		}

		public Vector3Int GetMirroredPos(Vector3Int original, bool mirrorX, bool mirrorY)
		{
			return new Vector3Int(original.x * ((!mirrorX) ? 1 : (-1)), original.y * ((!mirrorY) ? 1 : (-1)), original.z);
		}
	}
}
