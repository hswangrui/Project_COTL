using System.Collections.Generic;
using UnityEngine;

namespace Map
{
	[CreateAssetMenu]
	public class MapConfig : ScriptableObject
	{
		public bool RegenerateOnGenerateMap = true;

		public bool EnsureMinimumRewards = true;

		public bool RanomExtraRewardLayers = true;

		public List<NodeBlueprint> nodeBlueprints;

		public List<NodeType> MinimumRewards = new List<NodeType>();

		public NodeBlueprint FirstFloorBluePrint;

		public NodeBlueprint SecondFloorBluePrint;

		public NodeBlueprint MiniBossFloorBluePrint;

		public NodeBlueprint TreasureBluePrint;

		public NodeBlueprint LeaderFloorBluePrint;

		public IntMinMax numOfPreBossNodes;

		public IntMinMax numOfStartingNodes;

		public int NumOfCombatNodes;

		public int NumOfBlankNodes;

		public int NumOfMiniBossNodes;

		public float ChanceForBlank = 0.5f;

		[HideInInspector]
		public int MinStone;

		[HideInInspector]
		public int MinWood;

		[HideInInspector]
		public int MinFood;

		[Space]
		public NodeType[] FirstLayerBlacklist = new NodeType[0];

		public NodeType[] LastLayerBlacklist = new NodeType[0];

		private static MapLayer Treasure = new MapLayer
		{
			nodeType = NodeType.Treasure,
			distanceFromPreviousLayer = new FloatMinMax
			{
				min = 1f,
				max = 2f
			},
			nodesApartDistance = 2f,
			randomizePosition = 0.542f,
			randomizeNodes = 1f
		};

		private static MapLayer FirstFloor = new MapLayer
		{
			nodeType = NodeType.FirstFloor,
			distanceFromPreviousLayer = new FloatMinMax
			{
				min = 1f,
				max = 2f
			},
			nodesApartDistance = 2f,
			randomizePosition = 0f,
			randomizeNodes = 0f
		};

		private static MapLayer SecondFloor = new MapLayer
		{
			nodeType = NodeType.DungeonFloor,
			distanceFromPreviousLayer = new FloatMinMax
			{
				min = 1f,
				max = 2f
			},
			nodesApartDistance = 2f,
			randomizePosition = 0.542f,
			randomizeNodes = 0f
		};

		private static MapLayer MiniBoss = new MapLayer
		{
			nodeType = NodeType.MiniBossFloor,
			distanceFromPreviousLayer = new FloatMinMax
			{
				min = 1f,
				max = 2f
			},
			nodesApartDistance = 2f,
			randomizePosition = 0.542f,
			randomizeNodes = 0f
		};

		private static MapLayer Boss = new MapLayer
		{
			nodeType = NodeType.Boss,
			distanceFromPreviousLayer = new FloatMinMax
			{
				min = 1f,
				max = 2f
			},
			nodesApartDistance = 2f,
			randomizePosition = 0.542f,
			randomizeNodes = 0f
		};

		public int DungeonLength = 5;

		public int MaxDungeonLength = 5;

		public List<MapLayer> layers = new List<MapLayer>();

		public int GridWidth
		{
			get
			{
				return Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);
			}
		}

		public int TotalDungeonLength
		{
			get
			{
				return Mathf.Clamp(DungeonLength + GameManager.CurrentDungeonLayer - 1, 1, MaxDungeonLength);
			}
		}

		public static void Clear()
		{
			Treasure.BluePrint = null;
			FirstFloor.BluePrint = null;
			SecondFloor.BluePrint = null;
			MiniBoss.BluePrint = null;
			Boss.BluePrint = null;
		}

		public void ResetLayer()
		{
			Debug.Log("RESETTING LAYER!");
			layers = new List<MapLayer>();
			int num = -1;
			while (++num < TotalDungeonLength)
			{
				if (num == 0)
				{
					FirstFloor.BluePrint = FirstFloorBluePrint;
					layers.Add(FirstFloor);
				}
				else if (num == TotalDungeonLength - 1)
				{
					MiniBoss.BluePrint = MiniBossFloorBluePrint;
					if (DataManager.Instance.DungeonBossFight && (bool)LeaderFloorBluePrint)
					{
						MiniBoss.BluePrint = LeaderFloorBluePrint;
					}
					layers.Add(MiniBoss);
				}
				else
				{
					Treasure.BluePrint = MiniBossFloorBluePrint;
					layers.Add(Treasure);
				}
			}
		}
	}
}
