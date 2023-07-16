using UnityEngine;

public class PoisonTrail : BaseMonoBehaviour
{
	[SerializeField]
	private bool startActive;

	[SerializeField]
	private GameObject poisonPrefab;

	[SerializeField]
	private float distanceBetweenPoint;

	private Vector3 previousPlacedPosition = Vector3.zero;

	private Transform parent;

	public GameObject PoisonPrefab
	{
		get
		{
			return poisonPrefab;
		}
		set
		{
			poisonPrefab = value;
		}
	}

	public Transform Parent
	{
		get
		{
			return parent;
		}
		set
		{
			parent = value;
		}
	}

	private void Awake()
	{
		if (!startActive)
		{
			base.enabled = false;
		}
		parent = base.transform.parent;
	}

	private void OnEnable()
	{
		if (parent == null)
		{
			parent = base.transform.parent;
		}
	}

	private void Update()
	{
		if (previousPlacedPosition == Vector3.zero || Vector3.Distance(base.transform.position, previousPlacedPosition) > distanceBetweenPoint)
		{
			previousPlacedPosition = base.transform.position;
			Object.Instantiate(poisonPrefab, new Vector3(base.transform.position.x, base.transform.position.y, 0f), Quaternion.identity, parent);
		}
	}
}
