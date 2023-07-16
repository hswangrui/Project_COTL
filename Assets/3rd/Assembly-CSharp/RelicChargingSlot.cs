using UnityEngine;
using UnityEngine.UI;

public class RelicChargingSlot : MonoBehaviour
{
	[SerializeField]
	private Image activeIcon;

	public void SetActive(bool active)
	{
		activeIcon.gameObject.SetActive(active);
	}
}
