using UnityEngine;
using UnityEngine.UI;

public class SampleGradient : BaseMonoBehaviour
{
	public Gradient dayColors;

	public Gradient nightColors;

	public Image sceneryImage;

	public Image playerImage;

	public Image timeOfDay;

	public string MaterialColorName;

	private void Start()
	{
	}

	public void setDayColorFloat(float sampleAt)
	{
		if (sceneryImage != null)
		{
			sceneryImage.material.SetColor(MaterialColorName, Sample(sampleAt));
		}
		if (playerImage != null)
		{
			playerImage.material.SetColor(MaterialColorName, Sample(sampleAt));
		}
		if (timeOfDay != null)
		{
			timeOfDay.color = Sample(sampleAt);
		}
	}

	public Color Sample(float sampleAt)
	{
		return dayColors.Evaluate(sampleAt);
	}

	public Color NightSample(float sampleAt)
	{
		return nightColors.Evaluate(sampleAt);
	}
}
