using System;
using System.Collections;
using BlendModes;
using I2.Loc;
using Lamb.UI;
using MMBiomeGeneration;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HUD_DisplayName : BaseMonoBehaviour
{
	public enum Positions
	{
		BottomRight,
		Centre
	}

	public enum textBlendMode
	{
		Normal,
		FrogBoss,
		DungeonFinal
	}

	private static HUD_DisplayName Instance;

	private TextMeshProUGUI Text_English;

	private TextMeshProUGUI Text_Korean;

	private TextMeshProUGUI Text_ChineseTraditional;

	private TextMeshProUGUI Text_ChineseSimplified;

	private TextMeshProUGUI Text_Russian;

	private TextMeshProUGUI Text_Japanese;

	public GameObject Text_English_GO;

	private GameObject Text_Korean_GO;

	private GameObject Text_ChineseTraditional_GO;

	private GameObject Text_ChineseSimplified_GO;

	private GameObject Text_Russian_GO;

	private GameObject Text_Japanese_GO;

	public bool DarkMode;

	public BlendModeEffect blendMode;

	public CanvasGroup canvasGroup;

	private Positions Position;

	private string _localizationKey;

	private Canvas canvas;

	private void Awake()
	{
		CreateObjectForLanguage();
	}

	private void CreateObjectForLanguage()
	{
		switch (LocalizationManager.CurrentLanguage)
		{
		case "English":
			if (Text_English_GO == null)
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Resources_moved/Prefabs/UI/DisplayNames/English.prefab", base.transform);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> result)
				{
					Text_English_GO = result.Result;
				};
			}
			return;
		case "Japanese":
			if (Text_Japanese_GO == null)
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Resources_moved/Prefabs/UI/DisplayNames/Japanese.prefab", base.transform);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> result)
				{
					Text_Japanese_GO = result.Result;
				};
			}
			return;
		case "Russian":
			if (Text_Russian_GO == null)
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Resources_moved/Prefabs/UI/DisplayNames/Russian.prefab", base.transform);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> result)
				{
					Text_Russian_GO = result.Result;
				};
			}
			return;
		case "Chinese (Simplified)":
			if (Text_ChineseSimplified_GO == null)
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Resources_moved/Prefabs/UI/DisplayNames/Chinese-Simplified.prefab", base.transform);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> result)
				{
					Text_ChineseSimplified_GO = result.Result;
				};
			}
			return;
		case "Chinese (Traditional)":
			if (Text_ChineseTraditional_GO == null)
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Resources_moved/Prefabs/UI/DisplayNames/Chinese-Traditional.prefab", base.transform);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> result)
				{
					Text_ChineseTraditional_GO = result.Result;
				};
			}
			return;
		case "Korean":
			if (Text_Korean_GO == null)
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Resources_moved/Prefabs/UI/DisplayNames/Korean.prefab", base.transform);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> result)
				{
					Text_Korean_GO = result.Result;
				};
			}
			return;
		}
		if (Text_English_GO == null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Resources_moved/Prefabs/UI/DisplayNames/English.prefab", base.transform);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> result)
			{
				Text_English_GO = result.Result;
			};
		}
	}

	private TextMeshProUGUI GetTextLanguage()
	{
		switch (LocalizationManager.CurrentLanguage)
		{
		case "English":
			if (Text_English_GO != null)
			{
				return Text_English_GO.GetComponentInChildren<TextMeshProUGUI>(true);
			}
			return null;
		case "Japanese":
			if (Text_Japanese_GO != null)
			{
				return Text_Japanese_GO.GetComponentInChildren<TextMeshProUGUI>(true);
			}
			return null;
		case "Russian":
			if (Text_Russian_GO != null)
			{
				return Text_Russian_GO.GetComponentInChildren<TextMeshProUGUI>(true);
			}
			return null;
		case "Chinese (Simplified)":
			if (Text_ChineseSimplified_GO != null)
			{
				return Text_ChineseSimplified_GO.GetComponentInChildren<TextMeshProUGUI>(true);
			}
			return null;
		case "Chinese (Traditional)":
			if (Text_ChineseTraditional_GO != null)
			{
				return Text_ChineseTraditional_GO.GetComponentInChildren<TextMeshProUGUI>(true);
			}
			return null;
		case "Korean":
			if (Text_Korean_GO != null)
			{
				return Text_Korean_GO.GetComponentInChildren<TextMeshProUGUI>(true);
			}
			return null;
		default:
			if (Text_English_GO != null)
			{
				return Text_English_GO.GetComponentInChildren<TextMeshProUGUI>(true);
			}
			return null;
		}
	}

	public static void Play(string Name, int Delay = 5, Positions Position = Positions.BottomRight, textBlendMode blend = textBlendMode.Normal)
	{
		if (Instance == null)
		{
			Instance = UnityEngine.Object.FindObjectOfType<HUD_DisplayName>();
		}
		Instance.StartCoroutine(Instance.YieldForText(delegate
		{
			TextMeshProUGUI textLanguage = Instance.GetTextLanguage();
			if (!(textLanguage == null))
			{
				textLanguage.alpha = 0f;
				Instance.canvasGroup.alpha = 0f;
				Instance._localizationKey = Name;
				Name = LocalizationManager.Sources[0].GetTranslation(Instance._localizationKey);
				if (blend == textBlendMode.DungeonFinal)
				{
					textLanguage.color = Color.red;
					textLanguage.GetComponent<BlendModeEffect>().BlendMode = BlendMode.Darken;
				}
				else if (blend == textBlendMode.FrogBoss)
				{
					textLanguage.color = Color.red;
					textLanguage.GetComponent<BlendModeEffect>().BlendMode = BlendMode.Divide;
				}
				else
				{
					textLanguage.color = new Color(0f, 0.41f, 0.8f, 1f);
					textLanguage.GetComponent<BlendModeEffect>().BlendMode = BlendMode.Divide;
				}
				Instance.Show(Name, (float)Delay / 1.5f, Position);
			}
		}));
	}

	private IEnumerator YieldForText(Action andThen)
	{
		while (Instance.GetTextLanguage() == null)
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		if (andThen != null)
		{
			andThen();
		}
	}

	public static void PlayTranslatedText(string Name, int Delay = 5, Positions Position = Positions.BottomRight)
	{
		if (Instance == null)
		{
			Instance = UnityEngine.Object.FindObjectOfType<HUD_DisplayName>();
		}
		Instance.Show(Name, Delay, Position);
	}

	public static void Play(string Name, float Delay, bool DarkMode = false)
	{
		Instance.Show(Name, Delay);
		Instance.DarkMode = DarkMode;
	}

	private void DisableTexts()
	{
		if (Text_English_GO != null)
		{
			Text_English_GO.SetActive(false);
		}
		if (Text_Korean_GO != null)
		{
			Text_Korean_GO.SetActive(false);
		}
		if (Text_ChineseTraditional_GO != null)
		{
			Text_ChineseTraditional_GO.SetActive(false);
		}
		if (Text_ChineseSimplified_GO != null)
		{
			Text_ChineseSimplified_GO.SetActive(false);
		}
		if (Text_Russian_GO != null)
		{
			Text_Russian_GO.SetActive(false);
		}
		if (Text_Japanese_GO != null)
		{
			Text_Japanese_GO.SetActive(false);
		}
	}

	private void Start()
	{
		DisableTexts();
		if (blendMode == null)
		{
			blendMode = GetComponentInChildren<BlendModeEffect>();
		}
	}

	public void ShowAndTranslate(string Name, float Delay)
	{
		Show(LocalizationManager.Sources[0].GetTranslation(Name), Delay);
	}

	public void Show(string Name, float Delay, Positions Position = Positions.BottomRight, bool waitForUI = false)
	{
		Instance.StartCoroutine(Instance.YieldForText(delegate
		{
			TextMeshProUGUI textLanguage = GetTextLanguage();
			if (!(textLanguage == null))
			{
				switch (Position)
				{
				case Positions.BottomRight:
					textLanguage.fontSize = 125f;
					textLanguage.rectTransform.anchoredPosition = new Vector3(-960f, 91f) * canvas.scaleFactor;
					textLanguage.alignment = TextAlignmentOptions.Right;
					break;
				case Positions.Centre:
					textLanguage.fontSize = 150f;
					textLanguage.rectTransform.localPosition = new Vector3(0f, 0f) * canvas.scaleFactor;
					textLanguage.alignment = TextAlignmentOptions.Center;
					break;
				}
				textLanguage.text = Name;
				StopAllCoroutines();
				StartCoroutine(ShowText(Delay, waitForUI));
			}
		}));
	}

	private IEnumerator ShowText(float Delay, bool waitForUI = false)
	{
		while (GetTextLanguage() == null)
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		TextMeshProUGUI Text = GetTextLanguage();
		yield return new WaitForEndOfFrame();
		if (waitForUI)
		{
			if (LetterBox.IsPlaying)
			{
				yield return null;
			}
			if (UIMenuBase.ActiveMenus.Count > 0)
			{
				yield return null;
			}
		}
		if (Time.timeScale != 0f && Text.text != "")
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/area_text_intro", base.gameObject);
		}
		canvasGroup.alpha = 1f;
		Instance.canvasGroup.alpha = 1f;
		Text.gameObject.SetActive(true);
		Text.alpha = 0f;
		float Progress2 = 0f;
		while (true)
		{
			float num;
			Progress2 = (num = Progress2 + Time.deltaTime);
			if (!(num <= 1f))
			{
				break;
			}
			Text.alpha = Mathf.Lerp(0f, 1f, Progress2);
			yield return null;
		}
		yield return new WaitForSeconds(Delay);
		Progress2 = 0f;
		while (true)
		{
			float num;
			Progress2 = (num = Progress2 + Time.deltaTime);
			if (!(num <= 1f))
			{
				break;
			}
			Text.alpha = Mathf.Lerp(1f, 0f, Progress2);
			yield return null;
		}
		canvasGroup.alpha = 0f;
		Text.text = "";
		Text.gameObject.SetActive(false);
	}

	private void Hide()
	{
		TextMeshProUGUI textLanguage = GetTextLanguage();
		StopAllCoroutines();
		canvasGroup.alpha = 0f;
		if (textLanguage != null)
		{
			textLanguage.text = "";
		}
	}

	public void HideText()
	{
		TextMeshProUGUI textLanguage = GetTextLanguage();
		StopAllCoroutines();
		if (textLanguage != null)
		{
			textLanguage.text = "";
			textLanguage.gameObject.SetActive(false);
		}
		canvasGroup.alpha = 0f;
	}

	private void OnEnable()
	{
		Instance = this;
		BiomeGenerator.OnBiomeChangeRoom += Hide;
		canvas = GetComponentInParent<Canvas>();
		LocalizationManager.OnLocalizeEvent += OnLocalize;
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDay));
	}

	private void OnNewDay()
	{
		if (!GameManager.IsDungeon(PlayerFarming.Location))
		{
			if (Instance == null)
			{
				Instance = UnityEngine.Object.FindObjectOfType<HUD_DisplayName>();
			}
			Instance._localizationKey = "UI/DayNumber";
			string translation = LocalizationManager.Sources[0].GetTranslation(Instance._localizationKey);
			translation = translation.Replace("{0}", TimeManager.CurrentDay.ToString());
			Instance.Show(translation, 3f, Positions.Centre);
		}
	}

	private void OnDisable()
	{
		BiomeGenerator.OnBiomeChangeRoom -= Hide;
		Instance = null;
		LocalizationManager.OnLocalizeEvent -= OnLocalize;
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDay));
	}

	private void OnLocalize()
	{
		DisableTexts();
		CreateObjectForLanguage();
		TextMeshProUGUI textLanguage = GetTextLanguage();
		if (textLanguage == null)
		{
			Debug.Log("Text is null!");
			return;
		}
		textLanguage.alpha = 1f;
		textLanguage.gameObject.SetActive(true);
		textLanguage.fontSize = 150f;
		textLanguage.rectTransform.localPosition = new Vector3(0f, 0f) * canvas.scaleFactor;
		textLanguage.alignment = TextAlignmentOptions.Center;
		if (Instance != null)
		{
			if (Instance._localizationKey != "UI/DayNumber")
			{
				textLanguage.text = LocalizationManager.Sources[0].GetTranslation(Instance._localizationKey);
				return;
			}
			string translation = LocalizationManager.Sources[0].GetTranslation(Instance._localizationKey);
			translation = translation.Replace("{0}", TimeManager.CurrentDay.ToString());
			textLanguage.text = translation;
		}
	}
}
