using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Map
{
	[Serializable]
	public class Node
	{
		public Point point;

		public List<Point> incoming = new List<Point>();

		public List<Point> outgoing = new List<Point>();

		[JsonConverter(typeof(StringEnumConverter))]
		public NodeType nodeType;

		public NodeBlueprint blueprint;

		public Vector2 position;

		public DungeonModifier Modifier;

		public bool Hidden;

		public bool CanBeHidden = true;

		public FollowerLocation DungeonLocation = FollowerLocation.None;

		public Node(NodeType nodeType, NodeBlueprint blueprint, Point point, FollowerLocation location = FollowerLocation.None, DungeonModifier modifier = null)
		{
			this.nodeType = nodeType;
			this.blueprint = blueprint;
			this.point = point;
			Modifier = modifier;
			DungeonLocation = location;
			if (blueprint.ForcedDungeon != FollowerLocation.None)
			{
				DungeonLocation = blueprint.ForcedDungeon;
			}
			if (blueprint.CanBeHidden && CanBeHidden && UnityEngine.Random.Range(0f, 1f) < 0.1f && DataManager.Instance.MinimumRandomRoomsEncountered)
			{
				Hidden = true;
			}
		}

		public void AddIncoming(Point p)
		{
			if (!incoming.Any((Point element) => element.Equals(p)))
			{
				incoming.Add(p);
			}
		}

		public void AddOutgoing(Point p)
		{
			if (!outgoing.Any((Point element) => element.Equals(p)))
			{
				outgoing.Add(p);
			}
		}

		public void RemoveIncoming(Point p)
		{
			incoming.RemoveAll((Point element) => element.Equals(p));
		}

		public void RemoveOutgoing(Point p)
		{
			outgoing.RemoveAll((Point element) => element.Equals(p));
		}

		public bool HasNoConnections()
		{
			if (incoming.Count == 0)
			{
				return outgoing.Count == 0;
			}
			return false;
		}
	}
}
