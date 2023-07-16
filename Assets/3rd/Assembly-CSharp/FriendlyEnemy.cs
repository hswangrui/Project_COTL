using UnityEngine;

public class FriendlyEnemy : MonoBehaviour
{
	[SerializeField]
	private EnemySwordsman swordsman;

	[SerializeField]
	private GameObject container;

	private void Update()
	{
		if ((RespawnRoomManager.Instance != null && RespawnRoomManager.Instance.gameObject.activeSelf) || (DeathCatRoomManager.Instance != null && DeathCatRoomManager.Instance.gameObject.activeSelf) || (MysticShopKeeperManager.Instance != null && MysticShopKeeperManager.Instance.gameObject.activeSelf))
		{
			container.gameObject.SetActive(false);
			swordsman.enabled = false;
		}
		else
		{
			container.gameObject.SetActive(true);
			swordsman.enabled = true;
		}
	}
}
