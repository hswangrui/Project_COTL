using System.Collections;
using DG.Tweening;
using MMRoomGeneration;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class Bouncer : MonoBehaviour
{
	public AnimationCurve bounceCurve;

	public UnityEvent OnBounceCallback;

	[SerializeField]
	private bool isShrinkingOnRoomComplete = true;

	[SerializeField]
	private float inflictsPoisonAmmount;

	private float lastPoisonTime;

	public SkeletonAnimation Spine;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string idleAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Spine")]
	protected string bounceAnimation;

	private void Start()
	{
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	private void RoomLockController_OnRoomCleared()
	{
		GetComponent<CircleCollider2D>().enabled = false;
		if (isShrinkingOnRoomComplete)
		{
			base.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(delegate
			{
				base.transform.parent.gameObject.SetActive(false);
			});
		}
	}

	private void OnDestroy()
	{
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
	}

	public void bounceUnit(UnitObject unit, Vector3 dir, float power = 20f, float duration = 0.25f)
	{
		AudioManager.Instance.PlayOneShot("event:/material/mushroom_impact", base.gameObject);
		StartCoroutine(UpdateBounceUnit(unit, dir, power, duration));
	}

	private IEnumerator UpdateBounceUnit(UnitObject unit, Vector3 dir, float power, float duration)
	{
		float elapsedTime = 0f;
		dir.Normalize();
		Debug.Log("Spine on dropper " + Spine);
		if (Spine != null)
		{
			Spine.AnimationState.SetAnimation(0, bounceAnimation, false);
			Spine.AnimationState.AddAnimation(0, idleAnimation, true, 0f);
		}
		while (elapsedTime < duration)
		{
			if (SimulationManager.IsPaused)
			{
				yield return null;
			}
			float num = bounceCurve.Evaluate(1f / duration * elapsedTime);
			float num2 = power * Time.deltaTime * num;
			unit.moveVX = dir.x * num2;
			unit.moveVY = dir.y * num2;
			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		UnitObject component = collision.GetComponent<UnitObject>();
		if (!component)
		{
			return;
		}
		bounceUnit(component, component.transform.position - base.transform.position);
		if (inflictsPoisonAmmount > 0f)
		{
			Health component2 = component.GetComponent<Health>();
			if (component2 != null && component2.isPlayer && Time.realtimeSinceStartup - lastPoisonTime > 5f)
			{
				lastPoisonTime = Time.realtimeSinceStartup;
				TrapPoison.CreatePoison(component.transform.position, 3, 1.5f, GenerateRoom.Instance.transform);
			}
		}
		OnBounceCallback.Invoke();
	}
}
