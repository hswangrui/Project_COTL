using Map;
using UnityEngine;

namespace Lamb.UI
{
	public class NodeConnection : BaseMonoBehaviour
	{
		public AdventureMapNode From { get; private set; }

		public AdventureMapNode To { get; private set; }

		public MMUILineRenderer LineRenderer { get; private set; }

		public void Configure(AdventureMapNode from, AdventureMapNode to, MMUILineRenderer lineRenderer, Material solidLine, Material dottedLine)
		{
			From = from;
			To = to;
			LineRenderer = lineRenderer;
			if (to.State == NodeStates.Visited && from.State == NodeStates.Visited)
			{
				lineRenderer.Color = UIAdventureMapOverlayController.VisitedColour;
				lineRenderer.material = solidLine;
			}
			else if (to.State == NodeStates.Attainable && from.State == NodeStates.Visited)
			{
				lineRenderer.Color = UIAdventureMapOverlayController.AvailableColour;
				lineRenderer.material = dottedLine;
			}
			else
			{
				lineRenderer.Color = UIAdventureMapOverlayController.LockedColour;
				lineRenderer.material = dottedLine;
			}
		}
	}
}
