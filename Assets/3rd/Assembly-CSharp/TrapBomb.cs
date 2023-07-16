using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class TrapBomb : BaseMonoBehaviour
{
	public SpriteRenderer Target;

	public SpriteRenderer TargetWarning;

	public GameObject FuseParticles;

	public GameObject Light;

	public SkeletonAnimation Spine;

	public int ExplosionRange = 4;

	private Health health;

	public SpriteRenderer sprite;

	public SpriteRenderer Shadow;

	private float respawnTimer;

	private bool Detonating;

	private const string SHADER_COLOR_NAME = "_Color";

	public float TriggerRange = 2f;

	public float DetonateTime = 3f;

	private bool Respawning;

	private void Start()
	{
		Target.enabled = false;
		TargetWarning.enabled = false;
		FuseParticles.SetActive(false);
		health = GetComponent<Health>();
		health.OnDie += OnDie;
	}

	private void OnEnable()
	{
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	private void OnDisable()
	{
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
	}

	private void RoomLockController_OnRoomCleared()
	{
		StopAllCoroutines();
		Detonating = true;
		Respawning = false;
		Spine.AnimationState.SetAnimation(0, "hide", false);
		Spine.AnimationState.AddAnimation(0, "hidden", true, 0f);
		sprite.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
		Shadow.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
		Target.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
		TargetWarning.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		Explode(2);
	}

	private void Update()
	{
		if (Detonating)
		{
			return;
		}
		if (Respawning)
		{
			if ((respawnTimer -= Time.deltaTime) < 0f)
			{
				health.enabled = true;
				sprite.enabled = true;
				Shadow.enabled = true;
				Spine.AnimationState.SetAnimation(0, "appear", false);
				Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
				sprite.transform.localScale = Vector3.zero;
				sprite.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
				Shadow.transform.localScale = Vector3.zero;
				Shadow.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
				Respawning = false;
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.gameObject);
			}
		}
		else
		{
			if (Time.frameCount % 10 != 0)
			{
				return;
			}
			foreach (Health item in Health.playerTeam)
			{
				if (MagnitudeFindDistanceBetween(item.transform.position, health.transform.position) < TriggerRange * TriggerRange)
				{
					AudioManager.Instance.PlayOneShot("event:/explosion/bomb_fuse", base.transform.position);
					StartCoroutine(DoDetonating());
					StartCoroutine(FlashCircle());
					break;
				}
			}
		}
	}

	private IEnumerator FlashCircle()
	{
		Spine.AnimationState.SetAnimation(0, "explode", true);
		FuseParticles.SetActive(true);
		Light.SetActive(true);
		SpriteRenderer target = Target;
		SpriteRenderer targetWarning = TargetWarning;
		bool flag = true;
		targetWarning.enabled = true;
		target.enabled = flag;
		Target.transform.localScale = Vector3.zero;
		TargetWarning.transform.localScale = Vector3.zero;
		Target.transform.DOScale(Vector3.one * ExplosionRange * 0.5f, 0.3f).SetEase(Ease.OutBack);
		TargetWarning.transform.DOScale(Vector3.one * ExplosionRange * 0.5f, 0.3f).SetEase(Ease.OutBack);
		Color white = new Color(1f, 1f, 1f, 1f);
		Color color = white;
		float flashTickTimer = 0f;
		while (true)
		{
			Target.transform.Rotate(new Vector3(0f, 0f, 150f) * Time.deltaTime);
			if (flashTickTimer >= 0.12f && Time.timeScale == 1f && BiomeConstants.Instance.IsFlashLightsActive)
			{
				Material material = Target.material;
				Color value;
				color = (value = ((color == white) ? Color.red : white));
				material.SetColor("_Color", value);
				TargetWarning.material.SetColor("_Color", color);
				flashTickTimer = 0f;
			}
			flashTickTimer += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator DoDetonating()
	{
		Detonating = true;
		float Timer = 0f;
		Color white = Color.white;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < DetonateTime))
			{
				break;
			}
			yield return null;
		}
		Explode(ExplosionRange);
		Hide(false);
		Light.SetActive(false);
		Detonating = false;
	}

	private void Hide(bool Respawn)
	{
		Spine.AnimationState.SetAnimation(0, "hide", false);
		Spine.AnimationState.AddAnimation(0, "hidden", true, 0f);
		Respawning = true;
		Target.enabled = false;
		TargetWarning.enabled = false;
		FuseParticles.SetActive(false);
		health.enabled = false;
		sprite.enabled = false;
		Shadow.enabled = false;
		StopAllCoroutines();
		if (Respawn)
		{
			respawnTimer = 10f;
		}
		else
		{
			base.enabled = false;
		}
	}

	private void Explode(int Size)
	{
		Explosion.CreateExplosion(base.transform.position, Health.Team.KillAll, health, Size, 1f, 6f);
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, TriggerRange, Color.yellow);
		Utils.DrawCircleXY(base.transform.position, 2f, Color.red);
	}
}
