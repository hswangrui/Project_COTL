using UnityEngine;

public class BarrierEnemy : MonoBehaviour
{
	public GameObject barrierPrefab;

	[HideInInspector]
	public GameObject barrierGameObject;

	public UnitObject barrierPartner;

	public float barrierLength = 1f;

	public float lerpSpeed;

	public bool barrierDestroyed;

	private void Start()
	{
		if (barrierPartner != null)
		{
			Debug.Log("Barrier partner added");
			barrierGameObject = Object.Instantiate(barrierPrefab, base.transform);
			barrierGameObject.name = "BARRIER";
		}
	}

	private void FixedUpdate()
	{
		if (!barrierDestroyed)
		{
			if (barrierPartner == null)
			{
				Debug.Log("Barrier partner was null");
				Object.Destroy(barrierGameObject);
				barrierDestroyed = true;
				return;
			}
			Vector3 position = base.transform.position + (barrierPartner.transform.position - base.transform.position) / 2f;
			float num = Vector3.Distance(base.transform.position, barrierPartner.transform.position);
			barrierGameObject.transform.position = position;
			barrierGameObject.transform.LookAt(barrierPartner.transform.position, Vector3.forward);
			barrierGameObject.transform.Rotate(-90f, 0f, 90f);
			Vector3 localScale = barrierGameObject.transform.localScale;
			localScale.x = num * barrierLength;
			barrierGameObject.transform.localScale = localScale;
		}
	}
}
