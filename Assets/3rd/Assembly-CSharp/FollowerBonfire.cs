using System.Collections;
using I2.Loc;
using MMBiomeGeneration;
using UnityEngine;

public class FollowerBonfire : BaseMonoBehaviour
{
	public DataManager.Variables VariableOnComplete;

	public Vector3 TriggerArea = Vector3.zero;

	public float TriggerRadius = 5f;

	private GameObject Player;

	public BarricadeLine barricadeLine;

	public EnemyRounds enemyRounds;

	public Interaction interaction;

	public LocalizedString _MyLocalizedString;

	public GameObject BonfireOn;

	public GameObject BonfireOff;

	public GameObject Follower;

	public GameObject Bag;

	public Collider2D CutTheRopeCollider;

	private void OnEnable()
	{
		if (!DataManager.Instance.GetVariable(VariableOnComplete))
		{
			interaction.Interactable = false;
			StartCoroutine(WaitForPlayer());
			interaction.Label = _MyLocalizedString;
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void Start()
	{
		CutTheRopeCollider.enabled = false;
	}

	private IEnumerator WaitForPlayer()
	{
		while ((Player = GameObject.FindGameObjectWithTag("Player")) == null)
		{
			yield return null;
		}
		while (Vector3.Distance(base.transform.position + TriggerArea, Player.transform.position) > TriggerRadius)
		{
			yield return null;
		}
		BiomeGenerator.Instance.CurrentRoom.Active = true;
		barricadeLine.Close();
		BlockingDoor.CloseAll();
		RoomLockController.CloseAll();
		enemyRounds.BeginCombat(false, Close);
	}

	public void ReleaseFollower()
	{
		BonfireOn.SetActive(false);
		Bag.SetActive(false);
		Follower.SetActive(true);
		BlockingDoor.OpenAll();
		DataManager.Instance.SetVariable(DataManager.Variables.ForestRescueWorshipper, true);
	}

	private void Close()
	{
		StartCoroutine(CloseRoutine());
	}

	private IEnumerator CloseRoutine()
	{
		DataManager.Instance.SetVariable(VariableOnComplete, true);
		barricadeLine.Open();
		CutTheRopeCollider.enabled = true;
		interaction.Interactable = true;
		yield return new WaitForSeconds(0.5f);
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + TriggerArea, TriggerRadius, Color.yellow);
	}
}
