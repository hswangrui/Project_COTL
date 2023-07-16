using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleAnimator : BaseMonoBehaviour
{
	public bool reverseFacing;

	private float xScale = 1f;

	private float yScale = 1f;

	private float xScaleSpeed;

	private float yScaleSpeed;

	private bool flipX;

	private float moveSquish;

	private float moveSquishDelta;

	private SpriteRenderer spriterenderer;

	public Sprite IdleFrame;

	public Sprite AttackFrame;

	public Sprite PostAttackFrame;

	public Sprite PreAttackFrame;

	public Sprite DefendingFrame;

	public Sprite DodgingFrame;

	public Sprite FleeingFrame;

	public Sprite CustomAction0Frame;

	public Sprite RaiseAlarmFrame;

	public Sprite WorshippingFrame;

	public Sprite SleepingFrame;

	public Sprite InactiveFrame;

	public Sprite CustomAnimationFrame;

	private StateMachine state;

	private StateMachine.State prevState;

	public bool LookAtCamera;

	private Vector3 prevPosition;

	private void Start()
	{
		state = base.gameObject.GetComponentInParent<StateMachine>();
		spriterenderer = GetComponent<SpriteRenderer>();
		prevPosition = base.gameObject.transform.position;
	}

	private void Update()
	{
		if (LookAtCamera)
		{
			base.transform.rotation = Quaternion.Euler(Utils.GetAngle(base.transform.position, Camera.main.transform.position) + 90f, 0f, 0f);
		}
		if (state != null)
		{
			if (prevState != state.CURRENT_STATE)
			{
				base.transform.localPosition = Vector3.zero;
				base.transform.eulerAngles = new Vector3(-45f, 0f, 0f);
				switch (state.CURRENT_STATE)
				{
				case StateMachine.State.Idle:
					moveSquish = 0f;
					spriterenderer.sprite = IdleFrame;
					break;
				case StateMachine.State.Attacking:
					moveSquish = 0f;
					spriterenderer.sprite = AttackFrame;
					break;
				case StateMachine.State.Moving:
					SetScale(1.2f, 0.8f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = IdleFrame;
					break;
				case StateMachine.State.SignPostAttack:
					SetScale(0.7f, 2f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = PreAttackFrame;
					break;
				case StateMachine.State.RecoverFromAttack:
					SetScale(2f, 0.6f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = PostAttackFrame;
					break;
				case StateMachine.State.Defending:
					SetScale(2f, 0.6f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = DefendingFrame;
					break;
				case StateMachine.State.Dodging:
					base.transform.localPosition = new Vector3(0f, 0f, -0.2f);
					SetScale(2f, 0.6f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = DodgingFrame;
					break;
				case StateMachine.State.Fleeing:
					SetScale(0.8f, 1.2f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = FleeingFrame;
					break;
				case StateMachine.State.CustomAction0:
					SetScale(0.8f, 1.2f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = CustomAction0Frame;
					break;
				case StateMachine.State.RaiseAlarm:
					SetScale(0.8f, 1.2f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = RaiseAlarmFrame;
					break;
				case StateMachine.State.Worshipping:
					SetScale(0.8f, 1.2f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = WorshippingFrame;
					break;
				case StateMachine.State.Sleeping:
					SetScale(0.8f, 1.2f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = SleepingFrame;
					break;
				case StateMachine.State.InActive:
					SetScale(0.8f, 1.2f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = InactiveFrame;
					break;
				case StateMachine.State.CustomAnimation:
					SetScale(0.8f, 1.2f);
					moveSquishDelta = 0f;
					spriterenderer.sprite = CustomAnimationFrame;
					break;
				}
			}
			switch (state.CURRENT_STATE)
			{
			case StateMachine.State.Moving:
				moveSquish = 0.05f * Mathf.Cos(moveSquishDelta += 0.4f);
				break;
			case StateMachine.State.Fleeing:
				moveSquish = 0.05f * Mathf.Cos(moveSquishDelta += 0.4f);
				break;
			case StateMachine.State.Dodging:
				base.transform.Rotate(new Vector3(0f, 0f, 20 * (flipX ? 1 : (-1))));
				break;
			case StateMachine.State.Sleeping:
				moveSquish = 0.1f * Mathf.Cos(moveSquishDelta += 0.01f);
				break;
			}
		}
		prevState = state.CURRENT_STATE;
		setFacing(((!(state.facingAngle < 90f) || !(state.facingAngle > -90f)) ? 1 : (-1)) * ((!reverseFacing) ? 1 : (-1)));
		xScaleSpeed += (1f + moveSquish - xScale) * 0.2f;
		xScale += (xScaleSpeed *= 0.8f);
		yScaleSpeed += (1f - moveSquish - yScale) * 0.2f;
		yScale += (yScaleSpeed *= 0.8f);
		Vector3 localScale = new Vector3(xScale * (float)((!flipX) ? 1 : (-1)), yScale, 1f);
		base.gameObject.transform.localScale = localScale;
		if (Input.GetMouseButtonDown(1))
		{
			SetScale(2f, 0.5f);
		}
	}

	public void setFacing(int dir)
	{
		if (dir == 1 != flipX)
		{
			flipX = dir == 1;
			SetScale(1.2f, 0.8f);
		}
	}

	public void SetScale(float _xScale, float _yScale)
	{
		xScale = _xScale;
		yScale = _yScale;
	}
}
