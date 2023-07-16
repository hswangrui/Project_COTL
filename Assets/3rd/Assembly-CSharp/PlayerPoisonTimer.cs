using UnityEngine;
using UnityEngine.UI;

public class PlayerPoisonTimer : BaseMonoBehaviour
{
	[SerializeField]
	private Canvas worldCanvas;

	[SerializeField]
	private GameObject poisonParent;

	[SerializeField]
	private Image poisonWheel;

	[SerializeField]
	private Gradient colorGradient;

	private HealthPlayer health;

	private void Awake()
	{
		worldCanvas.worldCamera = Camera.main;
		health = GetComponentInParent<HealthPlayer>();
	}

	private void Update()
	{
		poisonParent.SetActive(health.IsPoisoned);
		if (health.IsPoisoned)
		{
			poisonWheel.fillAmount = health.PoisonNormalisedTime;
			poisonWheel.color = colorGradient.Evaluate(health.PoisonNormalisedTime);
		}
	}
}
