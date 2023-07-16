using System.Collections.Generic;
using System.Linq;
using MMBiomeGeneration;
using UnityEngine;

namespace Map
{
	public static class MapGenerator
	{
		public static MapConfig config;

		private static List<NodeType> RandomNodes;

		private static List<float> layerDistances;

		private static List<List<Point>> paths;

		private static readonly List<List<Node>> nodes = new List<List<Node>>();

		private static readonly List<Node> RewardNodes = new List<Node>();

		private static int blankNodes = 0;

		private static List<NodeType> nonOverrideNodes = new List<NodeType>
		{
			NodeType.Follower_Beginner,
			NodeType.Follower_Easy,
			NodeType.Follower_Medium,
			NodeType.MarketplaceRelics,
			NodeType.Follower_Hard,
			NodeType.Negative_PreviousMiniboss,
			NodeType.MiniBossFloor,
			NodeType.Boss,
			NodeType.FinalBoss,
			NodeType.MarketPlaceCat,
			NodeType.Special_FindRelic
		};

		public static List<List<Node>> Nodes
		{
			get
			{
				return nodes;
			}
		}

		public static void Clear()
		{
			config = null;
			List<NodeType> randomNodes = RandomNodes;
			if (randomNodes != null)
			{
				randomNodes.Clear();
			}
			List<float> list = layerDistances;
			if (list != null)
			{
				list.Clear();
			}
			List<List<Point>> list2 = paths;
			if (list2 != null)
			{
				list2.Clear();
			}
			List<List<Node>> list3 = nodes;
			if (list3 != null)
			{
				list3.Clear();
			}
			List<Node> rewardNodes = RewardNodes;
			if (rewardNodes != null)
			{
				rewardNodes.Clear();
			}
			blankNodes = 0;
		}

		public static Map GetMap(MapConfig conf)
		{
			if (conf == null)
			{
				Debug.LogWarning("Config was null in MapGenerator.Generate()");
				return null;
			}
			Random.InitState(BiomeGenerator.Instance.Seed);
			if (conf.RegenerateOnGenerateMap)
			{
				conf.ResetLayer();
			}
			config = conf;
			RandomNodes = new List<NodeType>();
			foreach (NodeBlueprint nodeBlueprint in config.nodeBlueprints)
			{
				RandomNodes.Add(nodeBlueprint.nodeType);
			}
			nodes.Clear();
			RewardNodes.Clear();
			GenerateLayerDistances();
			for (int i = 0; i < conf.layers.Count; i++)
			{
				PlaceLayer(i, conf);
			}
			GeneratePaths();
			SetUpConnections();
			RemoveCrossConnections();
			if (DungeonSandboxManager.Active)
			{
				FixBossNodes();
			}
			ReconnectEmptyNodes();
			List<Node> nodesList = (from n in nodes.SelectMany((List<Node> n) => n)
				where n.incoming.Count > 0 || n.outgoing.Count > 0
				select n).ToList();
			if (conf.EnsureMinimumRewards)
			{
				EnsureMinimumRewards(conf, nodesList);
			}
			if (config.NumOfCombatNodes > 0)
			{
				SetCombatNodes();
			}
			SetNodePositions();
			if (DungeonSandboxManager.Active)
			{
				EnsureNodesFixed();
				SetMiniBossNodes();
				EvenWeaponTarotRelicRooms();
			}
			return new Map(conf.name, nodesList, new List<Point>());
		}

		private static void SetMiniBossNodes()
		{
			int num = config.NumOfMiniBossNodes;
			int num2 = 0;
			while (num2++ < 100 && num > 0)
			{
				int index = Random.Range(0, nodes.Count);
				Node node = nodes[index][Random.Range(0, nodes[index].Count)];
				if (node.nodeType == NodeType.DungeonFloor && node.incoming.Where((Point x) => GetNode(x).nodeType == NodeType.MiniBossFloor).FirstOrDefault() == null && node.outgoing.Where((Point x) => GetNode(x).nodeType == NodeType.MiniBossFloor).FirstOrDefault() == null)
				{
					node.nodeType = NodeType.MiniBossFloor;
					node.DungeonLocation = config.layers[index].BluePrint.ForcedDungeon;
					node.blueprint = ((config.layers[index].OtherBluePrints.Length != 0) ? config.layers[index].OtherBluePrints[Random.Range(0, config.layers[index].OtherBluePrints.Length)] : MapManager.Instance.DungeonConfig.MiniBossFloorBluePrint);
					node.Hidden = false;
					node.Modifier = DungeonModifier.GetModifier();
					num--;
				}
			}
		}

