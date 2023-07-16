using UnityEngine;

public class Interaction_StructureMenu : Interaction
{
	private void Start()
	{
		Vector3 position = base.transform.position;
		Random.InitState((int)position.x + (int)position.y);
		Vector3 vector = new Vector3(0f, 0f, Random.Range(-0.015f, 0.015f));
		base.transform.position = position + vector;
	}
}
