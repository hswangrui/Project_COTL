using System;
using FMODUnity;
using UnityEngine;

public class DeadBodyFlying : BaseMonoBehaviour
{
	public Transform ObjectToRotate;

	private Vector2 DirectionalSpeed;

	private float Speed = 5f;

	private bool RightFacing;

	private Rigidbody2D rb2D;

	private float velocity;

	[EventRef]
	[SerializeField]
	private string deathSound;

	private float Z = -0.1f;

	private float ZSpeed = -10f;

	private Vector2 ScaleX = new Vector2(2f, 0f);

	private Vector2 ScaleY = new Vector2(2f, 0f);

	private float DestroyTimer;

	private void Start()
	{
		rb2D = GetComponent<Rigidbody2D>();
	}

	public void Init(float Angle)
	{
		DirectionalSpeed = new Vector2(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f)));
		if (DirectionalSpeed.x > 0f)
		{
			RightFacing = true;
		}
		else
		{
			RightFacing = false;
		}
		if (RightFacing)
		{
			ObjectToRotate.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			ObjectToRotate.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private void Update()
	{
		ObjectToRotate.transform.parent.Rotate(new Vector3(0f, Speed * (float)(RightFacing ? (-2) : 2) * Time.timeScale, 0f));
		BounceMe();
		Speed = Mathf.SmoothDamp(Speed, 0f, ref velocity, 0.5f);
		if ((DestroyTimer += Time.deltaTime) > 0.4f)
		{
			CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360), false);
			AudioManager.Instance.PlayOneShot(deathSound, base.gameObject);
			base.gameObject.Recycle();
		}
	}

	private void FixedUpdate()
	{
		rb2D.MovePosition(rb2D.position + DirectionalSpeed * Time.fixedDeltaTime);
	}

	private void BounceMe()
	{
		ZSpeed += 0.5f * GameManager.DeltaTime;
		Z += ZSpeed * Time.deltaTime;
		ScaleX.y += (1f - ScaleX.x) * 0.3f;
		ScaleX.x += (ScaleX.y *= 0.8f);
		ScaleY.y += (1f - ScaleY.x) * 0.3f;
		ScaleY.x += (ScaleY.y *= 0.8f);
		ObjectToRotate.transform.parent.transform.parent.localPosition = Vector3.forward * Z;
		ObjectToRotate.transform.parent.transform.parent.localScale = new Vector3(ScaleX.x, ScaleY.y, 1f);
	}

	private bool OnGround(Vector3 Position)
	{
		LayerMask layerMask = LayerMask.GetMask("Island");
		RaycastHit hitInfo;
		if (Physics.Raycast(Position, Vector3.forward, out hitInfo, 10f, layerMask))
		{
			return true;
		}
		return false;
	}
}
