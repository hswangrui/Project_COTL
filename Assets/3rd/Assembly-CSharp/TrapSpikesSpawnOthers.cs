using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class TrapSpikesSpawnOthers : BaseMonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private SkeletonAnimation spine;

	private BoxCollider2D boxCollider2D;

	private List<Collider2D> colliders;

	private ContactFilter2D contactFilter2D;

	private Vector2 ScaleX = Vector2.zero;

	private Vector2 ScaleY = Vector2.zero;

	private Health EnemyHealth;

	private string defaultSkinName;

	private Color defaultOverrideColor = Color.white;

	public SkeletonAnimation Spine
	{
		get
		{
			return spine;
		}
	}

	public Color OverrideColor { get; set; } = Color.white;


	private void Awake()
	{
		defaultSkinName = spine.Skeleton.Skin.Name;
		defaultOverrideColor = OverrideColor;
	}

	private void Start()
	{
		boxCollider2D = GetComponent<BoxCollider2D>();
		contactFilter2D = default(ContactFilter2D);
		contactFilter2D.NoFilter();
	}

	private void OnEnable()
	{
		ScaleX = Vector2.zero;
		ScaleY = Vector2.zero;
		spine.gameObject.SetActive(false);
		StartCoroutine(DoAttack());
		StartCoroutine(DoScale());
	}

	private void OnDisable()
	{
		ResetToDefaultSettings();
	}

	private void ResetToDefaultSettings()
	{
		Spine.Skeleton.SetSkin(defaultSkinName);
		OverrideColor = defaultOverrideColor;
	}

	private IEnumerator DoScale()
	{
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 5f)
			{
				ScaleX.y += (1f - ScaleX.x) * 0.3f;
				ScaleX.x += (ScaleX.y *= 0.7f);
				ScaleY.y += (1f - ScaleY.x) * 0.3f;
				ScaleY.x += (ScaleY.y *= 0.7f);
				base.transform.localScale = new Vector3(ScaleX.x, ScaleY.x, 1f);
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator DoAttack()
	{
		AudioManager.Instance.PlayOneShot("event:/boss/deathcat/chain_spawner", base.transform.position);
		spriteRenderer.color = ((OverrideColor != Color.white) ? OverrideColor : Color.white);
		yield return new WaitForSeconds(0.25f);
		spriteRenderer.color = ((OverrideColor != Color.white) ? OverrideColor : Color.yellow);
		yield return new WaitForSeconds(0.25f);
		spine.gameObject.SetActive(true);
		spine.state.SetAnimation(0, spine.AnimationName, false);
		spriteRenderer.color = ((OverrideColor != Color.white) ? OverrideColor : Color.red);
		CameraManager.shakeCamera(0.3f, Random.Range(0, 360));
		colliders = new List<Collider2D>();
		boxCollider2D.OverlapCollider(contactFilter2D, colliders);
		foreach (Collider2D collider in colliders)
		{
			EnemyHealth = collider.GetComponent<Health>();
			if (EnemyHealth != null && EnemyHealth.team != Health.Team.Team2)
			{
				EnemyHealth.DealDamage((EnemyHealth.team == Health.Team.PlayerTeam) ? 1 : 10, base.gameObject, base.transform.position);
			}
		}
		yield return new WaitForSeconds(0.75f);
		spriteRenderer.color = ((OverrideColor != Color.white) ? OverrideColor : Color.white);
		yield return new WaitForSeconds(0.25f);
		base.gameObject.Recycle();
	}
}
