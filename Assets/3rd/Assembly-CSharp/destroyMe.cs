using UnityEngine;

public class destroyMe : BaseMonoBehaviour
{
	private float timer;

	public float deathtimer = 10f;

	private void OnEnable()
	{
		timer = 0f;
	}

	private void OnDisable()
	{
		timer = 0f;
	}

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer >= deathtimer)
		{
			base.gameObject.Recycle();
		}
	}
}
