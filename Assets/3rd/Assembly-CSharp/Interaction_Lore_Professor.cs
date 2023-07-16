using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class Interaction_Lore_Professor : Interaction
{
	[Serializable]
	public class EnemyAndPosition
	{
		public GameObject Enemy;

		public Vector3 Position;

		public float Delay;

		public EnemyAndPosition(GameObject Enemy, Vector3 Position, float Delay)
		{
			this.Enemy = Enemy;
			this.Position = Position;
			this.Delay = Delay;
		}
	}

	private string sTalk;

	private bool spoken;

	public AudioClip PreCombatMusic;

	public AudioClip CombatMusic;

	public Vector3 ListenPosition;

	public Interaction_SimpleConversation Conversation;

	public SkeletonAnimation Spine;

	private List<List<EnemyAndPosition>> Rounds = new List<List<EnemyAndPosition>>();

	public List<EnemyAndPosition> Round1 = new List<EnemyAndPosition>();

	public List<EnemyAndPosition> Round2 = new List<EnemyAndPosition>();

	private int DeathCount;

	private void Start()
	{
		UpdateLocalisation();
		base.Label = sTalk;
		ActivateDistance = 2f;
		Rounds.Add(Round1);
		Rounds.Add(Round2);
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sTalk = ScriptLocalization.Interactions.Talk;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!spoken)
		{
			Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
			Conversation.Play();
			spoken = true;
			base.Label = "";
			PlayerFarming component = state.GetComponent<PlayerFarming>();
			GameObject gameObject = new GameObject();
			gameObject.transform.position = base.transform.position + ListenPosition;
			component.GoToAndStop(gameObject, base.gameObject);
		}
	}

	private void TellMeMore()
	{
	}

	private void EndConversation()
	{
	}

	public void BeginCombat()
	{
		StartCoroutine(DoBeginCombat());
	}

	private void OnSpawnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		Victim.OnDie -= OnSpawnDie;
		DeathCount++;
	}

	private IEnumerator DoBeginCombat()
	{
		yield return new WaitForEndOfFrame();
		RoomManager.Instance.BlockDoors();
		GameManager.GetInstance().OnConversationNew(true, true);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		Spine.AnimationState.SetAnimation(0, "lute-start", false);
		Spine.AnimationState.AddAnimation(0, "lute-loop", true, 0f);
		yield return new WaitForSeconds(1.5f);
		AmbientMusicController.PlayCombat(PreCombatMusic, CombatMusic);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		int CurrentRound = -1;
		while (true)
		{
			int num = CurrentRound + 1;
			CurrentRound = num;
			if (num >= Rounds.Count)
			{
				break;
			}
			DeathCount = 0;
			foreach (EnemyAndPosition item in Rounds[CurrentRound])
			{
				GameObject obj = UnityEngine.Object.Instantiate(item.Enemy, base.transform.parent);
				obj.transform.position = base.transform.position + item.Position;
				obj.GetComponent<Health>().OnDie += OnSpawnDie;
				GameObject obj2 = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Enemy Spawner/EnemySpawner")) as GameObject;
				obj2.transform.position = base.transform.position + item.Position;
				obj2.GetComponent<EnemySpawner>();
				yield return new WaitForSeconds(item.Delay);
			}
			while (DeathCount < Rounds[CurrentRound].Count)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(1f);
		Spine.AnimationState.SetAnimation(0, "lute-stop", true);
		AmbientMusicController.StopCombat();
		AudioManager.Instance.SetMusicCombatState(false);
		yield return new WaitForSeconds(1f);
		Spine.skeleton.ScaleX = -1f;
		Spine.AnimationState.SetAnimation(0, "teleport-out", false);
		yield return new WaitForSeconds(1.15f);
		RoomManager.Instance.UnbockDoors();
		DataManager.Instance.SetVariable(DataManager.Variables.Goat_First_Meeting, true);
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 2, base.transform.position + Vector3.back);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private new void OnDrawGizmos()
	{
		foreach (EnemyAndPosition item in Round1)
		{
			if (item != null)
			{
				Utils.DrawCircleXY(base.transform.position + item.Position, 0.2f, Color.yellow);
			}
		}
		foreach (EnemyAndPosition item2 in Round2)
		{
			if (item2 != null)
			{
				Utils.DrawCircleXY(base.transform.position + item2.Position, 0.2f, new Color(1f, 0.64f, 0f));
			}
		}
		Utils.DrawCircleXY(base.transform.position + ListenPosition, 0.4f, Color.blue);
	}
}