		private static void EvenWeaponTarotRelicRooms()
		{
			List<NodeType> list = new List<NodeType>
			{
				NodeType.MarketplaceRelics,
				NodeType.MarketPlaceWeapons,
				NodeType.Special_Healing,
				NodeType.MarketPlaceCat,
				NodeType.Tarot
			};
			List<NodeType> list2 = new List<NodeType>();
			if (PlayerFleeceManager.FleecePreventTarotCards())
			{
				list.Remove(NodeType.Tarot);
			}
			while (list.Count > 0)
			{
				int index = Random.Range(0, list.Count);
				list2.Add(list[index]);
				list.RemoveAt(index);
			}
			List<Node> list3 = new List<Node>();
			for (int num = Nodes.Count - 1; num >= 0; num--)
			{
				for (int num2 = Nodes[num].Count - 1; num2 >= 0; num2--)
				{
					Node node = Nodes[num][num2];
					if (list2.Contains(node.nodeType))
					{
						Debug.Log("Add to list! " + node.nodeType);
						list3.Add(node);
					}
				}
			}
			List<Node> list4 = new List<Node>();
			while (list3.Count > 0)
			{
				int index2 = Random.Range(0, list3.Count);
				list4.Add(list3[index2]);
				list3.RemoveAt(index2);
			}
			int num3 = 0;
			foreach (Node item in list4)
			{
				NodeType Type = list2[num3];
				Debug.Log("Random: ".Colour(Color.green) + " " + num3 + "  " + Type);
				item.nodeType = Type;
				item.blueprint = config.nodeBlueprints.FirstOrDefault((NodeBlueprint B) => B.nodeType == Type);
				num3 = (num3 + 1) % list2.Count;
			}
		}

		private static void EnsureMinimumRewards(MapConfig conf, List<Node> NodesList)
		{
			List<Node> list = new List<Node>(RewardNodes);
			List<NodeType> list2 = new List<NodeType>(conf.MinimumRewards);
			foreach (ObjectivesData objective in DataManager.Instance.Objectives)
			{
				if (objective.Type == Objectives.TYPES.FIND_FOLLOWER && ((Objectives_FindFollower)objective).TargetLocation == BiomeGenerator.Instance.DungeonLocation)
				{
					list2.Add(NodeType.Follower_Easy);
					list2.Add(NodeType.Follower_Medium);
					list2.Add(NodeType.Follower_Hard);
					break;
				}
			}
			foreach (NodeBlueprint nodeBlueprint2 in config.nodeBlueprints)
			{
				if (!nodeBlueprint2.HasEnsuredConditions)
				{
					continue;
				}
				bool flag = true;
				foreach (BiomeGenerator.VariableAndCondition ensuredConditionalVariable in nodeBlueprint2.EnsuredConditionalVariables)
				{
					if (DataManager.Instance.GetVariable(ensuredConditionalVariable.Variable) != ensuredConditionalVariable.Condition)
					{
						flag = false;
					}
				}
				if (flag)
				{
					list2.Add(nodeBlueprint2.nodeType);
				}
			}
			foreach (NodeType minimumReward in list2)
			{
				if (NodesList.Where((Node x) => x.nodeType == minimumReward).FirstOrDefault() != null)
				{
					continue;
				}
				string blueprintName = config.nodeBlueprints.Where((NodeBlueprint b) => b.nodeType == minimumReward).ToList().Random()
					.name;
				NodeBlueprint nodeBlueprint = config.nodeBlueprints.FirstOrDefault((NodeBlueprint n) => n.name == blueprintName);
				if ((nodeBlueprint.nodeType == NodeType.Wood && Inventory.GetItemQuantity(1) > 5) || (nodeBlueprint.nodeType == NodeType.Stone && Inventory.GetItemQuantity(2) >= 5) || (nodeBlueprint.nodeType == NodeType.Food && Inventory.GetFoodAmount() > 10))
				{
					continue;
				}
				if (nodeBlueprint.RequireCondition)
				{
					bool flag2 = true;
					foreach (BiomeGenerator.VariableAndCondition conditionalVariable in nodeBlueprint.ConditionalVariables)
					{
						if (DataManager.Instance.GetVariable(conditionalVariable.Variable) != conditionalVariable.Condition)
						{
							flag2 = false;
						}
					}
					if (!flag2)
					{
						continue;
					}
				}
				int index = Random.Range(0, list.Count);
				int num = 100;
				while (--num > 0 && (list[index].nodeType == NodeType.Wood || list[index].nodeType == NodeType.Food || list[index].nodeType == NodeType.Stone || list[index].nodeType == NodeType.None || !NodesList.Contains(list[index]) || nonOverrideNodes.Contains(list[index].nodeType)))
				{
					index = Random.Range(0, list.Count);
				}
				list[index].nodeType = minimumReward;
				list[index].blueprint = nodeBlueprint;
				list[index].Hidden = false;
				list[index].CanBeHidden = false;
				list.RemoveAt(index);
			}
		}

