using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAmmo : MonoBehaviour
{
	[Serializable]
	public struct BiomeColour
	{
		public Color Color;

		public FollowerLocation Location;
	}

	public Sprite AmmoSprite;

	public Sprite EmptySprite;

	public List<Image> Images = new List<Image>();

	public CanvasGroup CanvasGroup;

	public List<BiomeColour> Colours;

	private Color color;

	private float ShakeDuration = 1f;

	private float ShakeX = 0.1f;

	private void OnEnable()
	{
		FaithAmmo.OnAmmoCountChanged = (Action)Delegate.Combine(FaithAmmo.OnAmmoCountChanged, new Action(AmmoChanged));
		FaithAmmo.OnCantAfford = (Action)Delegate.Combine(FaithAmmo.OnCantAfford, new Action(CantAfford));
	}

	private void Start()
	{
		CanvasGroup.DOKill();
		CanvasGroup.alpha = 0f;
		foreach (BiomeColour colour in Colours)
		{
			if (colour.Location == PlayerFarming.Location)
			{
				color = colour.Color;
				break;
			}
		}
		foreach (Image image in Images)
		{
			image.color = color;
		}
	}

	private void OnDisable()
	{
		FaithAmmo.OnAmmoCountChanged = (Action)Delegate.Remove(FaithAmmo.OnAmmoCountChanged, new Action(AmmoChanged));
		FaithAmmo.OnCantAfford = (Action)Delegate.Remove(FaithAmmo.OnCantAfford, new Action(CantAfford));
	}

	private void CantAfford()
	{
		base.transform.DOShakePosition(ShakeDuration, new Vector3(ShakeX, 0f), 10, 0f);
		AmmoChanged();
	}

	private void AmmoChanged()
	{
		StopAllCoroutines();
		CanvasGroup.DOKill();
		CanvasGroup.DOFade(1f, 0.1f);
		int num = -1;
		while (++num < Images.Count)
		{
			Images[num].gameObject.SetActive(num < Mathf.FloorToInt(FaithAmmo.Total / (float)PlayerSpells.AmmoCost));
			if (num < Mathf.FloorToInt(FaithAmmo.Ammo / (float)PlayerSpells.AmmoCost))
			{
				if (Images[num].sprite == EmptySprite)
				{
					Images[num].transform.DOKill();
					Images[num].transform.DOPunchScale(new Vector3(0.1f, 0.1f), 1f);
				}
				Images[num].sprite = AmmoSprite;
				Images[num].color = color;
				continue;
			}
			if (Images[num].sprite == AmmoSprite)
			{
				Images[num].transform.DOKill();
				Images[num].transform.localScale = Vector3.one * 2f;
				Images[num].transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
			}
			Images[num].sprite = EmptySprite;
			Images[num].color = Color.white;
		}
		StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		yield return new WaitForSeconds(3f);
		CanvasGroup.DOKill();
		CanvasGroup.DOFade(0f, 1f);
	}

	private void GetAmmoSprites()
	{
		Images = new List<Image>(GetComponentsInChildren<Image>());
	}
}
