using UnityEngine;

[RequireComponent(typeof(WorkPlace))]
public class Interaction_JobAssign : Interaction
{
	public string Name;

	public string Description;

	private WorkPlace workPlace;

	private void Start()
	{
		workPlace = GetComponent<WorkPlace>();
	}

	public override void GetLabel()
	{
		if (DataManager.Instance.Followers.Count >= 1 && workPlace != null)
		{
			bool flag = Worshipper.GetWorshipperByJobID(workPlace.ID) != null;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (base.Label != "")
		{
			GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasMenuList>().JobAssignMenu.GetComponent<JobAssignMenu>().Show(Name, Description, workPlace);
		}
	}
}
