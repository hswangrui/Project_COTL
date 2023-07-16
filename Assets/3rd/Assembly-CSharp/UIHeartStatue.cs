using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHeartStatue : BaseMonoBehaviour
{
	[HideInInspector]
	public bool Upgrading;

	public RectTransform Bar;

	public List<GameObject> DiplayIcons = new List<GameObject>();

	public List<GameObject> LockIcons = new List<GameObject>();

	public static UIHeartStatue Instance;

	public GameObject canAffordButton;

	public GameObject cantAffordButton;

	public bool usesSlider;

	public Slider rangeSlider;

	public UISlideIndicator[] sliderIndicators;

	public int[] followerPrices;

	public Material normalMaterial;

	public Material BlackAndWhiteMaterial;

	public TextMeshProUGUI FollowerCount;

	private int lastPrice;

	private void OnEnable()
	{
		Instance = this;
		UpdateFollowerCount();
		FollowerManager.OnFollowerAdded = (FollowerManager.FollowerChanged)Delegate.Combine(FollowerManager.OnFollowerAdded, new FollowerManager.FollowerChanged(OnFollowerUpdated));
		FollowerManager.OnFollowerRemoved = (FollowerManager.FollowerChanged)Delegate.Combine(FollowerManager.OnFollowerRemoved, new FollowerManager.FollowerChanged(OnFollowerUpdated));
	}

	private void OnDisable()
	{
		Instance = null;
		FollowerManager.OnFollowerAdded = (FollowerManager.FollowerChanged)Delegate.Remove(FollowerManager.OnFollowerAdded, new FollowerManager.FollowerChanged(OnFollowerUpdated));
		FollowerManager.OnFollowerRemoved = (FollowerManager.FollowerChanged)Delegate.Remove(FollowerManager.OnFollowerRemoved, new FollowerManager.FollowerChanged(OnFollowerUpdated));
	}

	private void OnFollowerUpdated(int followerID)
	{
		UpdateSpiritHearts();
	}

	private void Start()
	{
		SetBarScale();
		UpdateDisplayIcons();
		UpdateFollowerCount();
		if (DataManager.Instance.ShrineHeart >= 4)
		{
			HideButton();
		}
	}

	public virtual bool CanUpgrade()
	{
		return DataManager.Instance.ShrineHeart < 4;
	}

	public virtual void Repair()
	{
		DataManager.Instance.ShrineHeart = 1;
	}

	private void UpdateFollowerCount()
	{
		if (!(FollowerCount != null))
		{
			return;
		}
		if (rangeSlider != null)
		{
			lastPrice = followerPrices.Length - 1;
			rangeSlider.minValue = 0f;
			rangeSlider.maxValue = 100f;
			for (int i = 0; i <= lastPrice; i++)
			{
				sliderIndicators[i].setPrice(followerPrices[i]);
			}
		}
		string translation = LocalizationManager.Sources[0].GetTranslation("UI/Generic/Followers");
		string text = DataManager.Instance.Followers.Count.ToString();
		string text2 = "<sprite name=\"icon_Followers\">" + text + " " + translation;
		FollowerCount.text = text2;
	}

	public void SetBarScale()
	{
		if (usesSlider)
		{
			if (rangeSlider == null)
			{
				return;
			}
			float num = 0f;
			int num2 = -1;
			float num3 = 0f;
			while (++num2 < followerPrices.Length)
			{
				if (DataManager.Instance.Followers.Count < followerPrices[num2])
				{
					float num4 = (float)DataManager.Instance.Followers.Count - num3;
					float num5 = (float)followerPrices[num2] - num3;
					if (num4 > 0f)
					{
						num += num4 / num5 * (float)(100 / (followerPrices.Length - 1));
					}
				}
				else if (followerPrices[num2] > 0)
				{
					num += (float)(100 / (followerPrices.Length - 1));
				}
				num3 = followerPrices[num2];
			}
			rangeSlider.value = num;
		}
		else
		{
			float value = ((float)DataManager.Instance.Followers.Count - 1f) / 6f;
			Bar.localScale = new Vector3(Mathf.Clamp(value, 0f, 1f), 1f, 1f);
		}
	}

	public virtual void Upgrade()
	{
		if (DataManager.Instance.ShrineHeart < 4 && !Upgrading)
		{
			Debug.Log("Upgrade!");
			StartCoroutine(DoUpgrade());
		}
	}

	public virtual IEnumerator DoUpgrade()
	{
		Upgrading = true;
		HideButton();
		DataManager.Instance.ShrineHeart = Mathf.Min(++DataManager.Instance.ShrineHeart, 4);
		Debug.Log("DataManager.Instance.ShrineHeart  " + DataManager.Instance.ShrineHeart);
		UpdateSpiritHearts();
		yield return new WaitForSeconds(0.3f);
		if (DataManager.Instance.ShrineHeart < 4)
		{
			ShowButton();
		}
		Upgrading = false;
	}

	public static void UpdateSpiritHearts()
	{
		HealthPlayer healthPlayer = UnityEngine.Object.FindObjectOfType<HealthPlayer>();
		if (healthPlayer == null)
		{
			return;
		}
		int count = DataManager.Instance.Followers.Count;
		int num = 0;
		switch (DataManager.Instance.ShrineHeart)
		{
		case 0:
			num = 0;
			break;
		case 1:
			if (count > 1)
			{
				num = 2;
			}
			else if (count <= 1)
			{
				num = 0;
			}
			break;
		case 2:
			if (count > 1 && count <= 4)
			{
				num = 2;
			}
			else if (count > 4)
			{
				num = 4;
			}
			else if (count <= 1)
			{
				num = 0;
			}
			break;
		case 3:
			if (count > 1 && count <= 4)
			{
				num = 2;
			}
			else if (count > 4 && count <= 9)
			{
				num = 4;
			}
			else if (count > 9)
			{
				num = 6;
			}
			else if (count <= 1)
			{
				num = 0;
			}
			break;
		case 4:
			if (count > 1 && count <= 4)
			{
				num = 2;
			}
			else if (count > 4 && count <= 9)
			{
				num = 4;
			}
			else if (count > 9 && count <= 14)
			{
				num = 6;
			}
			else if (count > 14)
			{
				num = 8;
			}
			else if (count <= 1)
			{
				num = 0;
			}
			break;
		}
		healthPlayer.TotalSpiritHearts = num;
		healthPlayer.SpiritHearts = healthPlayer.TotalSpiritHearts;
		UIHeartStatue instance = Instance;
		if ((object)instance != null)
		{
			instance.UpdateDisplayIcons();
		}
	}

	public virtual void UpdateDisplayIcons()
	{
		if (!(Instance != null))
		{
			return;
		}
		int num = -1;
		while (++num < Instance.DiplayIcons.Count)
		{
			if ((float)num < DataManager.Instance.PLAYER_SPIRIT_TOTAL_HEARTS / 2f)
			{
				Instance.DiplayIcons[num].GetComponent<Image>().material = normalMaterial;
			}
			else
			{
				Instance.DiplayIcons[num].GetComponent<Image>().material = BlackAndWhiteMaterial;
			}
		}
		if (Instance.LockIcons == null)
		{
			return;
		}
		num = -1;
		while (++num < Instance.LockIcons.Count)
		{
			if (num < DataManager.Instance.ShrineHeart)
			{
				LockIcons[num].SetActive(false);
			}
			else
			{
				LockIcons[num].SetActive(true);
			}
		}
	}

	public void ShowButton()
	{
		canAffordButton.SetActive(true);
		cantAffordButton.SetActive(false);
	}

	public void HideButton()
	{
		canAffordButton.SetActive(false);
		cantAffordButton.SetActive(true);
	}
}
