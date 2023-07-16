using UnityEngine;

public class NonUILayoutElement : MonoBehaviour
{
	public NonUILayout ParentLayout;

	public bool IgnoreLayout { get; set; }

	private void Start()
	{
		if (ParentLayout == null)
		{
			ParentLayout = GetComponentInParent<NonUILayout>();
		}
	}

	private void OnDisable()
	{
		ParentLayout.RefreshElements();
	}

	private void OnEnable()
	{
		ParentLayout.RefreshElements();
	}

	private void OnDestroy()
	{
		ParentLayout.RefreshElements();
	}
}
