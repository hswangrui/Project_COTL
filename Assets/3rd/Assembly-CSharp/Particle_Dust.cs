using UnityEngine;

public class Particle_Dust : BaseMonoBehaviour
{
	private void Update()
	{
		if (((Vector2)Camera.main.WorldToScreenPoint(base.transform.position)).x < 0f)
		{
			base.transform.position = base.transform.position + Vector3.right * 10f;
		}
	}
}
