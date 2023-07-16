using UnityEngine;

public class EnemyTimeCop : BaseMonoBehaviour
{
	public SpriteRenderer SpawnImage;

	public GameObject Enemy;

	private float Timer;

	private float SpawnTime = 3f;

	private float Scale;

	private float ScaleSpeed;

	private float Pulse;

	private bool init;

	private void Start()
	{
		Enemy.SetActive(false);
		SpawnImage.gameObject.SetActive(true);
		SpawnImage.transform.localScale = Vector3.one * 0.1f;
	}

	private void Update()
	{
		if ((Timer += Time.deltaTime) > SpawnTime)
		{
			if (!init)
			{
				ScaleSpeed = 0.2f;
				init = true;
			}
			ScaleSpeed -= 0.02f;
			Scale += ScaleSpeed;
			SpawnImage.transform.localScale = new Vector3(Scale, Scale, Scale);
			if (Scale <= 0f)
			{
				Health component = Enemy.GetComponent<Health>();
				Explosion.CreateExplosion(base.transform.position, component.team, component, 1f);
				Enemy.SetActive(true);
				Enemy.transform.parent = base.transform.parent;
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			ScaleSpeed += (1f + 0.1f * Mathf.Cos(Pulse += 0.1f) - Scale) * 0.3f;
			Scale += (ScaleSpeed *= 0.8f);
			SpawnImage.transform.localScale = new Vector3(Scale, Scale, Scale);
		}
	}
}
