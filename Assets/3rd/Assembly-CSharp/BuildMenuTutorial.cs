using MMTools;
using Rewired;
using Unify.Input;
using UnityEngine;

public class BuildMenuTutorial : BaseMonoBehaviour
{
	private RectTransform rectTransform;

	public Transform Target;

	public Vector3 Offset = new Vector3(0f, 0f, -1f);

	public MMControlPrompt mmControlPrompt;

	private CanvasMenuList list;

	private Player player
	{
		get
		{
			return RewiredInputManager.MainPlayer;
		}
	}

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		SetPosition();
		list = GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasMenuList>();
	}

	private void Update()
	{
		SetPosition();
		if (player.GetButtonUp(mmControlPrompt.Action) && list.BuildingMenu.activeSelf)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void SetPosition()
	{
		Vector3 position = Target.position + Offset;
		position = Camera.main.WorldToScreenPoint(position);
		rectTransform.position = position;
	}
}
