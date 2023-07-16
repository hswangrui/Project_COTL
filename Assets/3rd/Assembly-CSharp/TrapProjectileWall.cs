using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TrapProjectileWall : MonoBehaviour, IProjectileTrap
{
	[SerializeField]
	private Health health;

	[SerializeField]
	private GameObject EndPillar;

	[SerializeField]
	private List<GameObject> PillarsOn = new List<GameObject>();

	[SerializeField]
	private List<GameObject> PillarsOff = new List<GameObject>();

	[SerializeField]
	private GameObject Projectile;

	private bool active;

	private bool roomCleared;

	[SerializeField]
	private List<Projectile> Projectiles = new List<Projectile>();

	public int Count = 5;

	public float Distance = 0.5f;

	public Vector2 Direction = Vector2.one;

	private Vector3 EndPosition
	{
		get
		{
			return (Vector3)Direction * ((float)(Count + 1) * Distance);
		}
	}

	private void Start()
	{
		foreach (GameObject item in PillarsOn)
		{
			item.SetActive(false);
		}
		foreach (GameObject item2 in PillarsOff)
		{
			item2.SetActive(true);
		}
	}

	private void OnEnable()
	{
		RoomLockController.OnRoomCleared += DeactivateProjectiles;
		PlaceEndPillar();
	}

	private void OnDisable()
	{
		RoomLockController.OnRoomCleared -= DeactivateProjectiles;
		active = false;
	}

	private void DeactivateProjectiles()
	{
		roomCleared = true;
		foreach (GameObject item in PillarsOn)
		{
			item.SetActive(false);
		}
		foreach (GameObject item2 in PillarsOff)
		{
			item2.SetActive(true);
		}
		for (int num = Projectiles.Count - 1; num >= 0; num--)
		{
			if (Projectiles[num] != null)
			{
				Projectiles[num].DestroyProjectile(true);
			}
		}
		Projectiles.Clear();
	}

	private void Update()
	{
		if (GameManager.RoomActive && !active && !roomCleared)
		{
			ActivateProjectiles();
		}
	}

	private void ActivateProjectiles()
	{
		active = true;
		PlaceEndPillar();
		foreach (GameObject item in PillarsOn)
		{
			item.SetActive(true);
		}
		foreach (GameObject item2 in PillarsOff)
		{
			item2.SetActive(false);
		}
		StartCoroutine(ICreateProjectiles());
	}

	private void PlaceEndPillar()
	{
		EndPillar.transform.position = base.transform.position + EndPosition;
	}

	private void ResetEndPillar()
	{
		EndPillar.transform.position = base.transform.position;
	}

	private IEnumerator ICreateProjectiles()
	{
		yield return new WaitForSeconds(0.5f);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < Count - 1)
			{
				Projectile component = Object.Instantiate(Projectile).GetComponent<Projectile>();
				component.transform.position = base.transform.position + BulletPosition(i);
				component.transform.parent = base.transform.parent;
				component.transform.localScale = Vector3.zero;
				component.gameObject.SetActive(true);
				component.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
				component.health = health;
				component.team = Health.Team.Team2;
				Projectiles.Add(component);
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			break;
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, 0.5f, Color.red);
		for (int i = 0; i < Count; i++)
		{
			Utils.DrawCircleXY(base.transform.position + BulletPosition(i), 0.25f, Color.green);
		}
		Utils.DrawCircleXY(base.transform.position + EndPosition, 0.5f, Color.red);
	}

	private Vector3 BulletPosition(int i)
	{
		return (Vector3)Direction * (float)(i + 1) * Distance;
	}
}
