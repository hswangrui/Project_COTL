using System.Collections.Generic;
using UnityEngine;

public class TrapSpiderWeb : BaseMonoBehaviour
{
	private float Timer;

	private float Scale;

	private float ScaleSpeed;

	private List<PlayerController> playerControllers = new List<PlayerController>();

	private void Start()
	{
		Vector3 position = base.transform.position;
		position.z = 0f;
		base.transform.position = position;
	}

	private void Update()
	{
		foreach (PlayerController playerController in playerControllers)
		{
			playerController.RunSpeed = 1f;
		}
		if ((Timer += Time.deltaTime) > 30f)
		{
			Scale -= 0.5f * Time.deltaTime;
			base.transform.localScale = Vector3.one * Scale;
			if (Scale <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			ScaleSpeed += (1f - Scale) * 0.2f;
			Scale += (ScaleSpeed *= 0.8f);
			base.transform.localScale = Vector3.one * Scale;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerController component = collision.gameObject.GetComponent<PlayerController>();
		if (component != null && !playerControllers.Contains(component))
		{
			playerControllers.Add(component);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		PlayerController component = collision.gameObject.GetComponent<PlayerController>();
		if (playerControllers.Contains(component))
		{
			component.RunSpeed = component.DefaultRunSpeed;
			playerControllers.Remove(component);
		}
	}
}
