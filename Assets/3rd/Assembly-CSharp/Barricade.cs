using System.Collections.Generic;

public class Barricade : BaseMonoBehaviour
{
	public static List<Barricade> barricades = new List<Barricade>();

	public bool occupied;

	private void Start()
	{
		barricades.Add(this);
	}

	private void OnDestroy()
	{
		barricades.Remove(this);
	}
}
