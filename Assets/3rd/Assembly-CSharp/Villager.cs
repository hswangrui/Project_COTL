using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class Villager : BaseMonoBehaviour
{
	public Health health;

	public FormationFighter formationFighter;

	private static List<Villager> villagers = new List<Villager>();

	public Rigidbody2D rigidbody2D;

	private StateMachine state;

	public List<BaseMonoBehaviour> BaseMonoBehavioursToDisable = new List<BaseMonoBehaviour>();

	public SkeletonAnimation Spine;

	[TermsPopup("")]
	public string WarningSpeech1;

	[TermsPopup("")]
	public string WarningSpeech2;

	private GameObject Attacker;

	private int AttackCounter;

	private Health EnemyHealth;

	private void Start()
	{
		state = GetComponent<StateMachine>();
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
	}

	private void OnEnable()
	{
		health = GetComponent<Health>();
		villagers.Add(this);
		health.OnHit += OnHit;
		health.OnDie += OnDie;
	}

	private void OnDisable()
	{
		villagers.Remove(this);
		health.OnHit -= OnHit;
		health.OnDie -= OnDie;
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		this.Attacker = Attacker;
		ConversationObject conversationObject = null;
		Villager component = Conversation_Speaker.Speaker1.GetComponent<Villager>();
		component.AttackCounter++;
		switch (component.AttackCounter)
		{
		case 1:
			Debug.Log("WarningSpeech1 " + WarningSpeech1);
			conversationObject = new ConversationObject(new List<ConversationEntry>
			{
				new ConversationEntry(component.gameObject, component.WarningSpeech1)
			}, null, null);
			break;
		case 2:
			conversationObject = new ConversationObject(new List<ConversationEntry>
			{
				new ConversationEntry(component.gameObject, component.WarningSpeech2)
			}, null, SoundAlarm);
			break;
		}
		MMConversation.Play(conversationObject);
		formationFighter.knockBackVX = 0f;
		formationFighter.knockBackVY = 0f;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (Health.team2.Count <= 1)
		{
			AmbientMusicController.StopCombat();
			AudioManager.Instance.SetMusicCombatState(false);
		}
	}

	private void SoundAlarm()
	{
		AmbientMusicController.PlayCombat();
		AudioManager.Instance.SetMusicCombatState();
		VillagersAttack(Attacker);
		foreach (Villager villager in villagers)
		{
			if (villager != this)
			{
				villager.VillagersAttack(Attacker);
			}
		}
	}

	public void VillagersAttack(GameObject Attacker)
	{
		Spine.skeleton.SetSkin("Evil");
		foreach (BaseMonoBehaviour item in BaseMonoBehavioursToDisable)
		{
			item.enabled = false;
		}
		health.OnHit -= OnHit;
		EnemyHealth = PlayerFarming.Instance.GetComponent<Health>();
		StartCoroutine(DoVillagersAttack());
	}

	private IEnumerator DoVillagersAttack()
	{
		yield return new WaitForEndOfFrame();
		formationFighter.enabled = true;
		formationFighter.state.CURRENT_STATE = StateMachine.State.Idle;
		formationFighter.TargetEnemy = EnemyHealth;
	}
}