		private static void GenerateLayerDistances()
		{
			layerDistances = new List<float>();
			foreach (MapLayer layer in config.layers)
			{
				float value = layer.distanceFromPreviousLayer.GetValue();
				layerDistances.Add(value);
			}
		}

		private static float GetDistanceToLayer(int layerIndex)
		{
			if (layerIndex < 0 || layerIndex > layerDistances.Count)
			{
				return 0f;
			}
			return layerDistances.Take(layerIndex + 1).Sum();
		}

		private static void PlaceLayer(int layerIndex, MapConfig mapConfig)
		{
			MapLayer mapLayer = config.layers[layerIndex];
			List<Node> list = new List<Node>();
			float offset = mapLayer.nodesApartDistance * (float)config.GridWidth / 2f;
			int gridWidth = config.GridWidth;
			List<NodeBlueprint> list2 = ((mapLayer.OtherBluePrints != null && mapLayer.OtherBluePrints.Length != 0) ? new List<NodeBlueprint>(mapLayer.OtherBluePrints) : new List<NodeBlueprint>());
			for (int i = 0; i < gridWidth; i++)
			{
				if (list2.Count > 0)
				{
					list2.Remove(mapLayer.BluePrint = list2[Random.Range(0, list2.Count)]);
				}
				bool flag = Random.Range(0f, 1f) < mapLayer.randomizeNodes;
				Node nodeBasedOnLayer = GetNodeBasedOnLayer(mapLayer, mapConfig, list, layerIndex, i, offset, flag);
				if (nodeBasedOnLayer.blueprint.ForcedDungeon == FollowerLocation.None)
				{
					nodeBasedOnLayer.DungeonLocation = mapLayer.BluePrint.ForcedDungeon;
				}
				list.Add(nodeBasedOnLayer);
				if (flag)
				{
					RewardNodes.Add(nodeBasedOnLayer);
				}
			}
			nodes.Add(list);
		}

