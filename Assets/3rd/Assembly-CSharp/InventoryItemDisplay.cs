using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDisplay : BaseMonoBehaviour
{
	[Serializable]
	public class MyDictionaryEntry
	{
		public InventoryItem.ITEM_TYPE key;

		public Sprite value;
	}

	public Dictionary<InventoryItem.ITEM_TYPE, Sprite> Images = new Dictionary<InventoryItem.ITEM_TYPE, Sprite>();

	[SerializeField]
	private List<MyDictionaryEntry> ItemImages;

	private Dictionary<InventoryItem.ITEM_TYPE, Sprite> myDictionary;

	public List<Sprite> DoctrineStoneSprites;

	private Sprite sprite;

	public SpriteRenderer spriteRenderer;

	public Image image;

	public Image outline;

	public List<MyDictionaryEntry> imgs
	{
		get
		{
			return ItemImages;
		}
	}

	private void Awake()
	{
		GetItemImages();
	}

	private void GetItemImages()
	{
		myDictionary = new Dictionary<InventoryItem.ITEM_TYPE, Sprite>();
		foreach (MyDictionaryEntry itemImage in ItemImages)
		{
			myDictionary.Add(itemImage.key, itemImage.value);
		}
	}

	private void Start()
	{
		if (image == null && spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
	}

	private void DoScale()
	{
		Transform obj = spriteRenderer.transform;
		obj.DOKill();
		obj.localScale = Vector3.zero;
		spriteRenderer.transform.DOScale(1f, 1f).SetUpdate(true).SetEase(Ease.OutQuart);
	}

	public void SetImage(Sprite sprite, bool doScale = true)
	{
		StopAllCoroutines();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
		{
			spriteRenderer.sprite = sprite;
			if (doScale)
			{
				DoScale();
			}
		}
		if (image != null)
		{
			image.enabled = true;
			image.sprite = null;
			image.preserveAspect = true;
			image.sprite = sprite;
		}
		if (outline != null)
		{
			outline.enabled = true;
			outline.sprite = null;
			outline.preserveAspect = true;
			outline.sprite = sprite;
		}
	}

	public void SetImage(InventoryItem.ITEM_TYPE Type, bool andScale = true)
	{
		if (myDictionary == null)
		{
			Awake();
		}
		StopAllCoroutines();
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
		{
			switch (Type)
			{
			case InventoryItem.ITEM_TYPE.NONE:
				spriteRenderer.sprite = null;
				break;
			case InventoryItem.ITEM_TYPE.DOCTRINE_STONE:
				spriteRenderer.sprite = DoctrineStoneSprites[Mathf.Clamp(DataManager.Instance.CompletedDoctrineStones, 0, DoctrineStoneSprites.Count - 1)];
				break;
			default:
				if (myDictionary.ContainsKey(Type))
				{
					spriteRenderer.sprite = myDictionary[Type];
				}
				break;
			}
			if (andScale && base.gameObject.activeInHierarchy)
			{
				DoScale();
			}
		}
		if (image != null)
		{
			image.enabled = true;
			image.sprite = null;
			image.preserveAspect = true;
			switch (Type)
			{
			case InventoryItem.ITEM_TYPE.NONE:
				image.enabled = false;
				break;
			case InventoryItem.ITEM_TYPE.DOCTRINE_STONE:
				image.sprite = DoctrineStoneSprites[DataManager.Instance.CompletedDoctrineStones];
				break;
			default:
				if (!myDictionary.ContainsKey(Type))
				{
					image.sprite = null;
					Debug.Log(Type);
				}
				else
				{
					image.sprite = myDictionary[Type];
				}
				break;
			}
		}
		if (outline != null)
		{
			outline.enabled = true;
			outline.sprite = null;
			outline.preserveAspect = true;
			switch (Type)
			{
			case InventoryItem.ITEM_TYPE.NONE:
				outline.enabled = false;
				break;
			case InventoryItem.ITEM_TYPE.DOCTRINE_STONE:
				outline.sprite = DoctrineStoneSprites[DataManager.Instance.CompletedDoctrineStones];
				break;
			default:
				outline.sprite = myDictionary[Type];
				break;
			}
		}
	}

	public Sprite GetImage(InventoryItem.ITEM_TYPE Type)
	{
		if (myDictionary == null)
		{
			GetItemImages();
		}
		if (Type == InventoryItem.ITEM_TYPE.DOCTRINE_STONE)
		{
			return DoctrineStoneSprites[DataManager.Instance.CompletedDoctrineStones];
		}
		Sprite value;
		if (myDictionary.TryGetValue(Type, out value))
		{
			return value;
		}
		return null;
	}
}
