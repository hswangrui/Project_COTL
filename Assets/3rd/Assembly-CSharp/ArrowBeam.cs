using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ArrowBeam : MonoBehaviour
{
	[SerializeField]
	private LineRenderer lineRenderer;

	[SerializeField]
	private PolygonCollider2D polygonCollider;

	private static GameObject loadedBeam;

	private Health.Team team = Health.Team.Team2;

	private float duration;

	public static void CreateBeam(Vector3[] positions, float width, float duration, Health.Team team, Transform parent, Action<ArrowBeam> result)
	{
		if (loadedBeam == null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/Weapons/ArrowBeam.prefab");
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				loadedBeam = obj.Result;
				CreateBeam(positions, width, duration, team, parent, result);
			};
			return;
		}
		ArrowBeam component = ObjectPool.Spawn(loadedBeam).GetComponent<ArrowBeam>();
		component.duration = duration;
		component.lineRenderer.positionCount = positions.Length;
		component.lineRenderer.SetPositions(positions);
		component.lineRenderer.widthMultiplier = width;
		Vector2[] array = new Vector2[positions.Length * 2 + 1];
		int num = 0;
		for (int i = 0; i < positions.Length; i++)
		{
			Vector3 vector = ((i < positions.Length - 1) ? (positions[i + 1] - positions[i]).normalized : (positions[i] - positions[i - 1]).normalized);
			Vector3 vector2 = Quaternion.AngleAxis(-90f, -Vector3.forward) * vector * (width / 2f);
			Vector3 vector3 = Quaternion.AngleAxis(90f, -Vector3.forward) * vector * (width / 2f);
			if (i % 2 != 0)
			{
				vector2 *= -1f;
				vector3 *= -1f;
			}
			array[num] = positions[i] + vector2;
			array[num + 1] = positions[i] + vector3;
			num += 2;
		}
		array[array.Length - 1] = array[0];
		component.polygonCollider.points = array;
		Action<ArrowBeam> action = result;
		if (action != null)
		{
			action(component);
		}
	}

	private void Update()
	{
		duration -= Time.deltaTime;
		if (duration < 1f && polygonCollider.enabled)
		{
			polygonCollider.enabled = false;
			lineRenderer.material.DOFade(0f, 1f).OnComplete(delegate
			{
				UnityEngine.Object.Destroy(base.gameObject);
			});
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		Health component = collision.GetComponent<Health>();
		if (component != null && component.team != team)
		{
			float damage = ((team == Health.Team.PlayerTeam) ? PlayerWeapon.GetDamage(3f, DataManager.Instance.CurrentWeaponLevel) : 1f);
			component.DealDamage(damage, base.gameObject, base.gameObject.transform.position);
		}
	}
}
