using System.Collections.Generic;
using UnityEngine;

public class Fire : BaseMonoBehaviour
{
	public static List<Fire> Fires = new List<Fire>();

	private float Progress;

	private float WorkRequired = 3f;

	private float Scale = 1f;

	private float Delay;

	private void OnEnable()
	{
		Fires.Add(this);
	}

	private void OnDisable()
	{
		Fires.Remove(this);
	}

	private void Update()
	{
		if ((Delay -= Time.deltaTime) < 0f && Progress > 0f)
		{
			DoWork(-0.5f * Time.deltaTime);
		}
	}

	public void DoWork(float WorkDone)
	{
		Progress += WorkDone;
		Scale = 1f - Progress / WorkRequired;
		if (WorkDone > 0f)
		{
			Delay = 1f;
		}
		base.transform.localScale = new Vector3(Scale, Scale, Scale);
		if (Progress <= 0f)
		{
			Progress = 0f;
		}
		if (Progress >= WorkRequired)
		{
			GetComponent<Structure>().RemoveStructure();
			Object.Destroy(base.gameObject);
		}
	}
}
