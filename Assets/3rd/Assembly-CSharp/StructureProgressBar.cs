using UnityEngine;

public class StructureProgressBar : BaseMonoBehaviour
{
	public Structure structure;

	public Transform ProgressBar;

	public GameObject CandleOn;

	public GameObject CandleOff;

	private float _Y;

	private float Y
	{
		get
		{
			return _Y;
		}
		set
		{
			_Y = value;
			if (_Y >= 1f && CandleOff.activeSelf)
			{
				_Y = 1f;
				CandleOn.SetActive(true);
				CandleOff.SetActive(false);
				structure.ProgressCompleted();
			}
			if (_Y <= 0f && !CandleOff.activeSelf)
			{
				CandleOn.SetActive(false);
				CandleOff.SetActive(true);
			}
		}
	}

	private void Start()
	{
		if (structure.Structure_Info.Progress < structure.Structure_Info.ProgressTarget)
		{
			CandleOn.SetActive(false);
			CandleOff.SetActive(true);
		}
		else
		{
			CandleOn.SetActive(true);
			CandleOff.SetActive(false);
		}
	}

	private void Update()
	{
		if (Y < 1f)
		{
			Y = structure.Structure_Info.Progress / structure.Structure_Info.ProgressTarget;
			ProgressBar.localPosition = new Vector3(0f, -0.8f + Y * 0.8f, 0f);
		}
		if (structure.Structure_Info.Progress <= 0f && Y >= 1f)
		{
			Y = structure.Structure_Info.Progress / structure.Structure_Info.ProgressTarget;
		}
	}
}
