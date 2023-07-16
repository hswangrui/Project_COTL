using System.Collections;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class UIWeaponCard : BaseMonoBehaviour
{
	public SkeletonGraphic Spine;

	public Material NormalMaterial;

	public Material RareMaterial;

	public Material SuperRareMaterial;

	public RectTransform SpineRT;

	public RectTransform Effects;

	public TextMeshProUGUI EffectText;

	public TextMeshProUGUI SubtitleText;

	public TextMeshProUGUI NameText;

	[SerializeField]
	private ParticleSystem _particleSystem;

	public CanvasGroup InformationBox;

	public bool CameFromPauseMenu { get; set; }

	private void Start()
	{
		Effects.localPosition = new Vector3(-550f, 0f);
		Spine.AnimationState.SetAnimation(0, "empty", false);
		Spine.AnimationState.Event += HandleSpineEvent;
	}

	public void Show(TarotCards.TarotCard Card)
	{
		_particleSystem.Stop();
		Spine.Skeleton.SetSkin(TarotCards.Skin(Card.CardType));
		Spine.AnimationState.AddAnimation(0, "static", true, 0f);
		SubtitleText.text = TarotCards.LocalisedLore(Card.CardType);
		if (SubtitleText.text == "")
		{
			SubtitleText.gameObject.SetActive(false);
		}
		EffectText.text = TarotCards.LocalisedDescription(Card.CardType, 0);
		NameText.text = TarotCards.LocalisedName(Card.CardType, 0);
		if (Card.UpgradeIndex == 0)
		{
			Spine.material = NormalMaterial;
			Debug.Log("Normal Tarot Card");
		}
		else if (Card.UpgradeIndex == 1)
		{
			Spine.material = RareMaterial;
			_particleSystem.Play();
			Debug.Log("Rare Tarot Card");
		}
		else if (Card.UpgradeIndex == 2)
		{
			Spine.material = SuperRareMaterial;
			_particleSystem.Play();
			Debug.Log("Super Rare Tarot Card");
		}
		else
		{
			Spine.material = NormalMaterial;
			Debug.Log("Upgrade Index not set: " + Card.UpgradeIndex);
		}
		StartCoroutine(RevealEffectDetails());
	}

	private void OnDisable()
	{
		if (Spine != null && Spine.AnimationState != null)
		{
			Spine.AnimationState.Event -= HandleSpineEvent;
		}
	}

	private void HandleSpineEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		string text = e.Data.Name;
		if (!(text == "Shake Screen"))
		{
			if (text == "reveal")
			{
				AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_reveal", base.gameObject);
			}
		}
		else
		{
			CameraManager.shakeCamera(Random.Range(0.15f, 0.2f), Random.Range(0, 360));
		}
	}

	public IEnumerator Play(TarotCards.TarotCard Card, Vector3 Position)
	{
		_particleSystem.Stop();
		Spine.enabled = false;
		string text = TarotCards.Skin(Card.CardType);
		Debug.Log(string.Format("CARD: {0}  {1}", Card, text));
		if (text != "")
		{
			Spine.Skeleton.SetSkin(text);
		}
		SubtitleText.text = TarotCards.LocalisedLore(Card.CardType);
		if (SubtitleText.text == "")
		{
			SubtitleText.gameObject.SetActive(false);
		}
		EffectText.text = TarotCards.LocalisedDescription(Card.CardType, Card.UpgradeIndex);
		NameText.text = TarotCards.LocalisedName(Card.CardType, Card.UpgradeIndex);
		if (Card.UpgradeIndex == 0)
		{
			Spine.material = NormalMaterial;
			Debug.Log("Normal Tarot Card");
		}
		else if (Card.UpgradeIndex == 1)
		{
			Spine.material = RareMaterial;
			_particleSystem.Play();
			Debug.Log("Rare Tarot Card");
		}
		else if (Card.UpgradeIndex == 2)
		{
			Spine.material = SuperRareMaterial;
			_particleSystem.Play();
			Debug.Log("Super Rare Tarot Card");
		}
		else
		{
			Spine.material = NormalMaterial;
			Debug.Log("Upgrade Index not set: " + Card.UpgradeIndex);
		}
		SpineRT.localPosition = Position;
		if (!CameFromPauseMenu || Time.timeScale == 0f)
		{
			Spine.AnimationState.SetAnimation(0, "reveal", false);
			Spine.AnimationState.AddAnimation(0, "static", true, 0f);
		}
		else
		{
			Spine.AnimationState.AddAnimation(0, "static", true, 0f);
		}
		Spine.enabled = true;
		yield return new WaitForEndOfFrame();
		Spine.transform.localScale = Vector3.one * 1.75f;
		if (!CameFromPauseMenu || Time.timeScale == 0f)
		{
			yield return new WaitForSecondsRealtime(0.5f);
		}
		if (!CameFromPauseMenu || Time.timeScale == 0f)
		{
			StartCoroutine(RevealEffectDetails());
		}
		else
		{
			Effects.localPosition = Vector3.zero;
		}
	}

	private IEnumerator RevealEffectDetails()
	{
		yield return new WaitForSecondsRealtime(0.2f);
		float Progress = 0f;
		Vector3 StartPosition = Effects.localPosition;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime * 2f);
			if (!(num <= 1f))
			{
				break;
			}
			Effects.localPosition = Vector3.Lerp(StartPosition, Vector3.zero, Mathf.SmoothStep(0f, 1f, Progress));
			yield return null;
		}
		Effects.localPosition = Vector3.zero;
	}
}
