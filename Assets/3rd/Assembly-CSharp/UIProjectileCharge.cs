using UnityEngine;
using UnityEngine.UI;

public class UIProjectileCharge : BaseMonoBehaviour
{
	[SerializeField]
	private Image bar;

	[SerializeField]
	private RectTransform target;

	[SerializeField]
	private Vector3 offset;

	private static UIProjectileCharge instance;

	private Camera camera;

	private Canvas canvas;

	public static void Play()
	{
		if (instance == null)
		{
			Canvas component = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
			instance = Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/UI Projectile Charge"), component.transform).GetComponent<UIProjectileCharge>();
			instance.bar.fillAmount = 0f;
			instance.camera = Camera.main;
			instance.canvas = component;
		}
	}

	public static void Hide()
	{
		if (instance != null)
		{
			instance.bar.fillAmount = 0f;
			Object.Destroy(instance.gameObject);
		}
	}

	private void Update()
	{
		if (PlayerFarming.Instance != null)
		{
			Vector3 position = camera.WorldToScreenPoint(PlayerFarming.Instance.transform.position) + offset * canvas.scaleFactor;
			base.transform.position = position;
		}
	}

	public static void UpdateBar(float fillAmount)
	{
		instance.bar.fillAmount = fillAmount;
	}

	public static bool CorrectRelease()
	{
		if (instance != null && instance.bar.fillAmount >= 0.62f)
		{
			return instance.bar.fillAmount <= 0.8f;
		}
		return false;
	}
}
