using UnityEngine;

[RequireComponent(typeof(Dwelling))]
public class Interaction_DwellingAssign : Interaction
{
	public string Name;

	public string Description;

	private Dwelling dwelling;

	private void Start()
	{
		dwelling = GetComponent<Dwelling>();
	}

	public override void GetLabel()
	{
		if (DataManager.Instance.Followers.Count < 1)
		{
			base.Label = "";
		}
		else if (dwelling != null)
		{
			bool flag = (Object)null != (Object)null;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (base.Label != "")
		{
			GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasMenuList>().DwellingAssignMenu.GetComponent<DwellingAssignMenu>().Show(Name, Description, dwelling);
		}
	}
}