		private static Node GetNodeBasedOnLayer(MapLayer layer, MapConfig mapConfig, List<Node> nodesOnThisLayer, int layerIndex, int i, float offset, bool randomiseReward)
		{
			AnimationCurve.EaseInOut(0f, 1f, 1f, 0.75f);
			NodeType nodeType = NodeType.None;
			Node node = null;
			if (randomiseReward)
			{
				while (true)
				{
					nodeType = ((Random.Range(0f, 1f) > mapConfig.ChanceForBlank || blankNodes >= mapConfig.NumOfBlankNodes) ? GetRandomNode(mapConfig) : NodeType.None);
					if ((layerIndex != 1 || !mapConfig.FirstLayerBlacklist.Contains(nodeType)) && (layerIndex < mapConfig.layers.Count - 2 || !mapConfig.LastLayerBlacklist.Contains(nodeType)))
					{
						Node node2 = GetNode(new Point(i, layerIndex - 1));
						Node node3 = GetNode(new Point(i, layerIndex + 1));
						if ((nodeType != NodeType.None || node2 == null || node2.nodeType != NodeType.None) && (node3 == null || node3.nodeType != NodeType.None) && (nodesOnThisLayer.FirstOrDefault((Node x) => x.nodeType != NodeType.None) != null || nodeType != NodeType.None))
						{
							break;
						}
					}
				}
				string blueprintName = config.nodeBlueprints.Where((NodeBlueprint b) => b.nodeType == nodeType).ToList().Random()
					.name;
				NodeBlueprint nodeBlueprint = config.nodeBlueprints.FirstOrDefault((NodeBlueprint n) => n.name == blueprintName);
				if (nodeBlueprint.OnlyOne && !DungeonSandboxManager.Active)
				{
					Debug.Log("ONLY ONE!");
					RandomNodes.Remove(nodeType);
				}
				if (nodeType == NodeType.None)
				{
					blankNodes++;
				}
				return new Node(nodeType, nodeBlueprint, new Point(i, layerIndex))
				{
					position = new Vector2(0f - offset + (float)i * layer.nodesApartDistance, GetDistanceToLayer(layerIndex))
				};
			}
			if (nodesOnThisLayer.FirstOrDefault((Node x) => x.nodeType == NodeType.Boss) != null)
			{
				return new Node(NodeType.None, config.nodeBlueprints.FirstOrDefault((NodeBlueprint b) => b.nodeType == NodeType.None), new Point(i, layerIndex))
				{
					position = new Vector2(0f - offset + (float)i * layer.nodesApartDistance, GetDistanceToLayer(layerIndex))
				};
			}
			nodeType = layer.nodeType;
			return new Node(nodeType, layer.BluePrint, new Point(i, layerIndex))
			{
				position = new Vector2(0f - offset + (float)i * layer.nodesApartDistance, GetDistanceToLayer(layerIndex))
			};
		}

		private static void SetNodePositions()
		{
			float num = 0f;
			for (int i = 0; i < nodes.Count; i++)
			{
				List<Node> list = nodes[i];
				MapLayer mapLayer = config.layers[i];
				float num2 = ((i + 1 >= layerDistances.Count) ? 0f : layerDistances[i + 1]);
				float num3 = layerDistances[i];
				foreach (Node item in list)
				{
					float num4 = Random.Range(-0.5f, 0.5f);
					float num5 = Random.Range(-0.75f, 0.75f);
					float num6 = num4 * mapLayer.nodesApartDistance / 2f;
					float y = ((num5 < 0f) ? (num3 * num5 / 2f) : (num2 * num5 / 2f));
					item.position += new Vector2(num6 + num, y) * mapLayer.randomizePosition;
				}
				num = Mathf.Lerp(-1f, 1f, Random.Range(0f, 1f));
			}
		}

		private static void SetUpConnections()
		{
			int num = 0;
			foreach (List<Point> path in paths)
			{
				for (int i = 0; i < path.Count; i++)
				{
					Node node = GetNode(path[i]);
					if (i > 0)
					{
						Node node2 = GetNode(path[i - 1]);
						node2.AddIncoming(node.point);
						node.AddOutgoing(node2.point);
					}
					if (i < path.Count - 1)
					{
						Node node3 = GetNode(path[i + 1]);
						node3.AddOutgoing(node.point);
						node.AddIncoming(node3.point);
					}
				}
				num++;
			}
		}

		private static void EnsureNodesFixed()
		{
			for (int num = Nodes.Count - 1; num >= 0; num--)
			{
				for (int num2 = Nodes[num].Count - 1; num2 >= 0; num2--)
				{
					Node node = Nodes[num][num2];
					if (node.outgoing.Count == 0 && node.nodeType != NodeType.FinalBoss && node.nodeType != NodeType.MiniBossFloor && node.nodeType != NodeType.Boss)
					{
						for (int i = 0; i < node.incoming.Count; i++)
						{
							GetNode(node.incoming[i]).RemoveOutgoing(node.point);
						}
						for (int j = 0; j < node.outgoing.Count; j++)
						{
							GetNode(node.outgoing[j]).RemoveIncoming(node.point);
						}
					}
					for (int num3 = node.incoming.Count - 1; num3 >= 0; num3--)
					{
						if (!GetNode(node.incoming[num3]).outgoing.Contains(node.point))
						{
							node.RemoveIncoming(node.incoming[num3]);
						}
					}
				}
			}
		}

