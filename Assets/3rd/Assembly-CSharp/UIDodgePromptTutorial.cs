using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class UIDodgePromptTutorial : BaseMonoBehaviour
{
	public RectTransform PromptRT;

	private SkeletonAnimation[] AttackerSpines;

	public Material NormalMaterial;

	public Material BW_Material;

	private Material enemyNormalMaterial;

	private Material enemyBWMaterial;

	public Shader stencilShader;

	public void Play(GameObject Attacker)
	{
		Time.timeScale = 0f;
		HUD_Manager.Instance.Hide(false, 0);
		PromptRT.localScale = Vector3.one * 0.5f;
		PromptRT.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.distance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.distance = x;
		}, 8f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 8f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
		AttackerSpines = Attacker.GetComponentsInChildren<SkeletonAnimation>();
		SkeletonAnimation[] attackerSpines = AttackerSpines;
		foreach (SkeletonAnimation skeletonAnimation in attackerSpines)
		{
			Debug.Log(skeletonAnimation.name);
			if (stencilShader == null)
			{
				return;
			}
			skeletonAnimation.gameObject.AddComponent<SkeletonRendererCustomMaterials>();
			skeletonAnimation.CustomMaterialOverride.Clear();
			enemyNormalMaterial = skeletonAnimation.GetComponent<MeshRenderer>().material;
			enemyBWMaterial = enemyNormalMaterial;
			enemyBWMaterial.shader = stencilShader;
			enemyBWMaterial.SetFloat("_StencilRef", 128f);
			enemyBWMaterial.SetFloat("_StencilOp", 2f);
			skeletonAnimation.CustomMaterialOverride.Add(enemyNormalMaterial, enemyBWMaterial);
			skeletonAnimation.CustomMaterialOverride[enemyNormalMaterial] = enemyBWMaterial;
		}
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(PlayerFarming.Instance.originalMaterial, PlayerFarming.Instance.BW_Material);
		HUD_Manager.Instance.ShowBW(0.33f, 0f, 1f);
		StartCoroutine(AwaitInputRoutine());
	}

	private IEnumerator AwaitInputRoutine()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		float Duration = 0f;
		while (true)
		{
			float num;
			Duration = (num = Duration + Time.unscaledDeltaTime);
			if (!(num < 3f) || Time.timeScale > 0f)
			{
				break;
			}
			if (InputManager.Gameplay.GetDodgeButtonHeld())
			{
				DataManager.Instance.ShownDodgeTutorial = true;
				break;
			}
			yield return null;
		}
		SkeletonAnimation[] attackerSpines = AttackerSpines;
		for (int i = 0; i < attackerSpines.Length; i++)
		{
			attackerSpines[i].CustomMaterialOverride.Clear();
		}
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		HUD_Manager.Instance.ShowBW(0.33f, 1f, 0f);
		GameManager.GetInstance().CameraResetTargetZoom();
		HUD_Manager.Instance.Show(0);
		PlayerFarming.Instance.DodgeQueued = true;
		Time.timeScale = 1f;
		DataManager.Instance.ShownDodgeTutorialCount++;
		Object.Destroy(base.gameObject);
	}
}
