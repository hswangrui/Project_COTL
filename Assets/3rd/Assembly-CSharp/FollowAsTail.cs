using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class FollowAsTail : BaseMonoBehaviour
{
	public Transform FollowObject;

	public bool hasHealth;

	private List<Collider2D> collider2DList;

	public Collider2D DamageCollider;

	public Health health;

	private Health EnemyHealth;

	private Health mainHealth;

	public float Distance = 0.5f;

	private Vector3 Offset = new Vector3(0f, 0f, 0f);

	private SkeletonAnimation _Spine;

	private float Angle;

	private MeshRenderer meshRenderer;

	private MeshRenderer FollowObjectMeshRenderer;

	private SimpleSpineFlash simpleSpineFlash;

	private bool isTransformParentChanged;

	public bool Gravity;

	public float GravitySpeed = 0.3f;

	public float Grav;

	private SkeletonAnimation Spine
	{
		get
		{
			if (_Spine == null)
			{
				_Spine = GetComponent<SkeletonAnimation>();
			}
			return _Spine;
		}
	}

	private void OnEnable()
	{
		if (!isTransformParentChanged)
		{
			StartCoroutine(AssignToCorrectParent());
		}
	}

	private void Start()
	{
		FollowObjectMeshRenderer = FollowObject.gameObject.GetComponent<MeshRenderer>();
		if (hasHealth)
		{
			DamageCollider = GetComponent<Collider2D>();
			health = GetComponent<Health>();
		}
		meshRenderer = GetComponent<MeshRenderer>();
		simpleSpineFlash = GetComponent<SimpleSpineFlash>();
		if ((bool)GetComponentInParent<UnitObject>() && (bool)simpleSpineFlash)
		{
			mainHealth = GetComponentInParent<UnitObject>().GetComponent<Health>();
			if ((bool)mainHealth)
			{
				mainHealth.OnPoisoned += OnPoisoned;
				mainHealth.OnIced += OnIced;
				mainHealth.OnFreezeTime += OnFreezeTime;
				mainHealth.OnCharmed += OnCharmed;
				mainHealth.OnStasisCleared += OnStasisCleared;
			}
		}
	}

	private IEnumerator AssignToCorrectParent()
	{
		yield return new WaitForEndOfFrame();
		base.transform.parent = base.transform.parent.parent;
		isTransformParentChanged = true;
	}

	public void ForcePosition(Vector3 Direction)
	{
		if ((bool)FollowObject)
		{
			base.transform.position = FollowObject.position + Direction.normalized * Distance;
		}
	}

	public void UpdatePosition()
	{
		if (Vector3.Distance(base.transform.position, FollowObject.position) > Distance)
		{
			Vector3 vector = base.transform.position - FollowObject.position;
			base.transform.position = FollowObject.position + vector.normalized * Distance;
			if (Spine != null)
			{
				Spine.skeleton.ScaleX = ((Angle * 57.29578f < 90f && Angle * 57.29578f > -90f) ? 1 : (-1));
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)mainHealth)
		{
			mainHealth.OnPoisoned += OnPoisoned;
			mainHealth.OnIced += OnIced;
			mainHealth.OnCharmed += OnCharmed;
			mainHealth.OnStasisCleared += OnStasisCleared;
		}
	}

	private void Update()
	{
		if (FollowObject == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (meshRenderer != null)
		{
			if (!FollowObjectMeshRenderer.enabled)
			{
				meshRenderer.enabled = false;
				return;
			}
			if (FollowObject.transform.parent != null && FollowObject.transform.parent.GetComponent<UnitObject>() != null)
			{
				meshRenderer.enabled = FollowObject.gameObject.activeInHierarchy;
			}
			else if ((bool)FollowObject.GetComponent<MeshRenderer>())
			{
				meshRenderer.enabled = FollowObject.GetComponent<MeshRenderer>().enabled;
			}
		}
		if (Vector3.Distance(base.transform.position, FollowObject.position) >= Distance)
		{
			UpdatePosition();
		}
		if (Gravity && FollowObject.position.z >= 0f)
		{
			if (base.transform.position.z < 0f)
			{
				Grav += (0f - GravitySpeed) * Time.deltaTime;
				base.transform.position += Vector3.back * Grav;
			}
			else
			{
				Grav = 0f;
				base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
			}
		}
		if (!hasHealth)
		{
			return;
		}
		collider2DList = new List<Collider2D>();
		DamageCollider.GetContacts(collider2DList);
		Debug.Log(DamageCollider.GetContacts(collider2DList));
		foreach (Collider2D collider2D in collider2DList)
		{
			EnemyHealth = collider2D.gameObject.GetComponent<Health>();
			if (EnemyHealth != null && EnemyHealth.team != health.team)
			{
				Debug.Log("DAMAGE");
				EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
			}
		}
	}

	private void OnPoisoned()
	{
		simpleSpineFlash.Tint(Color.green);
	}

	private void OnIced()
	{
		Spine.timeScale = 0.25f;
		simpleSpineFlash.Tint(Color.cyan);
	}

	private void OnFreezeTime()
	{
		Spine.timeScale = 0f;
	}

	private void OnCharmed()
	{
		AudioManager.Instance.PlayOneShot("event:/player/Curses/enemy_charmed", base.gameObject);
		simpleSpineFlash.Tint(Color.red);
	}

	private void OnStasisCleared()
	{
		Spine.timeScale = 1f;
		simpleSpineFlash.Tint(Color.white);
	}
}
