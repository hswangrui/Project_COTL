using System.Collections.Generic;

public class BuildPlotPlacer : BaseMonoBehaviour
{
	public static List<BuildPlotPlacer> BuildPlotPlacers = new List<BuildPlotPlacer>();

	private void OnEnable()
	{
		BuildPlotPlacers.Add(this);
	}

	private void OnDisable()
	{
		BuildPlotPlacers.Remove(this);
	}

	private void OnDrawGizmos()
	{
	}
}
