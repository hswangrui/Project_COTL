using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PushableDecoration : BaseMonoBehaviour
{
	[SerializeField]
	private bool randomOrientation;

	private const float force = 6f;

	private const float torque = 40f;

	private Rigidbody2D rigidbody;

	private SpriteRenderer spriteRenderer;

	private bool pushed;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		if (randomOrientation)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!pushed && collision.gameObject.tag == "Player")
		{
			pushed = true;
			float num = Vector3.Distance(PlayerFarming.Instance.PreviousPosition, PlayerFarming.Instance.transform.position) * 100f / 15f;
			Vector2 vector = new Vector3(PlayerFarming.Instance.playerController.xDir, PlayerFarming.Instance.playerController.yDir);
			float angle = Utils.GetAngle(Vector2.zero, vector);
			vector += Utils.DegreeToVector2(angle + (float)Random.Range(-20, 20));
			rigidbody.AddForce(vector * 6f * num, ForceMode2D.Impulse);
			rigidbody.AddTorque(40f * num, ForceMode2D.Impulse);
			spriteRenderer.DOFade(0f, 1f).OnComplete(delegate
			{
				base.gameObject.SetActive(false);
			});
		}
	}
}
