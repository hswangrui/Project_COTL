using src.UINavigator;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
	private float TimeHolder;

	private bool InMenuHolder;

	public void OnEnable()
	{
		TimeHolder = Time.timeScale;
		Time.timeScale = 0f;
		InMenuHolder = GameManager.InMenu;
		GameManager.InMenu = true;
		MonoSingleton<UINavigatorNew>.Instance.LockNavigation = true;
		MonoSingleton<UINavigatorNew>.Instance.LockInput = true;
	}

	public void OnDisable()
	{
		Time.timeScale = TimeHolder;
		GameManager.InMenu = InMenuHolder;
		MonoSingleton<UINavigatorNew>.Instance.LockNavigation = false;
		MonoSingleton<UINavigatorNew>.Instance.LockInput = false;
	}
}
