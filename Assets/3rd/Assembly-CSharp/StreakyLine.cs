using UnityEngine;

public class StreakyLine : MonoBehaviour
{
	public Transform scaler;

	public Transform shifter;

	private float shiftLegnth = 2.5f;

	private float shiftSpeed = 25f;

	private float shiftPos;

	private float scaleLength = 0.5f;

	private float scaleSpeed = 80f;

	private float scalePos;

	private void ScalerAndShifter()
	{
		scaler = base.gameObject.transform.GetChild(0).transform;
		shifter = base.gameObject.transform.GetChild(0).GetChild(0).transform;
	}

	private void Start()
	{
		if (scaler == null)
		{
			shiftPos = Random.Range(0, 360);
		}
		scalePos = Random.Range(0, 360);
		shiftSpeed = Random.Range(20, 50);
		scaleSpeed = Random.Range(50, 80);
	}

	private void Update()
	{
		if (shiftPos < 360f)
		{
			shiftPos += shiftSpeed * Time.deltaTime;
		}
		else
		{
			shiftPos -= 360f;
		}
		if (scalePos < 360f)
		{
			scalePos += scaleSpeed * Time.deltaTime;
		}
		else
		{
			scalePos -= 360f;
		}
		shifter.localPosition = new Vector3(1f + Mathf.Cos(shiftPos) * shiftLegnth, 0f, 0f);
		scaler.localScale = new Vector3(1f, 1f + Mathf.Sin(scalePos) * (scaleLength * 2f), 1f);
	}
}
