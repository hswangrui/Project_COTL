using UnityEngine;

public class TrapProjectileCross : BaseMonoBehaviour, IProjectileTrap
{
	[SerializeField]
	private ProjectileCross projectileCross;

	private ProjectileCross currentProjectileCross;

	public GameObject trapOn;

	public GameObject trapOff;

	private Health health;

	private bool active;

	private bool isDisarmed;

	private void Awake()
	{
		health = GetComponent<Health>();
	}

	private void OnEnable()
	{
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	private void OnDisable()
	{
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
		active = false;
	}

	private void RoomLockController_OnRoomCleared()
	{
		if (base.gameObject.activeInHierarchy)
		{
			DeactivateProjectiles();
		}
		isDisarmed = true;
	}

	private void Update()
	{
		if (GameManager.RoomActive && !active && !isDisarmed)
		{
			ActivateProjectiles();
		}
	}

	private void ActivateProjectiles()
	{
		active = true;
		trapOn.SetActive(true);
		trapOff.SetActive(false);
		Projectile component = Object.Instantiate(projectileCross, base.transform).GetComponent<Projectile>();
		component.transform.position = base.transform.position;
		component.health = health;
		component.team = Health.Team.Team2;
		ProjectileCross component2 = component.GetComponent<ProjectileCross>();
		component2.InitDelayed();
		currentProjectileCross = component2;
	}

	private void DeactivateProjectiles()
	{
		if (currentProjectileCross != null)
		{
			StartCoroutine(currentProjectileCross.DisableProjectiles());
		}
		trapOn.SetActive(false);
		trapOff.SetActive(true);
	}

	private void OnDrawGizmos()
	{
		Utils.DrawLine(base.transform.position, base.transform.position + Vector3.up * 2.5f, Color.blue);
		Utils.DrawLine(base.transform.position, base.transform.position + Vector3.down * 2.5f, Color.blue);
		Utils.DrawLine(base.transform.position, base.transform.position + Vector3.right * 2.5f, Color.blue);
		Utils.DrawLine(base.transform.position, base.transform.position + Vector3.left * 2.5f, Color.blue);
	}
}
