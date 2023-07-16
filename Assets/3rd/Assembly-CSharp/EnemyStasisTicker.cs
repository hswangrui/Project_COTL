using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyStasisTicker : BaseMonoBehaviour
{
	private Health enemy;

	private bool active;

	private Vector2 offset;

	private float startFrame = -1f;

	public static void Instantiate(Health enemy, Vector2 offset, Health.AttackTypes attackType, Action<EnemyStasisTicker> result)
	{
		string key = "";
		switch (attackType)
		{
		case Health.AttackTypes.Poison:
			key = "Assets/Prefabs/Enemies/Misc/Enemy Poison Ticker.prefab";
			break;
		case Health.AttackTypes.Ice:
			key = "Assets/Prefabs/Enemies/Misc/Enemy Ice Ticker.prefab";
			break;
		case Health.AttackTypes.Charm:
			key = "Assets/Prefabs/Enemies/Misc/Enemy Charm Ticker.prefab";
			break;
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(key);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			EnemyStasisTicker component = obj.Result.GetComponent<EnemyStasisTicker>();
			component.enemy = enemy;
			component.offset = offset;
			component.startFrame = Time.time;
			component.transform.parent = enemy.transform.parent;
			Action<EnemyStasisTicker> action = result;
			if (action != null)
			{
				action(component);
			}
		};
	}

	private void OnEnable()
	{
		Show();
	}

	public void Show()
	{
		active = true;
		base.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f).OnComplete(delegate
		{
			base.transform.localScale = Vector3.one;
		});
	}

	public void Hide(bool destroyAfter = true)
	{
		active = false;
		base.transform.DOScale(Vector3.zero, 0.1f).OnComplete(delegate
		{
			if (destroyAfter)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		});
	}

	private void LateUpdate()
	{
		if (enemy != null && enemy.HP > 0f && !enemy.invincible && enemy.enabled)
		{
			if (!active)
			{
				Show();
			}
			base.transform.position = enemy.transform.position - new Vector3(offset.x, -0.5f, offset.y);
			startFrame = Time.time;
		}
		else if (active && startFrame != -1f && Time.time != startFrame)
		{
			Hide(enemy == null || enemy.HP <= 0f);
		}
		else if (enemy == null && startFrame != -1f && Time.time != startFrame)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
