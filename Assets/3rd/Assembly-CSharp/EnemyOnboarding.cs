using System.Collections;
using MMBiomeGeneration;
using UnityEngine;

public class EnemyOnboarding : BaseMonoBehaviour
{
	private UnitObject[] enemies;

	private Coroutine waitingRoutine;

	private GameObject chef;

	private void OnEnable()
	{
		if (waitingRoutine != null)
		{
			StopCoroutine(waitingRoutine);
		}
		waitingRoutine = StartCoroutine(WaitForTileToLoad());
	}

	private IEnumerator WaitForTileToLoad()
	{
		while (!BiomeGenerator.Instance.CurrentRoom.generateRoom.GeneratedDecorations)
		{
			yield return null;
		}
		waitingRoutine = null;
		OnboardEnemy();
		SpawnShopKeeperChef();
	}

	private void OnboardEnemy()
	{
		if (DataManager.Instance == null || DungeonSandboxManager.Active)
		{
			return;
		}
		UnitObject unitObject = null;
		enemies = GetComponentsInChildren<UnitObject>(true);
		UnitObject[] array = enemies;
		foreach (UnitObject unitObject2 in array)
		{
			if (!DataManager.Instance.HasEncounteredEnemy(unitObject2.name) && (bool)unitObject2.GetComponent<EnemyRequiresOnboarding>() && unitObject2.gameObject.activeSelf)
			{
				unitObject = unitObject2;
			}
		}
		if (!(unitObject != null))
		{
			return;
		}
		Interaction_Chest instance = Interaction_Chest.Instance;
		array = enemies;
		foreach (UnitObject unitObject3 in array)
		{
			if (unitObject3 != unitObject)
			{
				instance.Enemies.Remove(unitObject3.health);
				Object.Destroy(unitObject3.gameObject);
			}
		}
		RaycastHit2D[] array2 = Physics2D.CircleCastAll(instance.transform.position, 2f, Vector2.zero);
		foreach (RaycastHit2D raycastHit2D in array2)
		{
			Health component = raycastHit2D.collider.GetComponent<Health>();
			if (component != null && component.team != Health.Team.PlayerTeam && component.team != Health.Team.Team2 && component != unitObject.health)
			{
				component.DealDamage(2.1474836E+09f, base.gameObject, Vector3.Lerp(base.transform.position, component.transform.position, 0.7f));
			}
		}
		BreakableSpiderNest[] componentsInChildren = GetComponentsInChildren<BreakableSpiderNest>();
		if (componentsInChildren.Length != 0)
		{
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				componentsInChildren[num].GetComponent<Health>().DealDamage(float.MaxValue, base.gameObject, base.transform.position);
			}
		}
		SpiderNest[] componentsInChildren2 = GetComponentsInChildren<SpiderNest>();
		if (componentsInChildren2.Length != 0)
		{
			for (int num2 = componentsInChildren2.Length - 1; num2 >= 0; num2--)
			{
				Object.Destroy(componentsInChildren2[num2].gameObject);
			}
		}
		TrapCharger[] componentsInChildren3 = GetComponentsInChildren<TrapCharger>();
		if (componentsInChildren3.Length != 0)
		{
			for (int num3 = componentsInChildren3.Length - 1; num3 >= 0; num3--)
			{
				Object.Destroy(componentsInChildren3[num3].gameObject);
			}
		}
		TrapSpikes[] componentsInChildren4 = GetComponentsInChildren<TrapSpikes>();
		if (componentsInChildren4.Length != 0)
		{
			for (int num4 = componentsInChildren4.Length - 1; num4 >= 0; num4--)
			{
				Object.Destroy(componentsInChildren4[num4].ParentToDestroy);
			}
		}
		TrapProjectileCross[] componentsInChildren5 = GetComponentsInChildren<TrapProjectileCross>();
		if (componentsInChildren5.Length != 0)
		{
			for (int num5 = componentsInChildren5.Length - 1; num5 >= 0; num5--)
			{
				Object.Destroy(componentsInChildren5[num5].gameObject);
			}
		}
		TrapRockFall[] componentsInChildren6 = GetComponentsInChildren<TrapRockFall>();
		if (componentsInChildren6.Length != 0)
		{
			for (int num6 = componentsInChildren6.Length - 1; num6 >= 0; num6--)
			{
				Object.Destroy(componentsInChildren6[num6].gameObject);
			}
		}
		unitObject.transform.position = Interaction_Chest.Instance.transform.position;
		unitObject.RemoveModifier();
		Health.team2.Clear();
		Health.team2.Add(unitObject.GetComponent<Health>());
		DataManager.Instance.AddEncounteredEnemy(unitObject.name);
	}

	private void SpawnShopKeeperChef()
	{
		bool flag = BiomeGenerator.Instance.CurrentRoom.generateRoom.GetComponentInChildren<DungeonLeaderMechanics>(true) != null;
		if (DataManager.Instance.ShopKeeperChefState != 1 || flag || !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1) || DataManager.Instance.playerDeathsInARow != 0 || !(Random.Range(0f, 1f) < 0.05f))
		{
			return;
		}
		Interaction_Chest instance = Interaction_Chest.Instance;
		enemies = GetComponentsInChildren<UnitObject>(true);
		if (enemies.Length <= 1)
		{
			return;
		}
		int num = 0;
		if (num < enemies.Length)
		{
			Vector3 position = enemies[num].transform.position;
			Transform parent = enemies[num].transform.parent;
			instance.Enemies.Remove(enemies[num].health);
			Object.Destroy(enemies[num].gameObject);
			chef = Object.Instantiate(BiomeConstants.Instance.ShopKeeperChef, position, Quaternion.identity, parent);
			chef.GetComponent<Health>().OnDie += ChefDied;
			instance.AddEnemy(chef.GetComponent<Health>());
			if (RoomLockController.DoorsOpen)
			{
				chef.GetComponentInChildren<Interaction_SimpleConversation>(true).Interactable = true;
			}
			else
			{
				RoomLockController.OnRoomCleared += OnRoomCompleted;
			}
		}
	}

	private void OnRoomCompleted()
	{
		chef.GetComponentInChildren<Interaction_SimpleConversation>(true).Interactable = true;
		RoomLockController.OnRoomCleared -= OnRoomCompleted;
	}

	private void ChefDied(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		DataManager.Instance.ShopKeeperChefState = 2;
	}

	private void OnDisable()
	{
		if (chef != null)
		{
			Object.Destroy(chef.gameObject);
		}
	}
}
