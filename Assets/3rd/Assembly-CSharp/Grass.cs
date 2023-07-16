using UnityEngine;

public class Grass : BaseMonoBehaviour
{
	private float rotateSpeedY;

	private float rotateY;

	public GameObject image;

	public float RotationToCamera = -90f;

	private Health health;

	public Sprite[] grassSprites;

	private void Start()
	{
		health = GetComponent<Health>();
		if (health != null)
		{
			health.OnDie += OnDie;
		}
		if (grassSprites.Length != 0)
		{
			int num = Random.Range(0, grassSprites.Length);
			image.GetComponent<SpriteRenderer>().sprite = grassSprites[num];
		}
	}

	private void OnDisable()
	{
		if (health != null)
		{
			health.OnDie -= OnDie;
		}
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (DungeonDecorator.getInsance() != null)
		{
			DungeonDecorator.getInsance().UpdateStructures(NavigateRooms.r, base.transform.position, 0);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
		{
			rotateSpeedY = (10f + (float)Random.Range(-2, 2)) * (float)((!(collision.transform.position.x < base.transform.position.x)) ? 1 : (-1));
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Player" && collision.gameObject != null)
		{
			rotateSpeedY = (10f + (float)Random.Range(-2, 2)) * (float)((!(collision.transform.position.x < base.transform.position.x)) ? 1 : (-1));
		}
	}

	private void Update()
	{
		rotateSpeedY += (0f - rotateY) * 0.1f * GameManager.DeltaTime;
		rotateY += (rotateSpeedY *= 0.8f) * GameManager.DeltaTime;
		if (image != null)
		{
			image.transform.eulerAngles = new Vector3(RotationToCamera, rotateY, 0f);
		}
	}
}
