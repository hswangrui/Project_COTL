using UnityEngine;

public class CharacterBase : BaseMonoBehaviour
{
	private StateMachine statemachine;

	private float rotation;

	private float angle;

	public bool LockToGround;

	private SpriteRenderer spriteRender;

	public bool AffectedByQualitySettings = true;

	private void Start()
	{
		statemachine = GetComponentInParent<StateMachine>();
		if (AffectedByQualitySettings && QualitySettings.shadows != 0)
		{
			spriteRender = GetComponent<SpriteRenderer>();
			if (spriteRender != null)
			{
				spriteRender.enabled = false;
			}
		}
	}

	private void LateUpdate()
	{
		if (statemachine != null)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, statemachine.facingAngle);
			if (LockToGround)
			{
				base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
			}
		}
	}
}
