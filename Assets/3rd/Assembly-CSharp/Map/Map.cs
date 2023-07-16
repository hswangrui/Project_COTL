using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
	public class Map
	{
		public List<Node> nodes;

		public List<Point> path;

		public string configName;

		public Map(string configName, List<Node> nodes, List<Point> path)
		{
			this.configName = configName;
			this.nodes = nodes;
			this.path = path;
		}

		public Node GetBossNode()
		{
			return nodes.LastOrDefault((Node n) => n.nodeType == NodeType.MiniBossFloor);
		}

		public Node GetLeaderNode()
		{
			return nodes.LastOrDefault((Node n) => n.nodeType == NodeType.Boss);
		}

		public Node GetFinalBossNode()
		{
			Node node = nodes.LastOrDefault((Node n) => n.nodeType == NodeType.FinalBoss);
			if (node != null)
			{
				return node;
			}
			node = nodes.LastOrDefault((Node n) => n.nodeType == NodeType.Boss);
			if (node != null)
			{
				return node;
			}
			return nodes.LastOrDefault((Node n) => n.nodeType == NodeType.MiniBossFloor);
		}

		public Node GetCurrentNode()
		{
			if (path.Count > 0)
			{
				return GetNode(path.LastElement());
			}
			return null;
		}

		public float DistanceBetweenFirstAndLastLayers()
		{
			Node bossNode = GetBossNode();
			Node node = nodes.FirstOrDefault((Node n) => n.point.y == 0);
			if (bossNode == null || node == null)
			{
				return 0f;
			}
			return bossNode.position.y - node.position.y;
		}

		public Node GetFirstNode()
		{
			return nodes.First((Node n) => n.point.y.Equals(0));
		}

		public Node GetNode(Point point)
		{
			return nodes.FirstOrDefault((Node n) => n.point.Equals(point));
		}

		public Node GetNextNode(Point currentPoint)
		{
			Node node = GetNode(currentPoint);
			if (node.outgoing.Count > 0)
			{
				return GetNode(node.outgoing[Random.Range(0, node.outgoing.Count)]);
			}
			return node;
		}
	}
}
