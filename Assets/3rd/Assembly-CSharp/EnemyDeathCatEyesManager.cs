using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyDeathCatEyesManager : MonoBehaviour
{
	[Serializable]
	public struct PositionSet
	{
		public Vector3[] Positions;
	}

	public static EnemyDeathCatEyesManager Instance;

	[SerializeField]
	private List<EnemyDeathCatEye> eyes;

	[SerializeField]
	public PositionSet[] threeEyeSets;

	[SerializeField]
	public PositionSet[] twoEyeSets;

	[SerializeField]
	public PositionSet[] oneEyeSets;

	[Space]
	[SerializeField]
	private Vector2 timeBetweenAttacks;

	private int previousPositionSetIndex;

	private bool eyesActive;

	private Coroutine activeRoutine;

	public List<EnemyDeathCatEye> Eyes
	{
		get
		{
			return eyes;
		}
	}

	public bool Active { get; set; } = true;


	private void Awake()
	{
		Instance = this;
		foreach (EnemyDeathCatEye eye in eyes)
		{
			eye.GetComponent<Health>().enabled = false;
			eye.Spine.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		eyesActive = false;
		foreach (EnemyDeathCatEye eye in eyes)
		{
			if (eye.Active)
			{
				eyesActive = true;
				break;
			}
		}
		if (!eyesActive && Active && activeRoutine == null)
		{
			SetPositionsAndAttack();
		}
	}

	public void HideAllEyes(float maxDelay)
	{
		foreach (EnemyDeathCatEye eye in eyes)
		{
			eye.Hide(UnityEngine.Random.Range(0f, maxDelay));
		}
		if (activeRoutine != null)
		{
			StopCoroutine(activeRoutine);
			activeRoutine = null;
		}
	}

	private void SetPositionsAndAttack()
	{
		activeRoutine = StartCoroutine(SetPositionsAndAttackIE());
	}

	private IEnumerator SetPositionsAndAttackIE()
	{
		HideAllEyes(0.5f);
		yield return new WaitForSeconds(0.5f);
		if (eyes.Count <= 0)
		{
			activeRoutine = null;
			yield break;
		}
		yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
		int num = eyes.Count - 1;
		PositionSet[] array = threeEyeSets;
		switch (num)
		{
		case 1:
			array = twoEyeSets;
			break;
		case 0:
			array = oneEyeSets;
			break;
		}
		int num2;
		do
		{
			num2 = UnityEngine.Random.Range(0, array.Length);
		}
		while (num2 == previousPositionSetIndex);
		PositionSet positionSet = array[num2];
		previousPositionSetIndex = num2;
		for (int i = 0; i < num + 1; i++)
		{
			float num3 = Vector3.Distance(eyes[i].transform.position, positionSet.Positions[i]);
			eyes[i].Reposition(positionSet.Positions[i], num3 / 5f);
		}
		while (true)
		{
			bool flag = true;
			foreach (EnemyDeathCatEye eye in eyes)
			{
				if (!eye.Active)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			yield return null;
		}
		activeRoutine = StartCoroutine(Attack());
	}

	private IEnumerator Attack()
	{
		while (true)
		{
			List<EnemyDeathCatEye> list = new List<EnemyDeathCatEye>();
			foreach (EnemyDeathCatEye eye in eyes)
			{
				if (eye.Active)
				{
					list.Add(eye);
				}
			}
			int num = 5;
			int num2 = UnityEngine.Random.Range(0, num);
			float num3 = UnityEngine.Random.Range(0f, 1f);
			float num4 = num3 + UnityEngine.Random.Range(0f, 1f);
			int num5 = ((UnityEngine.Random.value > 0.5f) ? 1 : 0);
			if (list.Count >= 3)
			{
				list = list.OrderBy((EnemyDeathCatEye x) => x.transform.position.x).ToList();
				if (UnityEngine.Random.value < 0.5f)
				{
					list[0].Attack(num2, eyes.Count, (num5 == 0) ? num3 : num4);
					list[2].Attack(num2, eyes.Count, (num5 == 0) ? num3 : num4);
					list[1].Attack((int)Mathf.Repeat((!((float)num2 + UnityEngine.Random.value > 0.5f)) ? 1 : (-1), num), eyes.Count, (num5 == 0) ? num4 : num3);
				}
				else
				{
					list[0].Attack(num2, eyes.Count, UnityEngine.Random.Range(0f, 1f));
					list[1].Attack(num2, eyes.Count, UnityEngine.Random.Range(0f, 1f));
					list[2].Attack(num2, eyes.Count, UnityEngine.Random.Range(0f, 1f));
				}
			}
			else if (list.Count >= 2)
			{
				list[0].Attack(num2, eyes.Count, UnityEngine.Random.Range(0f, 1f));
				list[1].Attack(num2, eyes.Count, UnityEngine.Random.Range(0f, 1f));
			}
			else
			{
				if (list.Count < 1)
				{
					break;
				}
				list[0].Attack(num2, eyes.Count, UnityEngine.Random.Range(0f, 1f));
			}
			while (true)
			{
				bool flag = false;
				foreach (EnemyDeathCatEye eye2 in eyes)
				{
					if (eye2.Attacking && eye2.Active)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					break;
				}
				yield return null;
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.3f, 0.6f));
		}
		activeRoutine = null;
	}

	private void OnDrawGizmosSelected()
	{
		PositionSet[] array = threeEyeSets;
		for (int i = 0; i < array.Length; i++)
		{
			PositionSet positionSet = array[i];
			Gizmos.color = UnityEngine.Random.ColorHSV();
			Vector3[] positions = positionSet.Positions;
			for (int j = 0; j < positions.Length; j++)
			{
				Gizmos.DrawWireSphere(positions[j], 0.3f);
			}
		}
		array = twoEyeSets;
		for (int i = 0; i < array.Length; i++)
		{
			PositionSet positionSet2 = array[i];
			Gizmos.color = UnityEngine.Random.ColorHSV();
			Vector3[] positions = positionSet2.Positions;
			for (int j = 0; j < positions.Length; j++)
			{
				Gizmos.DrawWireSphere(positions[j], 0.3f);
			}
		}
		array = oneEyeSets;
		for (int i = 0; i < array.Length; i++)
		{
			PositionSet positionSet3 = array[i];
			Gizmos.color = UnityEngine.Random.ColorHSV();
			Vector3[] positions = positionSet3.Positions;
			for (int j = 0; j < positions.Length; j++)
			{
				Gizmos.DrawWireSphere(positions[j], 0.3f);
			}
		}
	}
}
