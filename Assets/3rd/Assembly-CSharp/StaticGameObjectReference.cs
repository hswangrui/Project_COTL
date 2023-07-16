using UnityEngine;

public class StaticGameObjectReference : BaseMonoBehaviour
{
	public BaseMonoBehaviour BaseMonoBehaviour;

	public Collider2D collider2D;

	public static StaticGameObjectReference Instance;

	private void OnEnable()
	{
		Instance = this;
	}

	public static void EnableCollider2D()
	{
		Instance.collider2D.enabled = true;
	}

	public static void DisableCollider2D()
	{
		Instance.collider2D.enabled = false;
	}

	public static void EnableBaseMonoBehaviour()
	{
		Instance.BaseMonoBehaviour.enabled = true;
	}

	public static void DisableBaseMonoBehaviour()
	{
		Instance.BaseMonoBehaviour.enabled = false;
	}
}
