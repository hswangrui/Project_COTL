using UnityEngine;
using UnityEngine.UI;

public class DamageNegationTimer : MonoBehaviour
{
	[SerializeField]
	private Image wheel;

	public void UpdateCharging(float normalised)
	{
		wheel.fillAmount = normalised;
	}
}
