using UnityEngine;

public class Mist : BaseMonoBehaviour
{
	private int dir = 1;

	private void Start()
	{
		dir = ((!((double)Random.Range(0f, 1f) <= 0.5)) ? 1 : (-1));
	}

	private void Update()
	{
		base.transform.Translate(new Vector3(0.2f * (float)dir * Time.deltaTime, 0f, 0f));
	}
}
