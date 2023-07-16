using System;
using TMPro;
using UnityEngine;

public class TextFollowGameObject : BaseMonoBehaviour
{
	public GameObject Target;

	public float distance = -1.3f;

	public float angle = -45f;

	public string Text;

	private void Start()
	{
		GetComponent<TextMeshProUGUI>().text = Text;
	}

	private void Update()
	{
		if (!(Target == null))
		{
			Vector3 vector = new Vector3(0f, distance * Mathf.Sin((float)Math.PI / 180f * angle), distance * Mathf.Cos((float)Math.PI / 180f * angle));
			Vector3 position = Target.transform.position + vector;
			position = Camera.main.WorldToScreenPoint(position);
			base.transform.position = position;
		}
	}
}
