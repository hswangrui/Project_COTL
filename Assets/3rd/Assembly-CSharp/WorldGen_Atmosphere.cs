using UnityEngine;

public class WorldGen_Atmosphere : BaseMonoBehaviour
{
	private void Start()
	{
		int num = 0;
		while (++num < 10)
		{
			Object.Instantiate(Resources.Load("Prefabs/Particles/Mist") as GameObject, base.transform.parent, true).transform.position = new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0f);
		}
	}

	private void Update()
	{
	}
}
