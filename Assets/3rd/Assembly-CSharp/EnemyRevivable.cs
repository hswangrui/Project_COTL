using UnityEngine;

public class EnemyRevivable : BaseMonoBehaviour
{
	[SerializeField]
	private GameObject enemy;

	public GameObject Enemy
	{
		get
		{
			return enemy;
		}
	}
}