		private static void FixBossNodes()
		{
			foreach (List<Point> path in paths)
			{
				for (int i = 0; i < path.Count; i++)
				{
					Node node = GetNode(path[i]);
					if (config.layers[GetNodeLayer(node)].nodeType != NodeType.Boss && config.layers[GetNodeLayer(node)].nodeType != NodeType.FinalBoss)
					{
						continue;
					}
					int layer = GetNodeLayer(node);
					Node node2 = Nodes[layer].FirstOrDefault((Node n) => n.nodeType == config.layers[layer].nodeType);
					List<Node> list = new List<Node>(Nodes[layer]);
					if (list.Count <= 1)
					{
						continue;
					}
					node2.incoming.Clear();
					node2.outgoing.Clear();
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].nodeType != NodeType.None)
						{
							continue;
						}
						for (int k = 0; k < list[j].incoming.Count; k++)
						{
							GetNode(list[j].incoming[k]).outgoing.Clear();
							GetNode(list[j].incoming[k]).outgoing.Add(node2.point);
							if (!node2.incoming.Contains(list[j].incoming[k]))
							{
								node2.incoming.Add(list[j].incoming[k]);
							}
						}
						for (int l = 0; l < list[j].outgoing.Count; l++)
						{
							GetNode(list[j].outgoing[l]).incoming.Clear();
							GetNode(list[j].outgoing[l]).incoming.Add(node2.point);
							if (!node2.outgoing.Contains(list[j].outgoing[l]))
							{
								node2.outgoing.Add(list[j].outgoing[l]);
							}
						}
						list[j].incoming.Clear();
						list[j].outgoing.Clear();
					}
					if (layer + 1 >= Nodes.Count)
					{
						continue;
					}
					list = new List<Node>(Nodes[layer + 1]);
					for (int m = 0; m < list.Count; m++)
					{
						list[m].incoming.Clear();
						list[m].incoming.Add(node2.point);
						if (!node2.outgoing.Contains(list[m].point))
						{
							node2.outgoing.Add(list[m].point);
						}
					}
				}
			}
		}

		private static void ReconnectEmptyNodes()
		{
			foreach (List<Point> path in paths)
			{
				for (int i = 0; i < path.Count; i++)
				{
					Node node = GetNode(path[i]);
					if (node.nodeType != NodeType.None)
					{
						continue;
					}
					for (int j = 0; j < node.incoming.Count; j++)
					{
						for (int k = 0; k < node.outgoing.Count; k++)
						{
							Node node2 = GetNode(node.incoming[j]);
							Node node3 = GetNode(node.outgoing[k]);
							node2.RemoveOutgoing(node.point);
							node3.RemoveIncoming(node.point);
							if (node2.outgoing.Count == 0 || node3.incoming.Count == 0)
							{
								node2.AddOutgoing(node3.point);
								node3.AddIncoming(node2.point);
							}
						}
					}
					node.incoming.Clear();
					node.outgoing.Clear();
				}
			}
		}

		private static void SetCombatNodes()
		{
			AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.75f);
			int num = GameManager.CurrentDungeonLayer - 1;
			int num2 = Mathf.RoundToInt((float)MapManager.Instance.DungeonConfig.NumOfCombatNodes * ((float)num * animationCurve.Evaluate((float)num / 3f)));
			int num3 = 0;
			for (int i = 0; i < 50; i++)
			{
				for (int j = 0; j < nodes.Count; j++)
				{
					for (int k = 0; k < nodes[j].Count; k++)
					{
						if (num3 >= num2)
						{
							break;
						}
						if (((nodes[j][k].outgoing.Count > 0 && nodes[j][k].incoming.Count > 0 && nodes[j][k].outgoing.Where((Point node) => GetNode(node).nodeType == NodeType.DungeonFloor).FirstOrDefault() == null && nodes[j][k].incoming.Where((Point node) => GetNode(node).nodeType == NodeType.DungeonFloor).FirstOrDefault() == null && j > 1) || DungeonSandboxManager.Active) && Random.value > 0.75f && !nonOverrideNodes.Contains(nodes[j][k].nodeType))
						{
							int nodeLayer = GetNodeLayer(nodes[j][k]);
							if (config.layers[nodeLayer].CanBeReplacedWithCombatNode)
							{
								nodes[j][k].nodeType = NodeType.DungeonFloor;
								nodes[j][k].DungeonLocation = config.layers[nodeLayer].BluePrint.ForcedDungeon;
								nodes[j][k].blueprint = MapManager.Instance.DungeonConfig.SecondFloorBluePrint;
								nodes[j][k].Hidden = false;
								nodes[j][k].Modifier = DungeonModifier.GetModifier();
								num3++;
							}
						}
					}
				}
				if (num3 >= num2)
				{
					break;
				}
			}
		}

		private static void RemoveCrossConnections()
		{
			for (int i = 0; i < config.GridWidth - 1; i++)
			{
				for (int j = 0; j < config.layers.Count - 1; j++)
				{
					Node node = GetNode(new Point(i, j));
					if (node == null || node.HasNoConnections())
					{
						continue;
					}
					Node node2 = GetNode(new Point(i + 1, j));
					if (node2 == null || node2.HasNoConnections())
					{
						continue;
					}
					Node top = GetNode(new Point(i, j + 1));
					if (top == null || top.HasNoConnections())
					{
						continue;
					}
					Node topRight = GetNode(new Point(i + 1, j + 1));
					if (topRight != null && !topRight.HasNoConnections() && node.outgoing.Any((Point element) => element.Equals(topRight.point)) && node2.outgoing.Any((Point element) => element.Equals(top.point)))
					{
						node.AddOutgoing(top.point);
						top.AddIncoming(node.point);
						node2.AddOutgoing(topRight.point);
						topRight.AddIncoming(node2.point);
						float num = Random.Range(0f, 1f);
						if (num < 0.33f)
						{
							node.RemoveOutgoing(topRight.point);
							topRight.RemoveIncoming(node.point);
							node2.RemoveOutgoing(top.point);
							top.RemoveIncoming(node2.point);
						}
						if (num < 0.66f)
						{
							node.RemoveOutgoing(topRight.point);
							topRight.RemoveIncoming(node.point);
						}
						else
						{
							node2.RemoveOutgoing(top.point);
							top.RemoveIncoming(node2.point);
						}
					}
				}
			}
		}

		private static void ResolveHangingNodes()
		{
			for (int num = config.GridWidth - 2; num >= 1; num--)
			{
				for (int num2 = config.layers.Count - 2; num2 >= 1; num2--)
				{
					Node node = GetNode(new Point(num, num2));
					if (node.nodeType != NodeType.None && (node.incoming.Count == 0 || (node.incoming.Count == 1 && GetNode(node.incoming[0]).nodeType == NodeType.None)))
					{
						foreach (Point item in node.outgoing)
						{
							GetNode(item).RemoveIncoming(node.point);
						}
						node.outgoing.Clear();
						node.nodeType = NodeType.None;
					}
					if (node.nodeType != NodeType.None && num2 < config.layers.Count - 1 && (node.outgoing.Count == 0 || (node.outgoing.Count == 1 && GetNode(node.outgoing[0]).nodeType == NodeType.None)))
					{
						foreach (Point item2 in node.incoming)
						{
							GetNode(item2).RemoveOutgoing(node.point);
						}
						node.incoming.Clear();
						node.nodeType = NodeType.None;
					}
				}
			}
			for (int i = 0; i < config.GridWidth - 1; i++)
			{
				for (int j = 1; j < config.layers.Count - 1; j++)
				{
					Node node2 = GetNode(new Point(i, j));
					if (node2.nodeType != NodeType.None && node2.outgoing.Count == 0)
					{
						Node closestNodeOfNextLayer = GetClosestNodeOfNextLayer(node2.point);
						node2.AddOutgoing(closestNodeOfNextLayer.point);
						closestNodeOfNextLayer.AddIncoming(node2.point);
					}
				}
			}
			for (int k = 0; k < config.GridWidth - 1; k++)
			{
				for (int l = 1; l < config.layers.Count - 1; l++)
				{
					Node node3 = GetNode(new Point(k, l));
					if (node3.nodeType != NodeType.None && node3.incoming.Count == 0)
					{
						Node closestNodeOfPreviousLayer = GetClosestNodeOfPreviousLayer(node3.point);
						node3.AddIncoming(closestNodeOfPreviousLayer.point);
						closestNodeOfPreviousLayer.AddOutgoing(node3.point);
					}
				}
			}
		}

		private static Node GetNode(Point p)
		{
			if (p.y >= nodes.Count)
			{
				return null;
			}
			if (p.x >= nodes[p.y].Count)
			{
				return null;
			}
			return nodes[p.y][p.x];
		}

		private static Node GetNode(int y, int x)
		{
			if (y >= 0 && y < nodes.Count - 1 && x >= 0 && x < nodes[y].Count - 1)
			{
				return nodes[y][x];
			}
			return null;
		}

		private static Node GetClosestNodeOfNextLayer(Point p)
		{
			Node node = null;
			foreach (Node item in nodes[p.y + 1])
			{
				if (item.nodeType != NodeType.None && (node == null || Mathf.Abs(item.point.x - p.x) < Mathf.Abs(node.point.x - p.x)))
				{
					node = item;
				}
			}
			return node;
		}

		private static Node GetClosestNodeOfPreviousLayer(Point p)
		{
			Node node = null;
			foreach (Node item in nodes[p.y - 1])
			{
				if (item.nodeType != NodeType.None && (node == null || Mathf.Abs(item.point.x - p.x) < Mathf.Abs(node.point.x - p.x)))
				{
					node = item;
				}
			}
			return node;
		}

		public static List<Node> GetAllFutureNodes(int currentLayer)
		{
			List<Node> list = new List<Node>();
			for (int i = currentLayer; i < nodes.Count; i++)
			{
				list.AddRange(nodes[i]);
			}
			return list;
		}

		private static Point GetFinalNode()
		{
			int y = config.layers.Count - 1;
			if (config.GridWidth % 2 == 1)
			{
				return new Point(config.GridWidth / 2, y);
			}
			if (Random.Range(0, 2) != 0)
			{
				return new Point(config.GridWidth / 2 - 1, y);
			}
			return new Point(config.GridWidth / 2, y);
		}

		private static void GeneratePaths()
		{
			Point finalNode = GetFinalNode();
			paths = new List<List<Point>>();
			int value = config.numOfStartingNodes.GetValue();
			int value2 = config.numOfPreBossNodes.GetValue();
			List<int> list = new List<int>();
			for (int i = 0; i < config.GridWidth; i++)
			{
				list.Add(i);
			}
			list.Shuffle();
			List<Point> list2 = (from x in list.Take(value2)
				select new Point(x, finalNode.y - 1)).ToList();
			int num = 0;
			foreach (Point item in list2)
			{
				List<Point> list3 = Path(item, 0, config.GridWidth);
				if (list3 != null)
				{
					list3.Insert(0, finalNode);
					paths.Add(list3);
				}
				num++;
			}
			while (!PathsLeadToAtLeastNDifferentPoints(paths, value) && num < 100)
			{
				List<Point> list4 = Path(list2[Random.Range(0, list2.Count)], 0, config.GridWidth);
				list4.Insert(0, finalNode);
				paths.Add(list4);
				num++;
			}
		}

		private static bool PathsLeadToAtLeastNDifferentPoints(IEnumerable<List<Point>> paths, int n)
		{
			return paths.Select((List<Point> path) => path[path.Count - 1].x).Distinct().Count() >= n;
		}

		private static List<Point> Path(Point from, int toY, int width, bool firstStepUnconstrained = false)
		{
			if (from.y == toY)
			{
				Debug.LogError("Points are on same layers, return");
				return null;
			}
			int num = ((from.y <= toY) ? 1 : (-1));
			List<Point> list = new List<Point> { from };
			while (list[list.Count - 1].y != toY)
			{
				Point point = list[list.Count - 1];
				List<int> list2 = new List<int>();
				if (point.y == 1)
				{
					list2.Add(config.GridWidth / 2);
				}
				else if (firstStepUnconstrained && point.Equals(from))
				{
					for (int i = 0; i < width; i++)
					{
						list2.Add(i);
					}
				}
				else
				{
					Node node = GetNode(new Point(point.x, point.y - 1));
					if (node != null && node.nodeType == NodeType.MiniBossFloor)
					{
						list2.Add(config.GridWidth / 2);
					}
					else
					{
						list2.Add(point.x);
						if (point.x - 1 >= 0)
						{
							list2.Add(point.x - 1);
						}
						if (point.x + 1 < width)
						{
							list2.Add(point.x + 1);
						}
					}
				}
				Point item = new Point(list2[Random.Range(0, list2.Count)], point.y + num);
				list.Add(item);
			}
			return list;
		}

		public static NodeType GetRandomNode(MapConfig mapConfig)
		{
			int num = 0;
			while (num++ < 100)
			{
				NodeType RandomNode = RandomNodes[Random.Range(0, RandomNodes.Count)];
				NodeBlueprint blueprint = MapManager.GetBlueprint(RandomNode, mapConfig);
				float num2 = blueprint.Probability;
				switch (blueprint.nodeType)
				{
				case NodeType.Wood:
					if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.LOG) >= 250)
					{
						num2 *= 0.5f;
					}
					break;
				case NodeType.Stone:
					if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.STONE) >= 250)
					{
						num2 *= 0.5f;
					}
					break;
				}
				if (Random.Range(0f, 1f) > num2)
				{
					continue;
				}
				if (blueprint.RequireCondition && !DungeonSandboxManager.Active)
				{
					bool flag = true;
					foreach (BiomeGenerator.VariableAndCondition conditionalVariable in blueprint.ConditionalVariables)
					{
						if (DataManager.Instance.GetVariable(conditionalVariable.Variable) != conditionalVariable.Condition)
						{
							flag = false;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
				if ((RandomNode == NodeType.Special_Healing || RandomNode == NodeType.Special_HealthChoice) && PlayerFleeceManager.FleecePreventsHealthPickups())
				{
					Debug.Log("Node type can't be a health type with 'no health' fleece");
				}
				else if (RandomNode != NodeType.Tarot || !PlayerFleeceManager.FleecePreventTarotCards())
				{
					config.nodeBlueprints.FirstOrDefault((NodeBlueprint n) => n.nodeType == RandomNode);
					return RandomNode;
				}
			}
			return RandomNodes[Random.Range(0, RandomNodes.Count)];
		}

		public static Node GetRandomNodeOnLayer(int layer)
		{
			List<Node> list = new List<Node>();
			if (layer >= nodes.Count - 1)
			{
				list.Add(MapManager.Instance.CurrentMap.GetBossNode());
			}
			else
			{
				foreach (Node item in nodes[layer])
				{
					if (item.nodeType != NodeType.None)
					{
						list.Add(item);
					}
				}
			}
			if (list.Count <= 0)
			{
				return GetNode(GetFinalNode());
			}
			return list[Random.Range(0, list.Count)];
		}

		public static Node GetFirstNodeOnLayer(int layer)
		{
			foreach (Node item in nodes[layer])
			{
				if (item.nodeType != NodeType.None && (layer != 0 || item.outgoing.Count > 0))
				{
					return item;
				}
			}
			return null;
		}

		public static List<Node> GetNodesOnLayer(int layer)
		{
			return nodes[layer];
		}

		public static int GetNodeLayer(Node node)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].Contains(node))
				{
					return i;
				}
			}
			return -1;
		}

		public static Node GetRandomWeightedNode(int minLayer, int maxLayer)
		{
			List<KeyValuePair<Node, float>> list = new List<KeyValuePair<Node, float>>();
			for (int i = minLayer; i < maxLayer; i++)
			{
				int num = i;
				float value = 1f - ((float)num / (float)nodes.Count - 1f);
				foreach (Node item in nodes[i])
				{
					list.Add(new KeyValuePair<Node, float>(item, value));
				}
			}
			if (list.Count > 0)
			{
				int num2 = 0;
				while (num2++ < 32)
				{
					KeyValuePair<Node, float> keyValuePair = list[Random.Range(0, list.Count)];
					if (Random.Range(0f, 1f) <= keyValuePair.Value)
					{
						return keyValuePair.Key;
					}
				}
			}
			return null;
		}
	}
}
