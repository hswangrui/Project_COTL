using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class MegaSlash : BaseMonoBehaviour, ICurseProduct
{
	[SerializeField]
	private GameObject symbol;

	[SerializeField]
	private GameObject pivot;

	[SerializeField]
	private SpriteRenderer renderer;

	[SerializeField]
	private GameObject collider;

	[SerializeField]
	private float minScale = 0.3f;

	[SerializeField]
	private float maxScale = 0.75f;

	[SerializeField]
	private AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	[SerializeField]
	private MeshRenderer fireWallRenderer;

	private MeshRenderer fireWallLightingRenderer;

	[SerializeField]
	private AnimationCurve fireWallSpreadCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0.5f), new Keyframe(1f, 1f));

	[SerializeField]
	private MeshRenderer groundScorchRenderer;

	[SerializeField]
	private AnimationCurve groundScorchCleanupCurve = new AnimationCurve(new Keyframe(0.666f, 0f), new Keyframe(1f, 1f));

	[SerializeField]
	private float duration = 0.75f;

	private DamageCollider damageCollider;

	private void DefaultSizedButton()
	{
		Play(1f);
	}

	public void Play(float norm)
	{
		if (!(fireWallRenderer == null) && !(groundScorchRenderer == null))
		{
			damageCollider = collider.GetComponent<DamageCollider>();
			damageCollider.Damage = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).Damage * PlayerSpells.GetCurseDamageMultiplier();
			damageCollider.DestroyBullets = true;
			StopAllCoroutines();
			StartCoroutine(SlashRoutine(norm));
			Debug.Log("Size: " + norm);
		}
	}

	private IEnumerator SlashRoutine(float norm)
	{
		symbol.SetActive(true);
		pivot.SetActive(false);
		symbol.SetActive(false);
		pivot.SetActive(true);
		fireWallLightingRenderer = fireWallRenderer.transform.GetChild(0).GetComponent<MeshRenderer>();
		float timer = 0f;
		bool isActive = false;
		while (timer <= duration)
		{
			timer += Time.deltaTime;
			float num = Mathf.Clamp01(timer / duration);
			if ((double)num >= 0.5 && !isActive)
			{
				isActive = true;
				collider.SetActive(false);
			}
			else
			{
				damageCollider.TriggerCheckCollision();
			}
			float num2 = Mathf.Lerp(minScale, maxScale, norm);
			pivot.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 1f), new Vector3(num2, num2, 1f), scaleCurve.Evaluate(num));
			float b = Mathf.Lerp(0.35f, 1f, norm);
			groundScorchRenderer.material.SetFloat("_BurnPos", Mathf.Lerp(0f, b, scaleCurve.Evaluate(num)));
			groundScorchRenderer.material.SetFloat("_SpreadThreshold", fireWallSpreadCurve.Evaluate(num));
			groundScorchRenderer.material.SetFloat("_ScorchPos", groundScorchCleanupCurve.Evaluate(num));
			Vector2 vector = new Vector2(pivot.transform.localScale.x, pivot.transform.localScale.z) / new Vector2(1f, 1f);
			fireWallRenderer.material.SetVector("_NoiseInvScale", vector);
			fireWallRenderer.material.SetFloat("_SpreadThreshold", fireWallSpreadCurve.Evaluate(num));
			fireWallRenderer.material.SetFloat("_DeformHeight", num);
			fireWallLightingRenderer.material = fireWallRenderer.material;
			fireWallLightingRenderer.material.SetColor("_Color", Color.white);
			yield return null;
		}
		pivot.SetActive(false);
		Object.Destroy(base.gameObject);
	}
}
