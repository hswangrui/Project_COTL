using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

namespace Map
{
	[CreateAssetMenu]
	public class NodeBlueprint : ScriptableObject
	{
		public Sprite sprite;

		public Sprite spriteNoDecoration;

		public Sprite flair;

		public NodeType nodeType;

		public string[] RoomPrefabs;

		[Space]
		[Range(0f, 1f)]
		public float Probability = 1f;

		[Space]
		public bool CanHaveModifier;

		public bool CanBeHidden;

		public bool OnlyOne;

		public bool RequireCondition;

		public List<BiomeGenerator.VariableAndCondition> ConditionalVariables = new List<BiomeGenerator.VariableAndCondition>();

		public bool HasEnsuredConditions;

		public List<BiomeGenerator.VariableAndCondition> EnsuredConditionalVariables = new List<BiomeGenerator.VariableAndCondition>();

		[Space]
		public bool ForceDisplayModifier;

		public Sprite ForceDisplayModifierIcon;

		[Space]
		public bool UseCustomLayerIcons;

		public LayerSprite[] LayerSprites;

		[Space]
		public FollowerLocation ForcedDungeon = FollowerLocation.None;

		public bool Testing;

		public Sprite GetSprite(FollowerLocation dungeonLocation, bool decoration = true)
		{
			if (ForcedDungeon != FollowerLocation.None)
			{
				dungeonLocation = ForcedDungeon;
			}
			if (UseCustomLayerIcons && dungeonLocation != FollowerLocation.None)
			{
				LayerSprite[] layerSprites = LayerSprites;
				for (int i = 0; i < layerSprites.Length; i++)
				{
					LayerSprite layerSprite = layerSprites[i];
					if (layerSprite.DungeonLocation == dungeonLocation)
					{
						if (decoration)
						{
							return layerSprite.Sprite;
						}
						return layerSprite.SpriteNoDecoration;
					}
				}
			}
			if (decoration)
			{
				return sprite;
			}
			return spriteNoDecoration;
		}
	}
}
