using UnityEngine.UI;

public class BaseTimer : BaseMonoBehaviour
{
	public delegate void BaseTimeComplete();

	public float BaseTime = 5f;

	public float Timer;

	public float Progress;

	public static BaseTimer Instance;

	public Image image;

	public event BaseTimeComplete OnBaseTimeComplete;

	private void OnEnable()
	{
		Instance = this;
		Progress = 0.5f;
		Timer = BaseTime * Progress;
	}

	private void Update()
	{
	}
}
