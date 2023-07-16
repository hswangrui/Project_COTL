using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;

public class CritterBird : UnitObject
{
	private const int AmountForRelic = 10;

	private float DangerDistance = 2f;

	public CircleCollider2D CircleCollider;

	private Animator animator;

	private float Timer;

	private float TargetAngle;

	private float vz;

	public GameObject Shadow;

	[SerializeField]
	private SkeletonAnimation bird;

	private void Start()
	{
	}

	private new void OnEnable()
	{
		state = GetComponent<StateMachine>();
		if (CircleCollider == null)
		{
			CircleCollider = GetComponent<CircleCollider2D>();
		}
		Timer = UnityEngine.Random.Range(2, 4);
		if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x * -1f, 1f, 1f);
		}
		DangerDistance = 2.5f;
		GetComponent<Health>().OnDie += OnDie;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		GetComponent<Health>().OnDie -= OnDie;
	}

	public void SetFleeing()
	{
		if (state == null)
		{
			state = GetComponent<StateMachine>();
		}
		state.CURRENT_STATE = StateMachine.State.Fleeing;
	}

	public override void Update()
	{
		if (PlayerRelic.TimeFrozen)
		{
			return;
		}
		base.Update();
		if (bird == null || bird.state == null)
		{
			return;
		}
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			LookForDanger();
			if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = UnityEngine.Random.Range(0.5f, 2f);
				state.CURRENT_STATE = StateMachine.State.CustomAction0;
				bird.state.SetAnimation(0, "EAT", true);
			}
			break;
		case StateMachine.State.CustomAction0:
			LookForDanger();
			if ((Timer -= Time.deltaTime) < 0f)
			{
				Timer = UnityEngine.Random.Range(2, 4);
				state.CURRENT_STATE = StateMachine.State.Idle;
				bird.state.SetAnimation(0, "IDLE", true);
				if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
				{
					base.transform.localScale = new Vector3(base.transform.localScale.x * -1f, 1f, 1f);
				}
			}
			break;
		case StateMachine.State.Fleeing:
			vx = 3f * Mathf.Cos(TargetAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			vy = 3f * Mathf.Sin(TargetAngle * ((float)Math.PI / 180f)) * Time.deltaTime;
			vz -= 0.07f * Time.deltaTime;
			base.transform.position = base.transform.position + new Vector3(vx, vy, vz);
			if (Shadow != null)
			{
				Shadow.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
				Shadow.transform.localScale = Shadow.transform.localScale - new Vector3(0.002f, 0.002f, 0f);
				if (Shadow.transform.localScale.x <= 0f)
				{
					base.gameObject.Recycle();
				}
			}
			break;
		}
	}

	private void LookForDanger()
	{
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team == Health.Team.PlayerTeam && Vector2.Distance(base.transform.position, allUnit.gameObject.transform.position) < DangerDistance && allUnit.team != 0 && !allUnit.untouchable)
			{
				TargetAngle = Utils.GetAngle(allUnit.transform.position, base.transform.position);
				bird.state.SetAnimation(0, "FLY", true);
				state.CURRENT_STATE = StateMachine.State.Fleeing;
				AudioManager.Instance.PlayOneShot("event:/birds/bird_fly_away", base.gameObject);
				base.transform.localScale = new Vector3((!(TargetAngle < 90f) || !(TargetAngle > -90f)) ? 1 : (-1), 1f, 1f);
				StartCoroutine(DisableCollider());
			}
		}
	}

	private IEnumerator DisableCollider()
	{
		yield return new WaitForSeconds(0.2f);
		CircleCollider.enabled = false;
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, DangerDistance, Color.white);
	}

	public override void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.OnDie(Attacker, AttackLocation, Victim, AttackType, AttackFlags);
		DataManager.Instance.TotalBirdsCaught++;
		if (DataManager.Instance.TotalBirdsCaught >= 10 && DataManager.Instance.OnboardedRelics && !DataManager.Instance.PlayerFoundRelics.Contains(RelicType.RandomEnemyIntoCritter) && Health.team2.Count <= 0)
		{
			GameObject gameObject = RelicCustomTarget.Create(base.transform.position, base.transform.position - Vector3.forward, 1.5f, RelicType.RandomEnemyIntoCritter, delegate
			{
				GameManager.GetInstance().OnConversationEnd();
			});
			BiomeConstants.Instance.EmitSmokeInteractionVFX(gameObject.transform.position, Vector2.one);
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(gameObject.gameObject, 6f);
		}
	}
}
