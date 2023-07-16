using UnityEngine;

namespace MMRoomGeneration
{
	public class IslandConnector : BaseMonoBehaviour
	{
		public enum Direction
		{
			North,
			East,
			South,
			West
		}

		public bool Active;

		public Direction MyDirection;

		private IslandPiece _ParentIsland;

		public IslandPiece ParentIsland
		{
			get
			{
				if (_ParentIsland == null)
				{
					_ParentIsland = GetComponentInParent<IslandPiece>();
				}
				return _ParentIsland;
			}
		}

		private void OnEnable()
		{
			GetComponent<SpriteRenderer>().enabled = false;
		}

		public void SetActive()
		{
			Active = true;
			ParentIsland.NorthConnectors.Remove(this);
			ParentIsland.EastConnectors.Remove(this);
			ParentIsland.SouthConnectors.Remove(this);
			ParentIsland.WestConnectors.Remove(this);
		}
	}
}
